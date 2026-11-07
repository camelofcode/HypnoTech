using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace HypnoTech.Demo.EndToEndTests;

[Collection(DemoEndToEndCollection.Name)]
public sealed class PublicExperienceTests : DemoPageTest
{
    public PublicExperienceTests(DemoEndToEndFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public Task Media_player_gallery_basket_and_calculation_only_checkout_work() =>
        RunJourneyAsync(
            nameof(Media_player_gallery_basket_and_calculation_only_checkout_work),
            async page =>
            {
                await page.GotoAsync($"{Fixture.WebsiteUrl}/Media");

                await Assertions.Expect(page.GetByTestId("media-audio")).ToBeVisibleAsync();
                await page.GetByRole(AriaRole.Button, new() { Name = "Lantern signal Play in persistent player" })
                    .ClickAsync();
                var player = page.GetByTestId("persistent-player");
                await Assertions.Expect(player).ToHaveAttributeAsync("aria-hidden", "false");
                await Assertions.Expect(player).ToContainTextAsync("Lantern signal");

                await Assertions.Expect(page.GetByTestId("youtube-videos").Locator(".video-card"))
                    .ToHaveCountAsync(1);
                await Assertions.Expect(page.GetByTestId("media-video").Locator("video source"))
                    .ToHaveAttributeAsync("src", "/Files/Demo/lantern-motion.mp4");
                await Assertions.Expect(page.GetByTestId("photo-galleries").Locator("figure img"))
                    .ToHaveCountAsync(3);

                await page.GetByRole(AriaRole.Link, new() { Name = "Shop", Exact = true }).ClickAsync();
                await Assertions.Expect(page.GetByTestId("shop-list")).ToBeVisibleAsync();
                await page.GetByRole(AriaRole.Link, new() { Name = "Lantern House risograph", Exact = true })
                    .ClickAsync();
                await Assertions.Expect(page.GetByTestId("product-detail")).ToBeVisibleAsync();
                await page.GetByLabel("Choose an option").SelectOptionAsync(new SelectOptionValue { Label = "Indigo" });
                await page.GetByRole(AriaRole.Button, new() { Name = "Add to basket", Exact = true }).ClickAsync();

                var basket = page.GetByTestId("persistent-basket");
                await Assertions.Expect(basket).ToHaveAttributeAsync("aria-hidden", "false");
                await Assertions.Expect(basket).ToContainTextAsync("Lantern House risograph (Indigo)");
                await Assertions.Expect(basket).ToContainTextAsync("£14.00");
                await basket.GetByRole(AriaRole.Link, new() { Name = "Review demo checkout", Exact = true })
                    .ClickAsync();

                await Assertions.Expect(page.GetByTestId("checkout-page")).ToBeVisibleAsync();
                await Assertions.Expect(page.GetByTestId("demo-checkout-safety"))
                    .ToContainTextAsync("No payment button is rendered");
                await page.GetByLabel("Delivery country").SelectOptionAsync("GB");
                var summary = page.Locator(".order-summary");
                await Assertions.Expect(summary).ToContainTextAsync("£3.50");
                await Assertions.Expect(summary).ToContainTextAsync("£17.50");
                await Assertions.Expect(page.GetByRole(AriaRole.Button, new()
                {
                    NameRegex = new System.Text.RegularExpressions.Regex("Pay|Place order|Submit order")
                }))
                    .ToHaveCountAsync(0);
            });

    [Fact]
    public Task Mobile_navigation_reaches_a_partial_navigation_page() =>
        RunJourneyAsync(
            nameof(Mobile_navigation_reaches_a_partial_navigation_page),
            async page =>
            {
                await page.GotoAsync(Fixture.WebsiteUrl);
                var menu = page.GetByRole(AriaRole.Button, new() { Name = "Menu", Exact = true });
                await menu.ClickAsync();
                await Assertions.Expect(menu).ToHaveAttributeAsync("aria-expanded", "true");
                await page.GetByRole(AriaRole.Link, new() { Name = "Events", Exact = true }).ClickAsync();
                await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.WebsiteUrl}/Events");
                await Assertions.Expect(page.GetByTestId("events-list")).ToBeVisibleAsync();
                await Assertions.Expect(menu).ToHaveAttributeAsync("aria-expanded", "false");
            },
            new ViewportSize { Width = 390, Height = 844 });
}
