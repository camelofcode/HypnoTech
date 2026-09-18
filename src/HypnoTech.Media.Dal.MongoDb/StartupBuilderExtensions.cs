using System;
using HypnoTech.Common.Extensions;
using HypnoTech.Common.Models;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace HypnoTech.Media.Dal.MongoDb
{
    public static class StartupBuilderExtensions
    {
        public static void UseMongoDbForMedia(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetSection("Mongo").Get<MongoOptions>() != null)
            {
                services.Configure<MongoOptions>(configuration.GetSection("Mongo"));

                services.AddPublicImplementedInterfaces(
                    typeof(StartupBuilderExtensions).Assembly,
                    type => type.Name.EndsWith("Repository", StringComparison.Ordinal));

                Map(configuration);
            }
        }

        private static void Map(IConfiguration configuration)
        {
            if (configuration.GetValue("Media:AudioEnabled", false))
            {
                BsonClassMap.RegisterClassMap<AudioTrack>(cm =>
                {
                    cm.MapMember(c => c.Id).SetElementName("_id")
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(c => c.RelatedId).SetElementName("related")
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(c => c.Title).SetElementName("title");
                    cm.MapMember(c => c.Path).SetElementName("path");
                });
                ModelRegistry.Register(typeof(AudioTrack), "audiotracks", "Audio Tracks");
            }

            if (configuration.GetValue("Media:YouTubeVideosEnabled", false))
            {
                BsonClassMap.RegisterClassMap<YouTubeVideo>(cm =>
                {
                    cm.MapMember(c => c.Id).SetElementName("_id")
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(c => c.Title).SetElementName("title");
                    cm.MapMember(c => c.YouTubeId).SetElementName("youTubeId");
                });
                ModelRegistry.Register(typeof(YouTubeVideo), "videos", "Videos");
            }

            if (configuration.GetValue("Media:PhotoAlbumsEnabled", false))
            {
                BsonClassMap.RegisterClassMap<PhotoAlbum>(cm =>
                {
                    cm.MapMember(c => c.Id).SetElementName("_id")
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(c => c.Date).SetElementName("date")
                        .SetSerializer(new DateTimeSerializer(BsonType.DateTime));
                    cm.MapMember(c => c.Url).SetElementName("url");
                    cm.MapMember(c => c.Title).SetElementName("title");
                    cm.MapMember(c => c.Photos).SetElementName("photos");
                });
                ModelRegistry.Register(typeof(PhotoAlbum), "photoalbums", "Photo Albums");

                BsonClassMap.RegisterClassMap<Photo>(cm =>
                {
                    cm.MapMember(c => c.Id).SetElementName("_id")
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(c => c.ImagePath).SetElementName("imagePath");
                    cm.MapMember(c => c.Caption).SetElementName("caption");
                });
            }
        }
    }
}
