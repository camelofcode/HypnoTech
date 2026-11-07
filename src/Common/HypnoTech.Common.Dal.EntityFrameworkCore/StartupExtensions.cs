using HypnoTech.Common;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HypnoTech.Common.Dal.EntityFrameworkCore
{
    public static class StartupExtensions
    {
        public static IServiceCollection UseEntityFrameworkCoreForCommon(
            this IServiceCollection services)
        {
            ModelRegistry.Register(typeof(Country), "countries", "Countries");
            ModelRegistry.Register(typeof(Config), "config", "Config");
            ModelRegistry.Register(typeof(EmailTemplate), "emailtemplates", "Email Templates");

            services.TryAddTransient<INavBarRepository, NavBarRepository>();
            services.TryAddTransient<INavBarAdminRepository, NavBarAdminRepository>();
            services.TryAddTransient<IUserRepository, UserRepository>();
            services.TryAddTransient<IAccountRepository, AccountRepository>();
            services.TryAddTransient<IConfigAdminRepository, ConfigAdminRepository>();
            services.TryAddTransient<ICountryRepository, CountryRepository>();
            services.TryAddTransient<IEmailTemplateRepository, EmailTemplateRepository>();
            services.TryAddTransient<IEmailTemplateAdminRepository, EmailTemplateAdminRepository>();
            services.TryAddTransient<IPasswordResetRepository, PasswordResetRepository>();
            services.TryAddTransient<ILoggingRepository, LoggingRepository>();
            services.TryAddTransient<IBackupRespository, BackupRepository>();
            return services;
        }
    }
}
