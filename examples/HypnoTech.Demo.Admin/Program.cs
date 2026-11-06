using System.Linq;
using System.Threading.Tasks;
using HypnoTech.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Demo.Admin;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var seedOnly = args.Contains("--seed-only");
        var hostArgs = args.Where(argument => argument != "--seed-only").ToArray();
        using var host = CreateHostBuilder(hostArgs).Build();

        if (seedOnly)
        {
            await host.StartAsync();
            await host.StopAsync();
            return;
        }

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .UseHypnoTechConfiguration("Demo Admin", args);
}
