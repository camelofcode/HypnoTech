using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class MongoPersistenceContractCollection : ICollectionFixture<MongoCompatibilityFixture>
{
    public const string Name = "Mongo persistence contracts";
}
