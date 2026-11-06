using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace HypnoTech.Demo.IntegrationTests;

public sealed class DemoMongoFixture : IAsyncLifetime
{
    private const int MongoPort = 27017;

    private readonly IContainer _container = new ContainerBuilder("mongo:8.0.32-noble")
        .WithPortBinding(MongoPort, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MongoPort))
        .Build();

    public string Host => _container.Hostname;

    public ushort Port => _container.GetMappedPublicPort(MongoPort);

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
