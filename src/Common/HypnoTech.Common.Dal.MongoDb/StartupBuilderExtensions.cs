using System;
using HypnoTech.Common.Extensions;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace HypnoTech.Common.Dal.MongoDb
{
    public static class StartupBuilderExtensions
    {
        public static void UseMongoDbForCommon(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetSection("Mongo").Get<MongoOptions>() != null)
            {
                services.Configure<MongoOptions>(configuration.GetSection("Mongo"));

                services.AddPublicImplementedInterfaces(
                    typeof(StartupBuilderExtensions).Assembly,
                    type => type.Name.EndsWith("Repository", StringComparison.Ordinal));

                Map();
            }
        }

        private static void Map()
        {
            BsonClassMapRegistration.RegisterIfNeeded<AdminUser>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.Username).SetElementName("username");
                cm.MapMember(c => c.Password).SetElementName("password");
                cm.MapMember(c => c.Email).SetElementName("email");
                cm.MapMember(c => c.Roles).SetElementName("roles");
            });

            BsonClassMapRegistration.RegisterIfNeeded<NavBarLink>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.Key).SetElementName("key");
                cm.MapMember(c => c.Text).SetElementName("text");
                cm.MapMember(c => c.Order).SetElementName("order").SetSerializer(new Int32Serializer(BsonType.Int32));
                cm.MapMember(c => c.Enabled).SetElementName("enabled").SetSerializer(new BooleanSerializer(BsonType.Boolean));
            });

            BsonClassMapRegistration.RegisterIfNeeded<Country>(cm =>
            {
                cm.MapMember(c => c.DisplayText).SetElementName("displayText");
                cm.MapMember(c => c.Code).SetElementName("code");
                cm.MapMember(c => c.Region).SetElementName("region");
            });


            BsonClassMapRegistration.RegisterIfNeeded<SerilogEntry>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.DateTime).SetElementName("Timestamp").SetSerializer(new DateTimeSerializer(BsonType.DateTime));
                cm.MapMember(c => c.Level).SetElementName("Level");
                cm.MapMember(c => c.Message).SetElementName("RenderedMessage");
                cm.MapMember(c => c.Exception).SetElementName("Exception");
                cm.MapMember(c => c.Properties).SetElementName("Properties");
            });

            BsonClassMapRegistration.RegisterIfNeeded<SerilogProperties>(cm =>
            {
                cm.MapMember(c => c.SiteName).SetElementName("SiteName");
                cm.MapMember(c => c.Path).SetElementName("RequestPath");
            });


            BsonClassMapRegistration.RegisterIfNeeded<Config>(cm =>
            {
                cm.MapMember(c => c.Paypal).SetElementName("paypal");
                cm.MapMember(c => c.Email).SetElementName("email");
                cm.MapMember(c => c.MainSite).SetElementName("mainSite");
            });

            BsonClassMapRegistration.RegisterIfNeeded<PaypalConfig>(cm =>
            {
                cm.MapMember(c => c.ApiClientId).SetElementName("apiClientId");
                cm.MapMember(c => c.ApiClientSecret).SetElementName("apiClientSecret");
                cm.MapMember(c => c.ApiUrl).SetElementName("apiUrl");
                cm.MapMember(c => c.SalesUrl).SetElementName("salesUrl");
                cm.MapMember(c => c.PaymentCompleteUrl).SetElementName("paymentCompleteUrl");
                cm.MapMember(c => c.PaymentCancelledUrl).SetElementName("paymentCancelledUrl");
                cm.MapMember(c => c.IsProduction).SetElementName("production").SetSerializer(new BooleanSerializer(BsonType.Boolean));

            });

            BsonClassMapRegistration.RegisterIfNeeded<EmailConfig>(cm =>
            {
                cm.MapMember(c => c.EnableEmails).SetElementName("enabled").SetSerializer(new BooleanSerializer(BsonType.Boolean));
                cm.MapMember(c => c.SmtpServer).SetElementName("smtpServer");
                cm.MapMember(c => c.MailboxUser).SetElementName("mailboxUser");
                cm.MapMember(c => c.MailboxPassword).SetElementName("mailboxPassword");
                cm.MapMember(c => c.Sender).SetElementName("sender");
                cm.MapMember(c => c.DefaultRecipient).SetElementName("defaultRecipient");
            });

            BsonClassMapRegistration.RegisterIfNeeded<MainSiteConfig>(cm =>
            {
                cm.MapMember(c => c.Url).SetElementName("url");
                cm.MapMember(c => c.ApiKey).SetElementName("apiKey");
            });

            BsonClassMapRegistration.RegisterIfNeeded<PasswordReset>(cm =>
            {
                cm.MapMember(c => c.Code).SetElementName("code");
                cm.MapMember(c => c.Username).SetElementName("username");
                cm.MapMember(c => c.Email).SetElementName("email");
                cm.MapMember(c => c.Created).SetElementName("created").SetSerializer(new DateTimeSerializer(BsonType.DateTime));
            });

            BsonClassMapRegistration.RegisterIfNeeded<EmailTemplate>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.EmailType).SetElementName("type").SetSerializer(new EnumSerializer<EmailTypes>(BsonType.String));
                cm.MapMember(c => c.Subject).SetElementName("subject");
                cm.MapMember(c => c.Header).SetElementName("header");
                cm.MapMember(c => c.IntroLine).SetElementName("introline");
                cm.MapMember(c => c.Body).SetElementName("body");
                cm.MapMember(c => c.FooterText).SetElementName("footertext");
            });

            ModelRegistry.Register(typeof(Country), "countries", "Countries");
            ModelRegistry.Register(typeof(Config), "config", "Config");
            ModelRegistry.Register(typeof(EmailTemplate), "emailtemplates", "Email Templates");
        }
    }
}
