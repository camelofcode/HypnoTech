using HypnoTech.Common.Models;

namespace HypnoTech.Demo.Models;

public static class DemoModelMappings
{
    public static void Map()
    {
        ModelRegistry.Register(typeof(Article), "demoArticles", "Articles");
        ModelRegistry.Register(typeof(Artist), "demoArtists", "Artists");
        ModelRegistry.Register(typeof(Event), "demoEvents", "Events");
    }
}
