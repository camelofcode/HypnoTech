using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace HypnoTech.Cms.MongoDb.IntegrationTests;

public sealed class MongoCompatibilityFixture : IAsyncLifetime
{
    private const int MongoPort = 27017;

    private readonly IContainer _production = BuildContainer("mongo:4.0.3");
    private readonly IContainer _supported = BuildContainer("mongo:8.0.32-noble");

    public IReadOnlyList<MongoTarget> Targets =>
    [
        new("production-4.0.3", _production.Hostname, _production.GetMappedPublicPort(MongoPort)),
        new("supported-8.0.32", _supported.Hostname, _supported.GetMappedPublicPort(MongoPort))
    ];

    public async ValueTask InitializeAsync()
    {
        await _production.StartAsync();
        await _supported.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _supported.DisposeAsync();
        await _production.DisposeAsync();
    }

    private static IContainer BuildContainer(string image)
    {
        return new ContainerBuilder(image)
            .WithPortBinding(MongoPort, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MongoPort))
            .Build();
    }
}

public sealed record MongoTarget(string Name, string Host, ushort Port);
