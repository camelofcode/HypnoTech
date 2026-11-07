using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Extensions;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Demo.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Media.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Demo.Admin;

public static class DemoSeeder
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "HypnoTechDemo!2026";

    public static async Task SeedAsync(
        IServiceProvider services,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "The public demo seed, including its documented credentials, is Development-only.");
        }

        using var scope = services.CreateScope();
        await CopyDemoUploadsAsync(
            scope.ServiceProvider.GetRequiredService<IConfiguration>(),
            environment,
            cancellationToken);
        var seed = scope.ServiceProvider.GetRequiredService<ISeedRepository>();

        await seed.SeedAsync<Artist>("demoArtists", "artists", cancellationToken);
        await seed.SeedAsync<Article>("demoArticles", "articles", cancellationToken);
        await seed.SeedAsync<Event>("demoEvents", "events", cancellationToken);
        await seed.SeedAsync<AudioTrack>("audiotracks", "audiotracks", cancellationToken);
        await seed.SeedAsync<PhotoAlbum>("photoalbums", "photoalbums", cancellationToken);
        await seed.SeedAsync<YouTubeVideo>("videos", "videos", cancellationToken);
        await seed.SeedAsync<MerchCategoryType>("merchcategorytypes", "merchcategorytypes", cancellationToken);
        await seed.SeedAsync<MerchItem>("merches", "merch", cancellationToken);
        await seed.SeedAsync<PostageProfile>("postageprofiles", "postageprofiles", cancellationToken);
        await seed.SeedAsync<Country>("countries", "countries", cancellationToken);
        await seed.SeedAsync<Config>("config", "config", cancellationToken);
        await seed.SeedAsync<NavBarLink>("navbar", "navbar", cancellationToken);

        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (await users.GetUserForLoginAsync(AdminUsername, cancellationToken) == null)
        {
            await users.AddUserAsync(new AdminUser
            {
                Username = AdminUsername,
                Password = PasswordHelper.HashPassword(AdminPassword),
                Email = "admin@lantern-house.example",
                Roles = new()
                {
                    "articles", "artists", "events", "audio", "photos", "videos", "merch",
                    "users", "config", "features", "emailtemplates", "logs", "backups",
                    "orders", "postageprofiles", "downloadcodes"
                }
            }, cancellationToken);
        }
    }

    private static async Task CopyDemoUploadsAsync(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        var packagedRoot = Path.Combine(environment.ContentRootPath, "DemoUploads");
        var websiteRoot = Directory.Exists(packagedRoot)
            ? packagedRoot
            : Path.GetFullPath(Path.Combine(
                environment.ContentRootPath, "..", "HypnoTech.Demo.Website", "wwwroot"));
        var uploadRoot = configuration.UploadPath();

        foreach (var (source, destination) in new[]
                 {
                     ("images/artwork-amber.svg", "Images/Demo/artwork-amber.svg"),
                     ("images/artwork-indigo.svg", "Images/Demo/artwork-indigo.svg"),
                     ("images/artwork-mint.svg", "Images/Demo/artwork-mint.svg"),
                     ("images/artwork-rose.svg", "Images/Demo/artwork-rose.svg"),
                     ("documents/shadow-workshop.txt", "Files/Demo/shadow-workshop.txt"),
                     ("documents/signals-programme.txt", "Files/Demo/signals-programme.txt"),
                     ("media/lantern-signal.wav", "Files/Demo/lantern-signal.wav"),
                     ("media/lantern-motion.mp4", "Files/Demo/lantern-motion.mp4")
                 })
        {
            var sourcePath = Path.Combine(websiteRoot, source);
            var destinationPath = Path.Combine(uploadRoot, destination);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            await using var input = new FileStream(
                sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
            await using var output = new FileStream(
                destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
            await input.CopyToAsync(output, cancellationToken);
        }
    }
}
