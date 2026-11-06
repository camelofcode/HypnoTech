(() => {
    const storageKey = "hypnotech-demo-cart";
    const money = value => `£${Number(value || 0).toFixed(2)}`;
    const readCart = () => JSON.parse(localStorage.getItem(storageKey) || "[]");
    const writeCart = cart => {
        localStorage.setItem(storageKey, JSON.stringify(cart));
        updateCount();
    };
    const updateCount = () => {
        document.querySelectorAll("[data-cart-count]").forEach(node => node.textContent = readCart().length);
    };

    const menuButton = document.querySelector(".menu-button");
    const navigation = document.getElementById("site-navigation");
    menuButton?.addEventListener("click", () => {
        const open = navigation.classList.toggle("is-open");
        menuButton.setAttribute("aria-expanded", String(open));
    });

    document.querySelectorAll(".js-add-to-cart").forEach(button => {
        button.addEventListener("click", () => {
            const optionSelect = document.getElementById(button.dataset.optionSelect);
            const cart = readCart();
            cart.push({ merchId: button.dataset.merchId, optionId: optionSelect?.value || "" });
            writeCart(cart);
            const message = document.querySelector("[data-add-message]");
            if (message) message.textContent = "Added to your demo basket.";
        });
    });

    const appendItems = (parameters, cart) => {
        cart.forEach((item, index) => {
            parameters.append(`Items[${index}].MerchId`, item.merchId);
            parameters.append(`Items[${index}].OptionId`, item.optionId || "");
        });
    };
    const postForm = async (url, parameters) => {
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
            body: parameters.toString()
        });
        if (!response.ok) throw new Error(`Request failed with status ${response.status}`);
        return response.json();
    };

    const cartItems = document.querySelector("[data-cart-items]");
    const renderCart = async () => {
        if (!cartItems) return;
        const cart = readCart();
        if (!cart.length) {
            cartItems.innerHTML = "<p>Your basket is empty.</p>";
            document.querySelector("[data-cart-subtotal]").textContent = money(0);
            document.querySelector("[data-cart-postage]").textContent = money(0);
            document.querySelector("[data-cart-tracked]").textContent = money(0);
            document.querySelector("[data-cart-total]").textContent = money(0);
            return;
        }

        const basketParameters = new URLSearchParams();
        appendItems(basketParameters, cart);
        const basket = await postForm("/Basket/GetDetails", basketParameters);

        cartItems.replaceChildren();
        basket.items.forEach(item => {
            const row = document.createElement("div");
            row.className = "cart-row";
            const description = document.createElement("div");
            const title = document.createElement("strong");
            title.textContent = item.displayName;
            const quantity = document.createElement("span");
            quantity.textContent = `Quantity ${item.quantity}`;
            description.append(title, quantity);
            const price = document.createElement("strong");
            price.textContent = money(item.rowPrice);
            row.append(description, price);
            cartItems.append(row);
        });

        const checkoutParameters = new URLSearchParams();
        appendItems(checkoutParameters, cart);
        checkoutParameters.append("CountryCode", document.getElementById("checkout-country").value);
        checkoutParameters.append("SpecialDelivery", document.getElementById("special-delivery").checked);
        const checkout = await postForm("/Checkout/GetDetails", checkoutParameters);
        document.querySelector("[data-cart-subtotal]").textContent = money(checkout.subTotal);
        document.querySelector("[data-cart-postage]").textContent = money(checkout.postage);
        document.querySelector("[data-cart-tracked]").textContent = money(checkout.specialDeliveryCost);
        document.querySelector("[data-cart-total]").textContent = money(checkout.total);
    };

    document.querySelector("[data-clear-cart]")?.addEventListener("click", () => {
        writeCart([]);
        renderCart();
    });
    document.getElementById("checkout-country")?.addEventListener("change", renderCart);
    document.getElementById("special-delivery")?.addEventListener("change", renderCart);
    document.querySelector("[data-demo-checkout]")?.addEventListener("click", () => {
        document.querySelector("[data-checkout-safety]")?.classList.add("is-visible");
    });

    updateCount();
    renderCart().catch(error => {
        if (cartItems) cartItems.textContent = `The demo basket could not be loaded: ${error.message}`;
    });
})();
