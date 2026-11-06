using Xunit;

namespace HypnoTech.Demo.EndToEndTests;

[CollectionDefinition(Name)]
public sealed class DemoEndToEndCollection : ICollectionFixture<DemoEndToEndFixture>
{
    public const string Name = "Demo end-to-end";
}
