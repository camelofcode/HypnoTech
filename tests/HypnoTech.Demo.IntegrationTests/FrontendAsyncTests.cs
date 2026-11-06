using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.Frontend.Ajax;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HypnoTech.Demo.IntegrationTests;

public sealed class FrontendAsyncTests
{
    [Fact]
    public async Task ExecuteAsync_writes_partial_json_without_synchronous_writer_calls()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.test");
        httpContext.Request.Path = "/stories";
        httpContext.Response.Body = new MemoryStream();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ControllerActionDescriptor());
        var controller = new TestController
        {
            ControllerContext = new ControllerContext(actionContext)
        };
        var result = new AjaxPartialViewResult(controller, "Index");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["CanonicalUrl"] = "https://example.test"
            })
            .Build();
        var executor = new AjaxPartialViewResultExecutor(
            new RejectingSyncWriterFactory(),
            NullLogger<AjaxPartialViewResultExecutor>.Instance,
            configuration);

        await executor.ExecuteAsync(actionContext, result);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"canonicalUrl\":\"https://example.test/stories\"", json);
    }

    [Fact]
    public async Task Navbar_async_default_preserves_sync_only_implementation_compatibility()
    {
        INavBarRepository repository = new SyncOnlyNavBarRepository();

        var links = await repository.GetEnabledNavBarLinksAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal("synthetic", Assert.Single(links).Key);
    }

    private sealed class RejectingSyncWriterFactory : IHttpResponseStreamWriterFactory
    {
        public TextWriter CreateWriter(Stream stream, Encoding encoding) =>
            new RejectingSyncTextWriter();
    }

    private sealed class TestController : Controller
    {
    }

    private sealed class RejectingSyncTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) =>
            throw new InvalidDataException("Synchronous response writes are forbidden.");

        public override void Write(char[] buffer, int index, int count) =>
            throw new InvalidDataException("Synchronous response writes are forbidden.");

        public override void Write(string value) =>
            throw new InvalidDataException("Synchronous response writes are forbidden.");
    }

    private sealed class SyncOnlyNavBarRepository : INavBarRepository
    {
        public List<NavBarLink> GetEnabledNavBarLinks() =>
        [
            new NavBarLink("synthetic", "Synthetic")
        ];
    }
}
