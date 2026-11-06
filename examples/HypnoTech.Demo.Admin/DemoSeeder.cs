using System;
using System.IO;
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

    public static void Seed(IServiceProvider services, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "The public demo seed, including its documented credentials, is Development-only.");
        }

        using var scope = services.CreateScope();
        CopyDemoUploads(scope.ServiceProvider.GetRequiredService<IConfiguration>(), environment);
        var seed = scope.ServiceProvider.GetRequiredService<ISeedRepository>();

        seed.Seed<Artist>("demoArtists", "artists");
        seed.Seed<Article>("demoArticles", "articles");
        seed.Seed<Event>("demoEvents", "events");
        seed.Seed<AudioTrack>("audiotracks", "audiotracks");
        seed.Seed<PhotoAlbum>("photoalbums", "photoalbums");
        seed.Seed<YouTubeVideo>("videos", "videos");
        seed.Seed<MerchCategoryType>("merchcategorytypes", "merchcategorytypes");
        seed.Seed<MerchItem>("merches", "merch");
        seed.Seed<PostageProfile>("postageprofiles", "postageprofiles");
        seed.Seed<Country>("countries", "countries");
        seed.Seed<Config>("config", "config");
        seed.Seed<NavBarLink>("navbar", "navbar");

        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (users.GetUserForLogin(AdminUsername) == null)
        {
            users.AddUser(new AdminUser
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
            });
        }
    }

    private static void CopyDemoUploads(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var packagedRoot = Path.Combine(environment.ContentRootPath, "DemoUploads");
        var websiteRoot = Directory.Exists(packagedRoot)
            ? packagedRoot
            : Path.GetFullPath(Path.Combine(
                environment.ContentRootPath, "..", "HypnoTech.Demo.Website", "wwwroot"));
        var uploadRoot = configuration.UploadPath();

        Copy("images/artwork-amber.svg", "Images/Demo/artwork-amber.svg");
        Copy("images/artwork-indigo.svg", "Images/Demo/artwork-indigo.svg");
        Copy("images/artwork-mint.svg", "Images/Demo/artwork-mint.svg");
        Copy("images/artwork-rose.svg", "Images/Demo/artwork-rose.svg");
        Copy("documents/shadow-workshop.txt", "Files/Demo/shadow-workshop.txt");
        Copy("documents/signals-programme.txt", "Files/Demo/signals-programme.txt");
        Copy("media/lantern-signal.wav", "Files/Demo/lantern-signal.wav");
        Copy("media/lantern-motion.mp4", "Files/Demo/lantern-motion.mp4");
        return;

        void Copy(string sourceRelativePath, string destinationRelativePath)
        {
            var sourcePath = Path.Combine(websiteRoot, sourceRelativePath);
            var destinationPath = Path.Combine(uploadRoot, destinationRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, true);
        }
    }
}
