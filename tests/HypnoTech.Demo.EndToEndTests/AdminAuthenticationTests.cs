using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace HypnoTech.Demo.EndToEndTests;

[Collection(DemoEndToEndCollection.Name)]
public sealed class AdminAuthenticationTests : DemoPageTest
{
    public AdminAuthenticationTests(DemoEndToEndFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public Task Protected_admin_route_requires_login_and_authenticated_user_can_log_out() =>
        RunJourneyAsync(
            nameof(Protected_admin_route_requires_login_and_authenticated_user_can_log_out),
            async page =>
            {
                await page.GotoAsync($"{Fixture.AdminUrl}/Cms/Articles");

                await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.AdminUrl}/Home/Login?ReturnUrl=%2FCms%2FArticles");
                await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Admin Login" }))
                    .ToBeVisibleAsync();

                await page.GetByLabel("Username").FillAsync("admin");
                await page.GetByLabel("Password").FillAsync("HypnoTechDemo!2026");
                await page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true }).ClickAsync();

                await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Welcome admin" }))
                    .ToBeVisibleAsync();
                await page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).ClickAsync();

                await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.AdminUrl}/Home/Login");
                await Assertions.Expect(page.GetByText("Logout Successful")).ToBeVisibleAsync();
            });
}
