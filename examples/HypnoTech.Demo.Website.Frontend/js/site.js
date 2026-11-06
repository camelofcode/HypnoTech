(() => {
    window.demoCart = {
        execute: function () {
            const cart = document.getElementById("cart-page");
            if (!cart || ko.dataFor(cart)) return;

            window.cartModel = window.cartModel || new CartViewModel();
            ko.applyBindings(window.cartModel, cart);
            events.trigger("cart:updated");
        }
    };

    const menuButton = document.querySelector(".menu-button");
    const navigation = document.getElementById("site-navigation");
    const playerDrawer = document.getElementById("audio-player");
    const playerToggle = document.querySelector(".player-drawer-toggle");
    const playerToggleLabel = playerToggle?.querySelector(".visually-hidden");
    const basketDrawer = document.getElementById("basket-drawer");
    const basketToggle = document.getElementById("cart-counter");
    const lightbox = document.querySelector("[data-video-lightbox]");
    const lightboxFrame = lightbox?.querySelector("iframe");
    const lightboxTitle = lightbox?.querySelector("[data-video-lightbox-title]");

    const setMenuOpen = open => {
        navigation?.classList.toggle("is-open", open);
        menuButton?.setAttribute("aria-expanded", String(open));
    };

    const setPlayerOpen = open => {
        if (open) setBasketOpen(false);
        playerDrawer?.classList.toggle("is-open", open);
        playerDrawer?.setAttribute("aria-hidden", String(!open));
        playerToggle?.setAttribute("aria-expanded", String(open));
        if (playerToggleLabel) playerToggleLabel.textContent = `${open ? "Close" : "Open"} audio player`;
    };

    const setBasketOpen = open => {
        if (open) setPlayerOpen(false);
        basketDrawer?.classList.toggle("is-open", open);
        basketDrawer?.setAttribute("aria-hidden", String(!open));
        basketToggle?.setAttribute("aria-expanded", String(open));
        playerToggle?.classList.toggle("is-hidden", open);
    };

    const closeLightbox = () => {
        if (lightbox?.open) lightbox.close();
        lightboxFrame?.removeAttribute("src");
    };

    menuButton?.addEventListener("click", () => {
        setMenuOpen(!navigation?.classList.contains("is-open"));
    });

    playerToggle?.addEventListener("click", () => {
        setPlayerOpen(!playerDrawer?.classList.contains("is-open"));
    });

    basketToggle?.addEventListener("click", event => {
        event.preventDefault();
        setBasketOpen(!basketDrawer?.classList.contains("is-open"));
    });

    basketDrawer?.querySelector("[data-cart-close]")?.addEventListener("click", () => setBasketOpen(false));

    document.addEventListener("click", event => {
        const addToCart = event.target.closest(".js-add-to-cart");
        if (addToCart) {
            const optionSelect = document.getElementById(addToCart.dataset.optionSelect);
            events.trigger("cart:addItem", {
                merchId: addToCart.dataset.merchId,
                optionId: optionSelect?.value || ""
            });

            const message = document.querySelector("[data-add-message]");
            if (message) message.textContent = "Added to your demo basket.";
            return;
        }

        const trackButton = event.target.closest("[data-play-track-id]");
        if (trackButton) {
            events.trigger("player:playTrack", trackButton.dataset.playTrackId);
            setPlayerOpen(true);
            return;
        }

        const videoButton = event.target.closest("[data-video-id]");
        if (videoButton && lightbox && lightboxFrame && lightboxTitle) {
            const title = videoButton.dataset.videoTitle || "YouTube video";
            lightboxTitle.textContent = title;
            lightboxFrame.title = title;
            lightboxFrame.src = `https://www.youtube.com/embed/${encodeURIComponent(videoButton.dataset.videoId)}?autoplay=1`;
            lightbox.showModal();
        }
    });

    lightbox?.querySelector("[data-video-lightbox-close]")?.addEventListener("click", closeLightbox);
    lightbox?.addEventListener("close", () => lightboxFrame?.removeAttribute("src"));
    lightbox?.addEventListener("click", event => {
        if (event.target === lightbox) closeLightbox();
    });

    events.on("page-loaded", () => setMenuOpen(false));
    events.on("cart:addItem", () => {
        if (window.matchMedia("(min-width: 821px)").matches) setBasketOpen(true);
    });
    events.on("cart:emptied", () => setBasketOpen(false));
    events.trigger("cart:updated");
})();
