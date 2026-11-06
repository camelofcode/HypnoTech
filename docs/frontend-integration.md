# Integrating HypnoTech frontend features

HypnoTech's frontend is server-rendered ASP.NET Core MVC with optional partial-page
navigation. Direct requests still return complete HTML, while opted-in internal links
replace only the changing Razor content. Persistent UI such as an audio player or
basket can therefore remain mounted and keep its state between pages.

Lantern House is the executable reference host in
[`examples/HypnoTech.Demo.Website`](../examples/HypnoTech.Demo.Website).

## Register the frontend modules

Register the core frontend and whichever feature modules the host uses:

```csharp
services.AddHttpContextAccessor();
services.UseHypnoTech(Environment);
services.AddMvc();
services.AddMediaFrontend<DemoAudioTrackEnrichmentService>(Configuration);
services.AddEcommerce(Configuration);

services.UseMongoDbForCommon(Configuration);
services.UseMongoDbForEcommerce(Configuration);
services.UseMongoDbForMedia(Configuration);
```

Complete middleware setup with:

```csharp
app.ConfigureHypnoTech(Environment, Configuration);
```

Add the tag helpers used by Razor views:

```razor
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, HypnoTech.Frontend
@addTagHelper *, HypnoTech.Media.Frontend
@addTagHelper *, HypnoTech.Ecommerce.Frontend
```

## Include the platform scripts

Place the script tag helper near the end of the layout, before host JavaScript that
uses platform globals such as `events`, `ko`, `CartViewModel` or `PlayerViewModel`:

```razor
<hypnotech-scripts site-name="Lantern House"></hypnotech-scripts>
<script src="~/js/site.js" asp-append-version="true"></script>
```

The engine serves pinned, self-hosted browser assets from its static-web-asset
projects. A consuming application does not need a CDN, npm install or Node.js runtime.

The authored engine sources are compiled into separate committed bundles:

| Authored source | Runtime bundle |
| --- | --- |
| `HypnoTech.Frontend.Frontend/js` (router, events, Ajax and utilities) | `hypnotech.frontend(.min).js` |
| `HypnoTech.Media.Frontend.Frontend/js/player.js` | `hypnotech.media(.min).js` |
| `HypnoTech.Ecommerce.Frontend.Frontend/js` (cart and checkout) | `hypnotech.ecommerce(.min).js` |

`<hypnotech-scripts>` emits these engine bundles. Host JavaScript should contain only
site-specific behavior and page scripts; it must not copy `PlayerViewModel`,
`CartViewModel`, the router or the shared event bus. Lantern House keeps those authored
assets in `examples/HypnoTech.Demo.Website.Frontend`; the root build emits committed
development and minified files, and `<site-styles>` / `<site-scripts>` select the right
versions for the current environment. The production sites follow the same source and
generated-output pattern.

Keep CMS editing concerns out of the public website's dependency graph. Shared model
projects can contain model types and BSON mappings, but fluent `FormClassMapping`
registrations belong to the Admin host. The public website should use its own focused
read repositories over `HypnoTech.MongoDb`, as Lantern House and the production sites
do, rather than referencing `HypnoTech.Cms` or `HypnoTech.Cms.MongoDb`.

This is also an asset boundary: `<hypnotech-scripts>` automatically emits bundles from
referenced HypnoTech static-web-asset projects. A public site that references the CMS
web project can therefore expose its admin runtime and execute TinyMCE-dependent code.
Do not filter around that dependency; remove it from the public host.

## Partial-page navigation

Partial navigation is progressive enhancement. A normal link remains a valid URL and
a direct request or disabled JavaScript receives a full document.

### Controller contract

Derive navigable controllers from `SpaController` and return `AjaxPartialView`:

```csharp
public sealed class ArticlesController : SpaController
{
    private readonly IArticleRepository _articles;

    public ArticlesController(IArticleRepository articles) => _articles = articles;

    public IActionResult Index() =>
        AjaxPartialView(_articles.GetAll().ToList());
}
```

Without `X-Partial-Request`, `AjaxPartialView` delegates to the normal MVC `View` and
renders the layout. With that request header it renders the requested Razor view and
returns JSON containing:

```json
{
  "html": "<section>...</section>",
  "title": "Stories",
  "script": null,
  "description": "Page description",
  "canonicalUrl": "https://example.test/articles"
}
```

The server remains authoritative for routing, authorization and rendering. This is not
a second client-side application with duplicated templates.

### Layout contract

The router expects these stable layout elements:

```razor
<meta name="Description" content="@ViewData["Description"]" />
<link rel="canonical" href="..." />
<title>@ViewData["Title"] - Lantern House</title>

<main id="main">
    <div id="content">
        @RenderBody()
    </div>
</main>

<hypnotech-scripts site-name="Lantern House"></hypnotech-scripts>
```

The exact capital `Description` name is part of the current JavaScript contract. The
scripts tag helper emits the hidden `site-name` and `page-script` values used by the
router.

Put only replaceable page content inside `#content`. Site navigation, persistent
drawers, dialogs, players and counters belong outside it.

### Opt links in

Add `data-navigo` to same-origin links whose target controller follows the contract:

```razor
<a asp-controller="Articles" asp-action="Index" data-navigo>Stories</a>
<a href="/articles/@article.Slug" data-navigo>@article.Title</a>
```

After a successful request the engine updates browser history, `#content`, title,
canonical URL and description, refreshes newly inserted `data-navigo` links, executes
the page script if present, scrolls to the top and publishes `page-loaded`.

Do not add `data-navigo` to external links, downloads, hash-only controls, form
submissions, or routes that intentionally need a new document. Lantern House leaves
checkout outside partial navigation because checkout owns a separate full-page flow.
Its controller therefore remains a normal MVC controller and the basket's checkout
link deliberately has no `data-navigo` attribute.

## Persistent audio player

Register Media frontend services, render the player outside `#content`, and bind it
with the tag helper:

```razor
<aside id="audio-player" class="persistent-player">
    <audio id="audio-element"
           data-bind="attr: { src: currentTrack().path }"></audio>
    <button type="button" data-bind="click: play, visible: !playing()">Play</button>
    <button type="button" data-bind="click: pause, visible: playing">Pause</button>
    <div data-bind="foreach: tracks">
        <button type="button" data-bind="click: $parent.selectTrack">
            <span data-bind="text: title"></span>
        </button>
    </div>
</aside>

<hypnotech-player-config player-element-id="audio-player" />
```

The tag helper gets the CMS-managed playlist through `IAudioTrackService`, constructs
`PlayerViewModel`, and applies Knockout bindings to the supplied element. The actual
markup and styling remain host-controlled.

Any page can launch a track through the shared event bus:

```html
<button type="button" data-play-track-id="@track.Id">Play</button>
```

```javascript
document.addEventListener("click", event => {
    const trigger = event.target.closest("[data-play-track-id]");
    if (!trigger) return;

    events.trigger("player:playTrack", trigger.dataset.playTrackId);
    openPlayerDrawer();
});
```

The player subscribes to `player:playTrack`, selects the matching CMS track and starts
it. Page code does not need a reference to the player view model. Because the audio
element lives outside `#content`, its playback and position survive partial navigation.

## Persistent basket and cart fallback

Render both the counter and the primary basket UI outside `#content`, then configure
the cart once in the layout. The link retains a real `/Cart` fallback, but host
JavaScript prevents its normal navigation when enhanced behavior is available and
opens the drawer instead:

```razor
<a asp-controller="Cart" asp-action="Index" id="cart-counter"
   aria-expanded="false" aria-controls="basket-drawer">
    Basket <span data-bind="text: count">0</span>
</a>

<aside id="basket-drawer" aria-hidden="true">
    <div id="cart">
        <p data-bind="visible: items().length === 0">Your basket is empty.</p>
        <div data-bind="foreach: items">
            <strong data-bind="text: displayName"></strong>
            <span data-bind="text: formattedRowPrice"></span>
            <button type="button" data-bind="click: $parent.removeItem">Remove</button>
        </div>
        <strong data-bind="text: formattedSubtotal"></strong>
        <a asp-controller="Checkout" asp-action="Index">Checkout</a>
        <button type="button" data-bind="click: clearCart">Clear basket</button>
    </div>
</aside>

<hypnotech-cart-config
    cart-element-id="cart"
    cart-counter-element-id="cart-counter"
    cookie-name="my-site-cart" />
```

Keep the checkout link free of `data-navigo`: checkout is an intentional fresh-document
boundary. Synchronize `aria-expanded` and `aria-hidden` when the host opens or closes
the drawer. Lantern House also opens the basket after `cart:addItem` on wider screens,
closes it on `cart:emptied`, and makes its player and basket drawers mutually exclusive.

Product controls publish an item summary; the engine writes its configured cart cookie
and publishes `cart:updated`:

```javascript
events.trigger("cart:addItem", {
    merchId: button.dataset.merchId,
    optionId: optionSelect?.value || ""
});
```

If the host retains a full `/Cart` fallback page, give its root a different ID and set
a page script name in the Razor view:

```razor
@{
    ViewData["Script"] = "siteCart";
}
<section id="cart-page" data-bind="foreach: items">...</section>
```

Define that named script in a JavaScript file loaded by the persistent layout, not an
inline script inside replaceable HTML:

```javascript
window.siteCart = {
    execute: function () {
        const element = document.getElementById("cart-page");
        if (!element || ko.dataFor(element)) return;

        window.cartModel = window.cartModel || new CartViewModel();
        ko.applyBindings(window.cartModel, element);
        events.trigger("cart:updated");
    }
};
```

The fallback uses the existing persistent `window.cartModel`, so the drawer and page
show the same state. The same function runs on an initial full cart page and after
partial navigation; the binding check prevents applying Knockout twice.

## Code that survives content replacement

Direct listeners attached to elements inside `#content` disappear when the router
replaces that content. Use one of these patterns:

1. Event delegation from `document` or another persistent ancestor for simple click
   behavior.
2. A named `ViewData["Script"]` object with an `execute` method for page initialization.
3. A listener on the shared `page-loaded` event for site-wide post-navigation work.

Delegation is a good fit for product, video and track buttons:

```javascript
document.addEventListener("click", event => {
    const video = event.target.closest("[data-video-id]");
    if (video) openVideo(video.dataset.videoId);
});

events.on("page-loaded", () => closeMobileNavigation());
```

Keep persistent dialogs such as a YouTube lightbox outside `#content`, then use
delegation so buttons on newly rendered Media pages can open it.

## Failure and accessibility behavior

If a partial request cannot reach the server or returns an unexpected failure, the
router falls back to normal navigation. Preserve real `href` values so that fallback,
open-in-new-tab, crawlers and keyboard navigation all work.

Host UI is still responsible for accessible controls: use actual buttons for actions,
keep `aria-expanded` and `aria-hidden` synchronized on drawers, provide audio control
labels, restore sensible focus behavior, and give dialogs a labelled heading and close
control.

## Verification checklist

1. Request a route normally and confirm a complete HTML document is returned.
2. Request it with `X-Partial-Request` and confirm JSON contains only the page fragment
   plus title, description, canonical URL and optional script.
3. Use a `data-navigo` link and confirm the URL, history, title and metadata update.
4. Use Back and Forward and confirm the expected page renders.
5. Start audio, navigate between several pages and confirm playback does not restart.
6. Launch a track from a page-level button and confirm the persistent player selects it.
7. Navigate to a product and Media page more than once; confirm delegated controls do
   not duplicate handlers.
8. Open the basket drawer and confirm items, removal, clearing and totals update; load
   `/Cart` directly and confirm the fallback page binds to the same model.
9. Confirm checkout, downloads and external URLs still perform normal navigation.
10. Repeat the primary journey at a narrow viewport and with keyboard controls.
