using System;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Demo.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.Media.Models;
using Microsoft.AspNetCore.Hosting;
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
        var seed = scope.ServiceProvider.GetRequiredService<ISeedRepository>();

        seed.Seed<Artist>("demoArtists", "artists");
        seed.Seed<Article>("demoArticles", "articles");
        seed.Seed<Event>("demoEvents", "events");
        seed.Seed<AudioTrack>("audiotracks", "audiotracks");
        seed.Seed<PhotoAlbum>("photoalbums", "photoalbums");
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
}
