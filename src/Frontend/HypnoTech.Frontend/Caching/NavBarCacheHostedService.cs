using System;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Frontend.Caching
{
    internal sealed class NavBarCacheHostedService : IHostedService
    {
        private readonly IServiceProvider _services;

        public NavBarCacheHostedService(IServiceProvider services)
        {
            _services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            NavBarCache.InitAsync(
                _services.GetService<INavBarRepository>(),
                cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
