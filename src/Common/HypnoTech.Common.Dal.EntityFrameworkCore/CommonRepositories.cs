using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Common.Dal.EntityFrameworkCore
{
    public sealed class NavBarRepository : INavBarRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public NavBarRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<List<NavBarLink>> GetEnabledNavBarLinksAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<NavBarLink>().AsNoTracking()
                .Where(link => link.Enabled)
                .OrderBy(link => link.Order)
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class NavBarAdminRepository : INavBarAdminRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public NavBarAdminRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<List<NavBarLink>> GetNavBarLinksAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<NavBarLink>().AsNoTracking()
                .OrderBy(link => link.Order)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateNavBarLinksAsync(
            List<NavBarLink> links,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Set<NavBarLink>().ExecuteDeleteAsync(cancellationToken);
            foreach (var link in links)
            {
                link.Id = Guid.NewGuid().ToString("N");
            }

            context.AddRange(links);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public sealed class ConfigAdminRepository : IConfigAdminRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public ConfigAdminRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<Config> GetConfigAsync(CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<Config>().AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        }

        public async Task SetConfigAsync(
            Config config,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Set<Config>().ExecuteDeleteAsync(cancellationToken);
            context.Add(config);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public sealed class CountryRepository : ICountryRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public CountryRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<List<Country>> GetCountriesAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<Country>().AsNoTracking()
                .OrderBy(country => country.DisplayText)
                .ToListAsync(cancellationToken);
        }

        public async Task<Country> GetCountryByCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<Country>().AsNoTracking()
                .SingleOrDefaultAsync(country => country.Code == code, cancellationToken);
        }
    }

    public sealed class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public EmailTemplateRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<EmailTemplate> GetEmailTemplateAsync(
            string type,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            if (!Enum.TryParse<EmailTypes>(type, out var emailType))
            {
                return null;
            }

            return await context.Set<EmailTemplate>().AsNoTracking()
                .SingleOrDefaultAsync(template => template.EmailType == emailType, cancellationToken);
        }
    }

    public sealed class EmailTemplateAdminRepository : IEmailTemplateAdminRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public EmailTemplateAdminRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<List<EmailTemplate>> GetTemplatesAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<EmailTemplate>().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<EmailTemplate> GetTemplateForEditAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<EmailTemplate>().AsNoTracking()
                .SingleOrDefaultAsync(template => template.Id == id, cancellationToken);
        }

        public async Task UpdateTemplateAsync(
            EmailTemplate profile,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            context.Update(profile);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public sealed class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public PasswordResetRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<string> CreateResetPasswordCodeAsync(
            AdminUser user,
            CancellationToken cancellationToken = default)
        {
            var reset = new PasswordReset(user);
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            context.Add(reset);
            await context.SaveChangesAsync(cancellationToken);
            return reset.Code;
        }

        public async Task<PasswordReset> GetCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<PasswordReset>().AsNoTracking()
                .SingleOrDefaultAsync(reset => reset.Code == code, cancellationToken);
        }

        public async Task RemoveCodeAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            await context.Set<PasswordReset>().Where(reset => reset.Code == code)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    public sealed class LoggingRepository : ILoggingRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public LoggingRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<PagedListResponse<SerilogEntry>> GetLogsAsync(
            PagedRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var query = context.Set<SerilogEntry>().AsNoTracking().OrderByDescending(entry => entry.DateTime);
            var count = await query.CountAsync(cancellationToken);
            return new PagedListResponse<SerilogEntry>
            {
                Items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync(cancellationToken),
                CurrentPage = request.Page,
                TotalPages = (int)Math.Ceiling(count / (decimal)request.PageSize)
            };
        }

        public async Task<SerilogEntry> GetLogAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<SerilogEntry>().AsNoTracking()
                .SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken);
        }
    }

    public sealed class BackupRepository : IBackupRespository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public BackupRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<dynamic> GetAllDataAsync(
            List<string> includedTypes,
            CancellationToken cancellationToken = default)
        {
            IDictionary<string, object> result = new ExpandoObject();
            var method = typeof(BackupRepository).GetMethod(
                nameof(GetAllAsObjectAsync),
                BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var registration in ModelRegistry.Registrations.Where(registration =>
                         includedTypes.Contains(registration.DataName)))
            {
                var task = (Task<object>)method.MakeGenericMethod(registration.Type)
                    .Invoke(this, [cancellationToken]);
                result[registration.DataName] = await task;
            }

            return (ExpandoObject)result;
        }

        private async Task<object> GetAllAsObjectAsync<T>(CancellationToken cancellationToken)
            where T : class
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}
