using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace HypnoTech.Demo.EndToEndTests;

[Collection(DemoEndToEndCollection.Name)]
public sealed class AdminContentTests : DemoPageTest
{
    private static readonly byte[] TestImage = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

    public AdminContentTests(DemoEndToEndFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public Task Dynamic_CMS_item_can_be_validated_created_uploaded_edited_searched_duplicated_and_removed() =>
        RunJourneyAsync(
            nameof(Dynamic_CMS_item_can_be_validated_created_uploaded_edited_searched_duplicated_and_removed),
            async page =>
            {
                var suffix = Guid.NewGuid().ToString("N")[..8];
                var searchTerm = $"Playwright story {suffix}";
                var originalTitle = searchTerm;
                var editedTitle = $"{searchTerm} edited";
                var duplicateTitle = $"{searchTerm} copy";

                await LoginAsync(page);
                await page.GotoAsync($"{Fixture.AdminUrl}/Cms/Articles");
                await page.GetByRole(AriaRole.Link, new() { Name = "Add Article", Exact = true }).ClickAsync();

                await page.GetByLabel("Title", new() { Exact = true }).FillAsync(originalTitle);
                await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes", Exact = true }).ClickAsync();

                await Assertions.Expect(page.GetByText("Summary is required.", new() { Exact = true }))
                    .ToBeVisibleAsync();
                await Assertions.Expect(page.GetByText("Body is required.", new() { Exact = true }))
                    .ToBeVisibleAsync();
                await Assertions.Expect(page.GetByLabel("Title", new() { Exact = true }))
                    .ToHaveValueAsync(originalTitle);

                await page.GetByLabel("Summary", new() { Exact = true })
                    .FillAsync("A synthetic article authored by the browser characterization suite.");
                await page.FrameLocator("iframe.tox-edit-area__iframe").Locator("body")
                    .FillAsync("This body verifies that the dynamic rich-text CMS control remains usable.");
                await page.GetByLabel("PublishedAt", new() { Exact = true }).FillAsync("2026-11-06");
                await page.GetByLabel("Hero image", new() { Exact = true }).SetInputFilesAsync(new FilePayload
                {
                    Name = $"playwright-{suffix}.png",
                    MimeType = "image/png",
                    Buffer = TestImage
                });
                await Assertions.Expect(page.Locator("input[name=AddedFiles]")).ToHaveCountAsync(1);

                await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes", Exact = true }).ClickAsync();
                await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.AdminUrl}/Cms/Articles");
                await Assertions.Expect(page.GetByRole(AriaRole.Alert))
                    .ToContainTextAsync($"Added '{originalTitle}'");

                await SearchAsync(page, searchTerm);
                var row = RowFor(page, originalTitle);
                await Assertions.Expect(row).ToBeVisibleAsync();
                await row.Locator("a:has(.fa-pen)").ClickAsync();
                await page.GetByLabel("Title", new() { Exact = true }).FillAsync(editedTitle);
                await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes", Exact = true }).ClickAsync();
                await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.AdminUrl}/Cms/Articles");
                await Assertions.Expect(page.GetByRole(AriaRole.Alert))
                    .ToContainTextAsync($"Updated '{editedTitle}'");

                await page.GotoAsync($"{Fixture.WebsiteUrl}/Articles");
                var publicArticle = page.Locator("article").Filter(new() { HasText = editedTitle });
                await Assertions.Expect(publicArticle).ToBeVisibleAsync();
                await Assertions.Expect(publicArticle.Locator("img")).ToHaveAttributeAsync("src", new System.Text.RegularExpressions.Regex("DemoArticles"));
                await publicArticle.GetByRole(AriaRole.Link, new() { Name = editedTitle, Exact = true }).ClickAsync();
                await Assertions.Expect(page.GetByTestId("article-detail")).ToContainTextAsync(editedTitle);

                await page.GotoAsync($"{Fixture.AdminUrl}/Cms/Articles");
                await SearchAsync(page, searchTerm);
                row = RowFor(page, editedTitle);
                await row.Locator("a:has(.fa-copy)").ClickAsync();
                await page.GetByLabel("Title", new() { Exact = true }).FillAsync(duplicateTitle);
                await page.GetByRole(AriaRole.Button, new() { Name = "Save Changes", Exact = true }).ClickAsync();
                await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.AdminUrl}/Cms/Articles");
                await Assertions.Expect(page.GetByRole(AriaRole.Alert))
                    .ToContainTextAsync($"Added '{duplicateTitle}'");

                await SearchAsync(page, searchTerm);
                await Assertions.Expect(RowFor(page, editedTitle)).ToBeVisibleAsync();
                await Assertions.Expect(RowFor(page, duplicateTitle)).ToBeVisibleAsync();

                await RemoveAsync(page, duplicateTitle, searchTerm);
                await RemoveAsync(page, editedTitle, searchTerm);
                await Assertions.Expect(page.Locator("tbody")).ToContainTextAsync("No items were added yet");
            });

    [Fact]
    public Task Ordered_CMS_items_can_move_and_seed_order_is_restored() =>
        RunJourneyAsync(
            nameof(Ordered_CMS_items_can_move_and_seed_order_is_restored),
            async page =>
            {
                await LoginAsync(page);
                await page.GotoAsync($"{Fixture.AdminUrl}/Cms/Artists");

                await AssertFirstArtistsAsync(page, "Mara Vale", "Jon Bell");
                await page.Locator("tbody tr").First.Locator("a:has(.fa-arrow-down)").ClickAsync();
                await AssertFirstArtistsAsync(page, "Jon Bell", "Mara Vale");

                await page.Locator("tbody tr").Nth(1).Locator("a:has(.fa-arrow-up)").ClickAsync();
                await AssertFirstArtistsAsync(page, "Mara Vale", "Jon Bell");
            });

    private static ILocator RowFor(IPage page, string title) =>
        page.Locator("tbody tr").Filter(new LocatorFilterOptions { HasText = title });

    private static async Task SearchAsync(IPage page, string searchTerm)
    {
        await page.Locator("#search-text")
            .EvaluateAsync("(element, value) => element.value = value", searchTerm);
        await page.Locator("#input-search").ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("[?&]search="));
    }

    private static async Task RemoveAsync(IPage page, string title, string searchTerm)
    {
        var row = RowFor(page, title);
        await row.Locator("button:has(.fa-times)").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Delete item", Exact = true }).ClickAsync();
        await SearchAsync(page, searchTerm);
        await Assertions.Expect(RowFor(page, title)).ToHaveCountAsync(0);
    }

    private static async Task AssertFirstArtistsAsync(IPage page, string first, string second)
    {
        var names = await page.Locator("tbody tr td:first-child").AllTextContentsAsync();
        Assert.Equal(new[] { first, second }, names.Take(2));
    }
}
