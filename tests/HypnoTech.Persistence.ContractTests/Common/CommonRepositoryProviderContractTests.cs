using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using Xunit;

namespace HypnoTech.Persistence.ContractTests.Common;

public abstract class CommonRepositoryProviderContractTests
{
    protected abstract IEnumerable<ICommonRepositoryContractContext> CreateContractContexts();

    [Fact]
    public async Task Navbar_returns_only_enabled_links_in_display_order_and_honours_cancellation()
    {
        await ForEachContextAsync(async context =>
        {
            await context.SeedNavBarLinksAsync(
            [
                new NavBarLink { Key = "later", Text = "Later", Order = 2, Enabled = true },
                new NavBarLink { Key = "disabled", Text = "Disabled", Order = 0, Enabled = false },
                new NavBarLink { Key = "earlier", Text = "Earlier", Order = 1, Enabled = true }
            ], TestContext.Current.CancellationToken);

            var links = await context.NavBarRepository.GetEnabledNavBarLinksAsync(
                TestContext.Current.CancellationToken);

            Assert.Equal(["earlier", "later"], links.Select(link => link.Key));

            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                context.NavBarRepository.GetEnabledNavBarLinksAsync(cancelled.Token));
        });
    }

    [Fact]
    public async Task Navbar_admin_returns_ordered_links_and_replaces_the_complete_set()
    {
        await ForEachContextAsync(async context =>
        {
            await context.SeedNavBarLinksAsync(
            [
                new NavBarLink { Key = "second", Text = "Second", Order = 2, Enabled = true },
                new NavBarLink { Key = "first", Text = "First", Order = 1, Enabled = true }
            ], TestContext.Current.CancellationToken);

            Assert.Equal(
                ["first", "second"],
                (await context.NavBarAdminRepository.GetNavBarLinksAsync(
                    TestContext.Current.CancellationToken)).Select(link => link.Key));

            await context.NavBarAdminRepository.UpdateNavBarLinksAsync(
            [
                new NavBarLink { Key = "replacement", Text = "Replacement", Order = 1, Enabled = true }
            ], TestContext.Current.CancellationToken);

            Assert.Equal(
                "replacement",
                Assert.Single(await context.NavBarAdminRepository.GetNavBarLinksAsync(
                    TestContext.Current.CancellationToken)).Key);
        });
    }

    [Fact]
    public async Task Users_and_accounts_preserve_uniqueness_roles_and_password_rules()
    {
        await ForEachContextAsync(async context =>
        {
            var alice = new AdminUser
            {
                Username = "Alice",
                Email = "alice@example.test",
                Password = "hash-one",
                Roles = ["Admin"]
            };
            var bob = new AdminUser
            {
                Username = "Bob",
                Email = "bob@example.test",
                Password = "bob-hash",
                Roles = ["Editor"]
            };
            Assert.Null(await context.UserRepository.AddUserAsync(
                alice,
                TestContext.Current.CancellationToken));
            Assert.Null(await context.UserRepository.AddUserAsync(
                bob,
                TestContext.Current.CancellationToken));
            Assert.Contains(
                "Username",
                await context.UserRepository.AddUserAsync(
                    new AdminUser
                    {
                        Username = "ALICE",
                        Email = "unique@example.test",
                        Password = "hash",
                        Roles = []
                    },
                    TestContext.Current.CancellationToken));
            Assert.Contains(
                "Email",
                await context.UserRepository.AddUserAsync(
                    new AdminUser
                    {
                        Username = "Unique",
                        Email = "BOB@example.test",
                        Password = "hash",
                        Roles = []
                    },
                    TestContext.Current.CancellationToken));

            Assert.Equal(
                ["Admin"],
                await context.UserRepository.GetUserRolesAsync(
                    "ALICE",
                    TestContext.Current.CancellationToken));
            Assert.Equal(
                alice.Id,
                (await context.UserRepository.GetUserForEditAsync(
                    alice.Id,
                    TestContext.Current.CancellationToken)).Id);

            alice.Username = "Alice Renamed";
            alice.Email = "alice-renamed@example.test";
            alice.Password = "must-not-replace";
            Assert.Null(await context.UserRepository.UpdateUserAsync(
                alice,
                TestContext.Current.CancellationToken));
            Assert.Equal(
                "hash-one",
                (await context.UserRepository.GetUserForLoginAsync(
                    "ALICE RENAMED",
                    TestContext.Current.CancellationToken)).Password);

            await context.UserRepository.ChangePasswordAsync(
                alice.Id,
                "hash-two",
                TestContext.Current.CancellationToken);
            await context.UserRepository.ChangePasswordByEmailAsync(
                "ALICE-RENAMED@example.test",
                "hash-three",
                TestContext.Current.CancellationToken);
            Assert.Equal(
                alice.Id,
                (await context.UserRepository.GetUserByUsernameOrEmailAsync(
                    "ALICE-RENAMED@example.test",
                    TestContext.Current.CancellationToken)).Id);

            var account = await context.AccountRepository.GetUserByNameAsync(
                "alice renamed",
                TestContext.Current.CancellationToken);
            account.Username = "Alice Account";
            account.Email = "alice-account@example.test";
            account.Password = "must-not-replace";
            account.Roles = ["must-not-replace"];
            Assert.Null(await context.AccountRepository.UpdateAccountAsync(
                account,
                TestContext.Current.CancellationToken));
            var updated = await context.AccountRepository.GetUserByNameAsync(
                "ALICE ACCOUNT",
                TestContext.Current.CancellationToken);
            Assert.Equal("hash-three", updated.Password);
            Assert.Equal(["Admin"], updated.Roles);

            updated.Username = bob.Username;
            Assert.Contains(
                "Username",
                await context.AccountRepository.UpdateAccountAsync(
                    updated,
                    TestContext.Current.CancellationToken));
            updated.Username = "Alice Account";
            updated.Email = bob.Email;
            Assert.Contains(
                "Email",
                await context.AccountRepository.UpdateAccountAsync(
                    updated,
                    TestContext.Current.CancellationToken));

            await context.AccountRepository.ChangePasswordAsync(
                alice.Id,
                "hash-four",
                TestContext.Current.CancellationToken);
            Assert.Equal(
                "hash-four",
                (await context.UserRepository.GetUserForEditAsync(
                    alice.Id,
                    TestContext.Current.CancellationToken)).Password);

            await context.UserRepository.RemoveUserAsync(
                bob.Id,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                alice.Id,
                Assert.Single(await context.UserRepository.GetAllUsersAsync(
                    TestContext.Current.CancellationToken)).Id);
        });
    }

    [Fact]
    public async Task Config_country_and_email_template_repositories_preserve_values()
    {
        await ForEachContextAsync(async context =>
        {
            await context.SeedAsync(
                "config",
            [
                new Config { MainSite = new MainSiteConfig { Url = "https://before.example.test" } }
            ], TestContext.Current.CancellationToken);
            await context.SeedAsync(
                "countries",
            [
                new Country { Code = "US", DisplayText = "United States", Region = "ROW" },
                new Country { Code = "GB", DisplayText = "United Kingdom", Region = "UK" }
            ], TestContext.Current.CancellationToken);
            var template = new EmailTemplate
            {
                EmailType = EmailTypes.PasswordReset,
                Subject = "Original subject",
                Body = "Original body"
            };
            await context.SeedAsync(
                "emailtemplates",
                [template],
                TestContext.Current.CancellationToken);

            var config = await context.ConfigAdminRepository.GetConfigAsync(
                TestContext.Current.CancellationToken);
            config.MainSite.Url = "https://after.example.test";
            await context.ConfigAdminRepository.SetConfigAsync(
                config,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                "https://after.example.test",
                (await context.ConfigAdminRepository.GetConfigAsync(
                    TestContext.Current.CancellationToken)).MainSite.Url);

            Assert.Equal(
                ["United Kingdom", "United States"],
                (await context.CountryRepository.GetCountriesAsync(
                    TestContext.Current.CancellationToken)).Select(country => country.DisplayText));
            Assert.Equal(
                "UK",
                (await context.CountryRepository.GetCountryByCodeAsync(
                    "GB",
                    TestContext.Current.CancellationToken)).Region);

            var listed = Assert.Single(await context.EmailTemplateAdminRepository.GetTemplatesAsync(
                TestContext.Current.CancellationToken));
            Assert.Equal(
                "Original subject",
                (await context.EmailTemplateRepository.GetEmailTemplateAsync(
                    nameof(EmailTypes.PasswordReset),
                    TestContext.Current.CancellationToken)).Subject);
            listed.Subject = "Updated subject";
            await context.EmailTemplateAdminRepository.UpdateTemplateAsync(
                listed,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                "Updated subject",
                (await context.EmailTemplateAdminRepository.GetTemplateForEditAsync(
                    listed.Id,
                    TestContext.Current.CancellationToken)).Subject);
        });
    }

    [Fact]
    public async Task Password_reset_codes_round_trip_and_can_be_removed()
    {
        await ForEachContextAsync(async context =>
        {
            var code = await context.PasswordResetRepository.CreateResetPasswordCodeAsync(
                new AdminUser { Username = "reset-user", Email = "reset@example.test" },
                TestContext.Current.CancellationToken);
            var reset = await context.PasswordResetRepository.GetCodeAsync(
                code,
                TestContext.Current.CancellationToken);

            Assert.Equal("reset-user", reset.Username);
            Assert.Equal("reset@example.test", reset.Email);
            Assert.True(reset.Valid);

            await context.PasswordResetRepository.RemoveCodeAsync(
                code,
                TestContext.Current.CancellationToken);
            Assert.Null(await context.PasswordResetRepository.GetCodeAsync(
                code,
                TestContext.Current.CancellationToken));
        });
    }

    [Fact]
    public async Task Logging_repository_pages_newest_first_and_loads_details()
    {
        await ForEachContextAsync(async context =>
        {
            var older = new SerilogEntry
            {
                DateTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                Level = "Warning",
                Message = "Older"
            };
            var newer = new SerilogEntry
            {
                DateTime = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
                Level = "Error",
                Message = "Newer"
            };
            await context.SeedAsync(
                "serilog",
                [older, newer],
                TestContext.Current.CancellationToken);

            var page = await context.LoggingRepository.GetLogsAsync(
                new PagedRequest { Page = 1, PageSize = 1 },
                TestContext.Current.CancellationToken);

            Assert.Equal(2, page.TotalPages);
            Assert.Equal("Newer", Assert.Single(page.Items).Message);
            Assert.Equal(
                "Newer",
                (await context.LoggingRepository.GetLogAsync(
                    newer.Id,
                    TestContext.Current.CancellationToken)).Message);
        });
    }

    [Fact]
    public async Task Backup_returns_only_requested_registered_logical_stores()
    {
        await ForEachContextAsync(async context =>
        {
            await context.SeedAsync(
                "emailtemplates",
            [
                new EmailTemplate
                {
                    EmailType = EmailTypes.AdminOrder,
                    Subject = "Backup subject"
                }
            ], TestContext.Current.CancellationToken);

            var backup = (IDictionary<string, object>)await context.BackupRepository.GetAllDataAsync(
                ["emailtemplates"],
                TestContext.Current.CancellationToken);

            Assert.Single(backup);
            var templates = Assert.IsAssignableFrom<IEnumerable<EmailTemplate>>(backup["emailtemplates"]);
            Assert.Equal("Backup subject", Assert.Single(templates).Subject);
        });
    }

    private async Task ForEachContextAsync(Func<ICommonRepositoryContractContext, Task> assertion)
    {
        foreach (var context in CreateContractContexts())
        {
            await using (context)
            {
                try
                {
                    await assertion(context);
                }
                catch (Exception exception)
                {
                    throw new Xunit.Sdk.XunitException(
                        $"Persistence provider '{context.ProviderName}' failed: {exception}");
                }
            }
        }
    }
}
