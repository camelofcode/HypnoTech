using System;
using HypnoTech.Common.Extensions;
using HypnoTech.Common.Models;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace HypnoTech.Ecommerce.Dal.MongoDb
{
    public static class StartupBuilderExtensions
    {
        public static void UseMongoDbForEcommerce(this IServiceCollection services, IConfiguration configuration)
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
            BsonClassMap.RegisterClassMap<PostageProfile>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.UnitedKingdom).SetElementName("uk");
                cm.MapMember(c => c.EuropeanUnion).SetElementName("eu");
                cm.MapMember(c => c.RestOfWorld1).SetElementName("row1");
                cm.MapMember(c => c.RestOfWorld2).SetElementName("row2");
                cm.MapMember(c => c.RestOfWorld3).SetElementName("row3");
            });

            BsonClassMap.RegisterClassMap<PostageRegion>(cm =>
            {
                cm.MapMember(c => c.Alone).SetElementName("alone").SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                cm.MapMember(c => c.WithOthers).SetElementName("withOthers").SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });

            BsonClassMap.RegisterClassMap<DownloadCodeCollection>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.RedemptionUrl).SetElementName("redemptionUrl");
                cm.MapMember(c => c.Codes).SetElementName("codes");
            });

            BsonClassMap.RegisterClassMap<DownloadCode>(cm =>
            {
                cm.MapMember(c => c.Code).SetElementName("code");
                cm.MapMember(c => c.AssignedTo).SetElementName("assignedto");
            });

            BsonClassMap.RegisterClassMap<Order>(cm =>
            {
                cm.MapMember(c => c.DateTime).SetElementName("createdDate").SetSerializer(new DateTimeSerializer(BsonType.DateTime));
                cm.MapMember(c => c.PaymentId).SetElementName("paymentId");
                cm.MapMember(c => c.SaleId).SetElementName("saleId");
                cm.MapMember(c => c.SaleUrl).SetElementName("saleUrl");
                cm.MapMember(c => c.PayerId).SetElementName("payerId");
                cm.MapMember(c => c.Status).SetElementName("status");
                cm.MapMember(c => c.Tracked).SetElementName("tracked").SetSerializer(new BooleanSerializer(BsonType.Boolean));
                cm.MapMember(c => c.Postage).SetElementName("postage").SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                cm.MapMember(c => c.Total).SetElementName("total").SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                cm.MapMember(c => c.SubTotal).SetElementName("subTotal").SetSerializer(new DecimalSerializer(BsonType.Decimal128));
                cm.MapMember(c => c.EmailAddress).SetElementName("emailAddress");
                cm.MapMember(c => c.Items).SetElementName("items");
                cm.MapMember(c => c.Address).SetElementName("address");
                cm.MapMember(c => c.DownloadCodeIds).SetElementName("downloadCodeIds");
            });

            BsonClassMap.RegisterClassMap<OrderItem>(cm =>
            {
                cm.MapMember(c => c.Item).SetElementName("item");
                cm.MapMember(c => c.Quantity).SetElementName("quantity").SetSerializer(new Int32Serializer(BsonType.Int32));
                cm.MapMember(c => c.RowPrice).SetElementName("rowPrice").SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });

            BsonClassMap.RegisterClassMap<PostageAddress>(cm =>
            {
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.Address1).SetElementName("address1");
                cm.MapMember(c => c.Address2).SetElementName("address2");
                cm.MapMember(c => c.Town).SetElementName("town");
                cm.MapMember(c => c.CountryCode).SetElementName("countryCode");
                cm.MapMember(c => c.Country).SetElementName("country");
                cm.MapMember(c => c.PostalCode).SetElementName("postalCode");
                cm.MapMember(c => c.Region).SetElementName("region");
            });

            BsonClassMap.RegisterClassMap<MerchItem>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.ImagePath).SetElementName("imagePath");
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.Description).SetElementName("description");
                cm.MapMember(c => c.Category).SetElementName("category");
                cm.MapMember(c => c.SubCategory).SetElementName("subcategory");
                cm.MapMember(c => c.Price).SetElementName("price");
                cm.MapMember(c => c.FormatId).SetElementName("formatId").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.PostageProfile).SetElementName("postageprofile");
                cm.MapMember(c => c.Options).SetElementName("options");
                cm.MapMember(c => c.DownloadCodeId).SetElementName("downloadCodeId");
                cm.MapMember(c => c.Slug).SetElementName("slug");
            });

            BsonClassMap.RegisterClassMap<MerchCategoryType>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.Order).SetElementName("order");
                cm.MapMember(c => c.SubcategoryTypes).SetElementName("subcategorytypes");
            });

            BsonClassMap.RegisterClassMap<MerchSubcategoryType>(cm =>
            {
                cm.MapMember(c => c.Name).SetElementName("name");
                cm.MapMember(c => c.Order).SetElementName("order");
            });

            BsonClassMap.RegisterClassMap<MerchOption>(cm =>
            {
                cm.MapMember(c => c.Id).SetElementName("_id").SetSerializer(new StringSerializer(BsonType.ObjectId));
                cm.MapMember(c => c.Name).SetElementName("name");
            });

            ModelRegistry.Register(typeof(MerchItem), "merches", "Merch");
            ModelRegistry.Register(typeof(MerchCategoryType), "merchcategorytypes", "Merch Categories");
            ModelRegistry.Register(typeof(PostageProfile), "postageprofiles", "Postage Profiles");
            ModelRegistry.Register(typeof(DownloadCodeCollection), "downloadcodes", "Download Codes");
        }
    }
}
