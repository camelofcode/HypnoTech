using Xunit;

namespace HypnoTech.Cms.EntityFrameworkCore.PostgreSql.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection :
    ICollectionFixture<PostgreSqlFixture>,
    ICollectionFixture<SqlServerFixture>
{
    public const string Name = "Relational persistence contracts";
}
