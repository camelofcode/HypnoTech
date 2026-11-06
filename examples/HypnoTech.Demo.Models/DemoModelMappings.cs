using HypnoTech.Common.Models;
using HypnoTech.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace HypnoTech.Demo.Models;

public static class DemoModelMappings
{
    public static void Map()
    {
        BsonClassMapRegistration.RegisterIfNeeded<Article>(map =>
        {
            map.MapMember(model => model.Id).SetElementName("_id")
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(model => model.Title).SetElementName("title");
            map.MapMember(model => model.Summary).SetElementName("summary");
            map.MapMember(model => model.Body).SetElementName("body");
            map.MapMember(model => model.PublishedAt).SetElementName("publishedAt")
                .SetSerializer(new DateTimeSerializer(BsonType.DateTime));
            map.MapMember(model => model.HeroImagePath).SetElementName("heroImagePath");
            map.MapMember(model => model.Featured).SetElementName("featured")
                .SetSerializer(new BooleanSerializer(BsonType.Boolean));
            map.MapMember(model => model.Slug).SetElementName("slug");
        });
        ModelRegistry.Register(typeof(Article), "demoArticles", "Articles");

        BsonClassMapRegistration.RegisterIfNeeded<Artist>(map =>
        {
            map.MapMember(model => model.Id).SetElementName("_id")
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(model => model.Name).SetElementName("name");
            map.MapMember(model => model.Discipline).SetElementName("discipline");
            map.MapMember(model => model.Biography).SetElementName("biography");
            map.MapMember(model => model.ImagePath).SetElementName("imagePath");
            map.MapMember(model => model.Website).SetElementName("website");
            map.MapMember(model => model.Featured).SetElementName("featured")
                .SetSerializer(new BooleanSerializer(BsonType.Boolean));
            map.MapMember(model => model.Order).SetElementName("order")
                .SetSerializer(new Int32Serializer(BsonType.Int32));
        });
        ModelRegistry.Register(typeof(Artist), "demoArtists", "Artists");

        BsonClassMapRegistration.RegisterIfNeeded<Event>(map =>
        {
            map.MapMember(model => model.Id).SetElementName("_id")
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(model => model.Title).SetElementName("title");
            map.MapMember(model => model.StartsAt).SetElementName("startsAt")
                .SetSerializer(new DateTimeSerializer(BsonType.DateTime));
            map.MapMember(model => model.Type).SetElementName("type");
            map.MapMember(model => model.ArtistId).SetElementName("artistId")
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(model => model.Venue).SetElementName("venue");
            map.MapMember(model => model.Summary).SetElementName("summary");
            map.MapMember(model => model.Description).SetElementName("description");
            map.MapMember(model => model.TicketPrice).SetElementName("ticketPrice");
            map.MapMember(model => model.ImagePath).SetElementName("imagePath");
            map.MapMember(model => model.ProgrammePath).SetElementName("programmePath");
            map.MapMember(model => model.Featured).SetElementName("featured")
                .SetSerializer(new BooleanSerializer(BsonType.Boolean));
            map.MapMember(model => model.Reference).SetElementName("reference");
            map.MapMember(model => model.Slug).SetElementName("slug");
            map.MapMember(model => model.Schedule).SetElementName("schedule");
        });
        ModelRegistry.Register(typeof(Event), "demoEvents", "Events");

        BsonClassMapRegistration.RegisterIfNeeded<ScheduleItem>(map =>
        {
            map.MapMember(model => model.Id).SetElementName("_id")
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
            map.MapMember(model => model.Time).SetElementName("time");
            map.MapMember(model => model.Title).SetElementName("title");
            map.MapMember(model => model.Format).SetElementName("format");
            map.MapMember(model => model.Order).SetElementName("order")
                .SetSerializer(new Int32Serializer(BsonType.Int32));
        });
    }
}
