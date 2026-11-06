using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;
using Xunit.Sdk;

namespace HypnoTech.Demo.EndToEndTests;

public abstract class DemoPageTest
{
    protected DemoPageTest(DemoEndToEndFixture fixture)
    {
        Fixture = fixture;
    }

    protected DemoEndToEndFixture Fixture { get; }

    protected Task RunJourneyAsync(
        string testName,
        Func<IPage, Task> journey,
        ViewportSize viewport = null) =>
        RunJourneyInternalAsync(testName, journey, viewport);

    protected async Task LoginAsync(IPage page)
    {
        await page.GotoAsync($"{Fixture.AdminUrl}/Home/Login");
        await page.GetByLabel("Username").FillAsync("admin");
        await page.GetByLabel("Password").FillAsync("HypnoTechDemo!2026");
        await page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync($"{Fixture.AdminUrl}/");
    }

    private async Task RunJourneyInternalAsync(
        string testName,
        Func<IPage, Task> journey,
        ViewportSize viewport)
    {
        Fixture.ClearArtifactDirectory(testName);
        var errors = new List<string>();
        await using var context = await Fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = viewport ?? new ViewportSize { Width = 1440, Height = 1000 }
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                errors.Add($"console: {message.Text}");
            }
        };
        page.PageError += (_, error) => errors.Add($"page: {error}");

        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Title = $"{Fixture.BrowserName}: {testName}",
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        try
        {
            await journey(page);
            if (errors.Count > 0)
            {
                throw new XunitException(
                    $"Browser errors were recorded:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }

            await context.Tracing.StopAsync();
        }
        catch
        {
            var artifactDirectory = Fixture.CreateArtifactDirectory(testName);
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = true,
                Path = Path.Combine(artifactDirectory, "failure.png")
            });
            await context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(artifactDirectory, "trace.zip")
            });
            await File.WriteAllLinesAsync(
                Path.Combine(artifactDirectory, "browser-errors.log"),
                errors);
            Fixture.WriteApplicationLogs(artifactDirectory);
            throw;
        }
    }
}
