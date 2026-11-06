(() => {
    const menuButton = document.querySelector(".menu-button");
    const navigation = document.getElementById("site-navigation");
    menuButton?.addEventListener("click", () => {
        const open = navigation.classList.toggle("is-open");
        menuButton.setAttribute("aria-expanded", String(open));
    });

    document.querySelectorAll(".js-add-to-cart").forEach(button => {
        button.addEventListener("click", () => {
            const optionSelect = document.getElementById(button.dataset.optionSelect);
            events.trigger("cart:addItem", {
                merchId: button.dataset.merchId,
                optionId: optionSelect?.value || ""
            });

            const message = document.querySelector("[data-add-message]");
            if (message) message.textContent = "Added to your demo basket.";
        });
    });

    const lightbox = document.querySelector("[data-video-lightbox]");
    const lightboxFrame = lightbox?.querySelector("iframe");
    const lightboxTitle = lightbox?.querySelector("[data-video-lightbox-title]");
    const closeLightbox = () => {
        if (lightbox?.open) lightbox.close();
        if (lightboxFrame) lightboxFrame.removeAttribute("src");
    };

    document.querySelectorAll("[data-video-id]").forEach(button => {
        button.addEventListener("click", () => {
            const title = button.dataset.videoTitle || "YouTube video";
            lightboxTitle.textContent = title;
            lightboxFrame.title = title;
            lightboxFrame.src = `https://www.youtube.com/embed/${encodeURIComponent(button.dataset.videoId)}?autoplay=1`;
            lightbox.showModal();
        });
    });
    lightbox?.querySelector("[data-video-lightbox-close]")?.addEventListener("click", closeLightbox);
    lightbox?.addEventListener("close", () => lightboxFrame?.removeAttribute("src"));
    lightbox?.addEventListener("click", event => {
        if (event.target === lightbox) closeLightbox();
    });

    events.trigger("cart:updated");
})();
