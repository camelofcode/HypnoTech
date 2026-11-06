using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HypnoTech.Common.Emails
{
    public static class StartupExtensions
    {
        public static void UseEmails(this IServiceCollection services)
        {
            services.TryAddTransient<IEmailService, EmailService>();
        }
    }
}
