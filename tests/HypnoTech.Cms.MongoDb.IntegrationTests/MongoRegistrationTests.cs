using System.Collections.Generic;
using HypnoTech.Cms.Contracts.Interfaces;
using HypnoTech.Cms.MongoDb;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Media.Dal.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

public sealed class MongoRegistrationTests
{
    [Fact]
    public void Provider_extensions_register_representative_cms_common_ecommerce_and_media_repositories()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        services.UseMongoDbForCms(configuration);
        HypnoTech.Common.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForCommon(services, configuration);
        HypnoTech.Ecommerce.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForEcommerce(services, configuration);
        HypnoTech.Media.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForMedia(services, configuration);

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ICmsRepository<>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ISelectListRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IUserRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMerchRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAudioTrackRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IVideoRepository));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPhotoAlbumRepository));

        var registryCount = HypnoTech.Common.Models.ModelRegistry.Registrations.Count;
        var secondServices = new ServiceCollection();

        secondServices.UseMongoDbForCms(configuration);
        HypnoTech.Common.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForCommon(secondServices, configuration);
        HypnoTech.Ecommerce.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForEcommerce(secondServices, configuration);
        HypnoTech.Media.Dal.MongoDb.StartupBuilderExtensions.UseMongoDbForMedia(secondServices, configuration);

        Assert.Contains(secondServices, descriptor => descriptor.ServiceType == typeof(IUserRepository));
        Assert.Contains(secondServices, descriptor => descriptor.ServiceType == typeof(IMerchRepository));
        Assert.Contains(secondServices, descriptor => descriptor.ServiceType == typeof(IAudioTrackRepository));
        Assert.Equal(registryCount, HypnoTech.Common.Models.ModelRegistry.Registrations.Count);
        Assert.Equal(
            "username",
            BsonClassMap.LookupClassMap(typeof(HypnoTech.Common.Models.Settings.AdminUser))
                .GetMemberMap(nameof(HypnoTech.Common.Models.Settings.AdminUser.Username))
                .ElementName);
        Assert.Equal(
            "imagePath",
            BsonClassMap.LookupClassMap(typeof(HypnoTech.Ecommerce.Models.MerchItem))
                .GetMemberMap(nameof(HypnoTech.Ecommerce.Models.MerchItem.ImagePath))
                .ElementName);
        Assert.Equal(
            "related",
            BsonClassMap.LookupClassMap(typeof(HypnoTech.Media.Models.AudioTrack))
                .GetMemberMap(nameof(HypnoTech.Media.Models.AudioTrack.RelatedId))
                .ElementName);
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Mongo:UseMongo"] = "true",
                ["Mongo:Server"] = "localhost",
                ["Mongo:Port"] = "27017",
                ["Mongo:Database"] = "hypnotech_registration_tests",
                ["Media:AudioEnabled"] = "true",
                ["Media:YouTubeVideosEnabled"] = "true",
                ["Media:PhotoAlbumsEnabled"] = "true"
            })
            .Build();
    }
}
