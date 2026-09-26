//checkout.js
function CheckoutItemViewModel(data) {
    var self = this;

    self.displayName = ko.observable(data.displayName);
    self.quantity = ko.observable(data.quantity);
    self.rowPrice = ko.observable(data.rowPrice);
    self.merchId = ko.observable(data.summary.merchId);
    self.optionId = ko.observable(data.summary.optionId);

    self.formattedRowPrice = ko.pureComputed(function () {
        return utilities.formatCurrency(self.rowPrice());
    });
}

function CountryViewModel(data) {
    var self = this;

    self.name = ko.observable(data.displayText);
    self.code = ko.observable(data.code);
}

function DeliveryOptionsViewModel(text, value) {
    var self = this;

    self.text = ko.observable(text);
    self.value = ko.observable(value);
}

function CheckoutViewModel(countries) {
    var self = this;

    //Address Fields
    self.name = ko.observable("").extend({ required: true });
    self.address1 = ko.observable("").extend({ required: true });
    self.address2 = ko.observable("");
    self.town = ko.observable("").extend({ required: true });
    self.countryCode = ko.observable(null).extend({ required: true });
    self.postalCode = ko.observable(""); // paypal validation
    self.region = ko.observable("").extend({ required: true });
    self.countries = ko.observableArray([]);

    self.errors = ko.validation.group(self);

    //Cart
    self.items = ko.observableArray([]);
    self.subTotal = ko.observable(0);
    self.postage = ko.observable(0);
    self.total = ko.observable(0);

    for (var j = 0; j < countries.length; j++) {
        self.countries.push(new CountryViewModel(countries[j]));
    }

    self.deliveryOptions = ko.observableArray([new DeliveryOptionsViewModel("Standard Delivery", false), new DeliveryOptionsViewModel("Tracked Delivery (+£4.00)", true)]);


    self.specialDelivery = ko.observable(false);

    self.showPostage = ko.pureComputed(function () {
        return self.countryCode() !== null;
    });

    self.formattedSubtotal = ko.pureComputed(function () {
        return utilities.formatCurrency(self.subTotal());
    });

    self.formattedPostage = ko.pureComputed(function () {
        return utilities.formatCurrency(self.postage());
    });

    self.formattedTotal = ko.pureComputed(function () {
        return utilities.formatCurrency(self.total());
    });

    function populateCheckout() {
        var cart = readCartFromCookie();
        var hasCountry = self.countryCode() !== null;
        if (cart) {
            ajax.post(hasCountry ? "/Checkout/GetDetails" : "/Basket/GetDetails",
                {
                    specialDelivery: self.specialDelivery(),
                    countryCode: self.countryCode(),
                    items: cart
                }).done(function (data) {
                    self.items([]);
                    for (var i = 0; i < data.items.length; i++) {
                        self.items.push(new CheckoutItemViewModel(data.items[i]));
                    }

                    self.subTotal(data.subTotal);
                    if (hasCountry) {
                        self.postage(data.postage);
                        self.total(data.total);
                    }

            });
        } else {
            self.items([]);
        }
    }

    self.processing = ko.observable(false);
    self.payWithPaypal = function () {
        $("#zip ~ .text-danger").remove();
        //validate
        if (self.errors().length > 0) {
            self.errors.showAllMessages();
            return;
        }
        var cart = readCartFromCookie();
        if (cart) {
            self.processing(true);
            ajax.post("/Checkout/Pay", {
                summary: {
                    specialDelivery: self.specialDelivery(),
                    countryCode: self.countryCode(),
                    items: cart
                },
                address: {
                    name: self.name(),
                    address1: self.address1(),
                    address2: self.address2(),
                    town: self.town(),
                    countryCode: self.countryCode(),
                    postalCode: self.postalCode(),
                    region: self.region()
                },
                __RequestVerificationToken: document.getElementsByName("__RequestVerificationToken")[0].value
            }).done(function (data) {
                if (data.status === "SUCCESS") {
                    Cookies.set("paypal_payment_id", data.order.paymentId);
                    window.location = data.redirectUrl;
                } else {
                    self.processing(false);
                    $("#zip").after("<span class='text-danger d-block'>" + data.validationMessage + "</span>");
                }
            });
        } else {
            self.items([]);
        }
    };

    self.decreaseQuantity = function (item) {
        self.removeItem(item, true);
    };

    self.increaseQuantity = function (item) {
        var cart = readCartFromCookie();
        cart.push({
            merchId: item.merchId(),
            optionId: item.optionId()
        });
        writeCartToCookie(cart);
        populateCheckout();
    };

    self.removeAll = function (item) {
        self.removeItem(item, false);
    };

    self.removeItem = function (item, removeOne) {
        var cart = readCartFromCookie();
        var updatedCart = [];
        var oneRemoved = false;
        if (cart) {
            for (var i = 0; i < cart.length; i++) {
                var cartItem = cart[i];
                if (cartItem.merchId === item.merchId() && cartItem.optionId === item.optionId()) {
                    if (removeOne && oneRemoved) {
                        updatedCart.push(cartItem);
                    } else {
                        if (removeOne) {
                            oneRemoved = true;
                        }
                        continue;
                    }
                } else {
                    updatedCart.push(cartItem);
                }
            }
            writeCartToCookie(updatedCart);
            populateCheckout();
        }
    };

    self.clearCart = function () {
        writeCartToCookie([]);
        populateCheckout();
    };

    self.countryCode.subscribe(function () {
        populateCheckout();
    });

    self.specialDelivery.subscribe(function () {
        populateCheckout();
    });

    $(document).ready(function () {
        updateCookieName();
        populateCheckout();
    });
}
