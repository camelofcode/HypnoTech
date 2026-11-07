using System;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace HypnoTech.Demo.Admin.Persistence;

internal sealed class DemoRelationalSeeder : IHypnoTechStartupSeeder
{
    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _environment;

    public DemoRelationalSeeder(
        IServiceProvider services,
        IWebHostEnvironment environment)
    {
        _services = services;
        _environment = environment;
    }

    public int Order => 100;

    public Task SeedAsync(CancellationToken cancellationToken = default) =>
        DemoSeeder.SeedAsync(_services, _environment, cancellationToken);
}
