using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Demo.Admin;

internal sealed class DemoSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _environment;

    public DemoSeederHostedService(
        IServiceProvider services,
        IWebHostEnvironment environment)
    {
        _services = services;
        _environment = environment;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        DemoSeeder.SeedAsync(_services, _environment, cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
