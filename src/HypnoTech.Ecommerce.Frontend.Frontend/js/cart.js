//cart.js
var cookieName = "hypnotech-cart";

function updateCookieName() {
    var element = document.getElementById("cart-cookie-name");
    if (element) {
        cookieName = element.value;
    }
}

function writeCartToCookie(cart) {
    updateCookieName();
    Cookies.set(cookieName, JSON.stringify(cart));
}

function readCartFromCookie() {
    updateCookieName();
    var cookie = Cookies.get(cookieName);
    return cookie ? JSON.parse(cookie) : [];
}

events.on("cart:addItem", function (item) {
    var cart = readCartFromCookie();
    cart.push(item);
    writeCartToCookie(cart);

    events.trigger("cart:updated");
});

events.on("cart:clear", function () {
    writeCartToCookie([]);
    events.trigger("cart:updated");
    events.trigger("cart:emptied");
});

function CartCountViewModel() {
    var self = this;
    self.count = ko.observable(0);

    self.showCartIcon = ko.pureComputed(function () {
        return self.count() > 0;
    });

    self.animateCart  = function () {
        var target = $("#cartIcon");
        target.addClass("animated").addClass("heartBeat");
        setTimeout(function () {
            target.removeClass("animated").removeClass("heartBeat");
        }, 500);
    };

    function setCount() {
        var cart = readCartFromCookie();
        self.count(cart ? cart.length : 0);
    }

    events.on("cart:updated", function () {
        setCount();
        self.animateCart();
    });
}

function CartItemViewModel(data) {
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

function CartViewModel() {
    var self = this;

    self.items = ko.observableArray([]);
    self.subTotal = ko.observable(0);

    self.formattedSubtotal = ko.pureComputed(function () {
        return utilities.formatCurrency(self.subTotal());
    });

    function populateCart() {
        var cart = readCartFromCookie();
        if (cart) {
            ajax.post("/Basket/GetDetails", { items: cart }).done(function (data) {
                self.items([]);
                for (var i = 0; i < data.items.length; i++) {
                    self.items.push(new CartItemViewModel(data.items[i]));
                }
                self.subTotal(data.subTotal);
            });
        } else {
            self.items([]);
        }
    }

    self.removeItem = function (item) {
        var cart = readCartFromCookie();
        var updatedCart = [];
        if (cart) {
            for (var i = 0; i < cart.length; i++) {
                var cartItem = cart[i];
                if (cartItem.merchId === item.merchId() && cartItem.optionId === item.optionId()) {
                    continue;
                } else {
                    updatedCart.push(cartItem);
                }
            }
            writeCartToCookie(updatedCart);
            events.trigger("cart:updated");
            if (updatedCart.length < 1) {
                events.trigger("cart:emptied");
            }
        }
    };

    self.clearCart = function () {
        writeCartToCookie([]);
        events.trigger("cart:updated");
        events.trigger("cart:emptied");
    };

    events.on("cart:updated", function () {
        populateCart();
    });

    $(document).ready(function () {
        updateCookieName();
        events.trigger("cart:updated");
    });
}
