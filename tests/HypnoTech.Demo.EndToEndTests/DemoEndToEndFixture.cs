using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Playwright;
using Xunit;

namespace HypnoTech.Demo.EndToEndTests;

public sealed class DemoEndToEndFixture : IAsyncLifetime
{
    private const int MongoPort = 27017;

    private readonly IContainer _mongo = new ContainerBuilder("mongo:8.0.32-noble")
        .WithPortBinding(MongoPort, assignRandomHostPort: true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MongoPort))
        .Build();
    private readonly ConcurrentQueue<string> _applicationLogs = new();
    private readonly string _certificatePassword = Guid.NewGuid().ToString("N");
    private readonly string _certificatePath = Path.Combine(
        Path.GetTempPath(),
        $"hypnotech-e2e-{Guid.NewGuid():N}.pfx");
    private readonly string _databaseName = $"hypnotech_e2e_{Guid.NewGuid():N}";
    private readonly string _uploadPath = Path.Combine(
        Path.GetTempPath(),
        $"hypnotech-e2e-uploads-{Guid.NewGuid():N}");
    private Process _adminProcess;
    private Process _websiteProcess;
    private IPlaywright _playwright;

    public DemoEndToEndFixture()
    {
        BrowserName = (Environment.GetEnvironmentVariable("PLAYWRIGHT_BROWSER") ?? "chromium")
            .ToLowerInvariant();
        if (BrowserName is not ("chromium" or "firefox" or "webkit"))
        {
            throw new ArgumentException(
                $"Unsupported PLAYWRIGHT_BROWSER value '{BrowserName}'. " +
                "Expected chromium, firefox, or webkit.");
        }

        RepositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        AdminPort = GetAvailablePort();
        WebsitePort = GetAvailablePort();
    }

    public string RepositoryRoot { get; }

    public int AdminPort { get; }

    public int WebsitePort { get; }

    public string AdminUrl => $"https://localhost:{AdminPort}";

    public string WebsiteUrl => $"https://localhost:{WebsitePort}";

    public string BrowserName { get; }

    public IBrowser Browser { get; private set; }

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(_uploadPath);
        CreateHttpsCertificate();
        await _mongo.StartAsync();

        _adminProcess = StartApplication(
            "admin",
            "examples/HypnoTech.Demo.Admin/HypnoTech.Demo.Admin.csproj",
            AdminPort);
        await WaitForReadyAsync("admin", $"{AdminUrl}/Home/Login", _adminProcess);

        _websiteProcess = StartApplication(
            "website",
            "examples/HypnoTech.Demo.Website/HypnoTech.Demo.Website.csproj",
            WebsitePort);
        await WaitForReadyAsync("website", WebsiteUrl, _websiteProcess);

        _playwright = await Playwright.CreateAsync();
        var browserType = BrowserName switch
        {
            "firefox" => _playwright.Firefox,
            "webkit" => _playwright.Webkit,
            _ => _playwright.Chromium
        };
        Browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();
        StopApplication(_websiteProcess);
        StopApplication(_adminProcess);
        await _mongo.DisposeAsync();

        if (Directory.Exists(_uploadPath))
        {
            Directory.Delete(_uploadPath, recursive: true);
        }

        if (File.Exists(_certificatePath))
        {
            File.Delete(_certificatePath);
        }
    }

    public string CreateArtifactDirectory(string testName)
    {
        var path = GetArtifactDirectory(testName);
        Directory.CreateDirectory(path);
        return path;
    }

    public void ClearArtifactDirectory(string testName)
    {
        var path = GetArtifactDirectory(testName);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    public void WriteApplicationLogs(string artifactDirectory)
    {
        File.WriteAllLines(
            Path.Combine(artifactDirectory, "applications.log"),
            _applicationLogs);
    }

    private Process StartApplication(string name, string project, int port)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = RepositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(Path.Combine(RepositoryRoot, project));
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-launch-profile");

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = $"https://127.0.0.1:{port}";
        startInfo.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = _certificatePath;
        startInfo.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = _certificatePassword;
        startInfo.Environment["Mongo__Server"] = _mongo.Hostname;
        startInfo.Environment["Mongo__Port"] = _mongo.GetMappedPublicPort(MongoPort).ToString();
        startInfo.Environment["Mongo__Database"] = _databaseName;
        startInfo.Environment["UploadPath"] = _uploadPath;
        startInfo.Environment["HostURL"] = name == "admin" ? AdminUrl : WebsiteUrl;
        startInfo.Environment["CanonicalUrl"] = WebsiteUrl;
        startInfo.Environment["Cms__AdminUrl"] = AdminUrl;

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, args) => RecordApplicationLog(name, args.Data);
        process.ErrorDataReceived += (_, args) => RecordApplicationLog(name, args.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private async Task WaitForReadyAsync(string name, string url, Process process)
    {
        using var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
        for (var attempt = 0; attempt < 120; attempt++)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"The {name} application exited during startup.{Environment.NewLine}" +
                    string.Join(Environment.NewLine, _applicationLogs));
            }

            try
            {
                using var response = await client.GetAsync(url);
                if ((int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(250);
        }

        throw new TimeoutException(
            $"The {name} application did not become ready.{Environment.NewLine}" +
            string.Join(Environment.NewLine, _applicationLogs));
    }

    private void RecordApplicationLog(string name, string line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            _applicationLogs.Enqueue($"[{DateTimeOffset.UtcNow:O}] [{name}] {line}");
        }
    }

    private void CreateHttpsCertificate()
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=localhost",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new("1.3.6.1.5.5.7.3.1") },
                false));

        var subjectAlternativeName = new SubjectAlternativeNameBuilder();
        subjectAlternativeName.AddDnsName("localhost");
        subjectAlternativeName.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(subjectAlternativeName.Build());

        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddDays(1));
        File.WriteAllBytes(
            _certificatePath,
            certificate.Export(X509ContentType.Pfx, _certificatePassword));
    }

    private string GetArtifactDirectory(string testName)
    {
        var safeName = string.Concat(testName.Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_'));
        return Path.Combine(RepositoryRoot, "TestResults", "Playwright", BrowserName, safeName);
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static void StopApplication(Process process)
    {
        if (process == null)
        {
            return;
        }

        if (!process.HasExited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }

        process.Dispose();
    }
}
