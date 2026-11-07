using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace HypnoTech.ArchitectureTests;

public sealed class RepositoryArchitectureTests
{
    [Fact]
    public void Source_projects_target_net10()
    {
        var projectFiles = Directory.GetFiles(Path.Combine(FindRepositoryRoot(), "src"), "*.csproj", SearchOption.AllDirectories);

        var incorrectlyTargeted = projectFiles
            .Where(path => (string)XDocument.Load(path).Root?.Element("PropertyGroup")?.Element("TargetFramework") != "net10.0")
            .Select(Path.GetFileName)
            .ToList();

        Assert.NotEmpty(projectFiles);
        Assert.Empty(incorrectlyTargeted);
    }

    [Fact]
    public void Contract_and_dal_abstraction_projects_do_not_reference_provider_implementations()
    {
        var root = FindRepositoryRoot();
        var abstractionProjects = new[]
        {
            "src/Cms/HypnoTech.Cms.Contracts/HypnoTech.Cms.Contracts.csproj",
            "src/Common/HypnoTech.Common.Dal/HypnoTech.Common.Dal.csproj",
            "src/Ecommerce/HypnoTech.Ecommerce.Dal/HypnoTech.Ecommerce.Dal.csproj",
            "src/Media/HypnoTech.Media.Dal/HypnoTech.Media.Dal.csproj"
        };
        var violations = new List<string>();

        foreach (var relativePath in abstractionProjects)
        {
            var document = XDocument.Load(Path.Combine(root, relativePath));
            var implementationReferences = document
                .Descendants("ProjectReference")
                .Select(reference => (string)reference.Attribute("Include"))
                .Where(reference => reference != null &&
                    (reference.Contains("MongoDb", StringComparison.OrdinalIgnoreCase) ||
                     reference.Contains("HypnoTech.Cms\\", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            violations.AddRange(implementationReferences.Select(reference => $"{relativePath}: {reference}"));
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void Request_path_source_does_not_block_on_tasks()
    {
        var root = FindRepositoryRoot();
        var scopedDirectories = new[]
        {
            "src/Cms",
            "src/Common",
            "src/Persistence",
            "src/Ecommerce",
            "src/Media",
            "src/Frontend",
            "examples/HypnoTech.Demo.Website"
        };
        var blockingPattern = new Regex(
            @"\.Wait\s*\(|\.GetAwaiter\s*\(\s*\)\s*\.GetResult\s*\(\s*\)|\.Result\b(?!\s*[=<])",
            RegexOptions.Compiled);

        var violations = scopedDirectories
            .SelectMany(directory => Directory.GetFiles(
                Path.Combine(root, directory),
                "*.cs",
                SearchOption.AllDirectories))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new { path, line, number = index + 1 }))
            .Where(candidate => blockingPattern.IsMatch(candidate.line))
            .Select(candidate =>
                $"{Path.GetRelativePath(root, candidate.path)}:{candidate.number}: {candidate.line.Trim()}")
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Server_options_do_not_enable_synchronous_io()
    {
        var root = FindRepositoryRoot();
        var sourceDirectories = new[] { "src", "examples" };
        var violations = sourceDirectories
            .SelectMany(directory => Directory.GetFiles(
                Path.Combine(root, directory),
                "*.cs",
                SearchOption.AllDirectories))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new { path, line, number = index + 1 }))
            .Where(candidate => candidate.line.Contains(
                "AllowSynchronousIO = true",
                StringComparison.Ordinal))
            .Select(candidate =>
                $"{Path.GetRelativePath(root, candidate.path)}:{candidate.number}: {candidate.line.Trim()}")
            .ToList();

        Assert.Empty(violations);
    }

    [Fact]
    public void Public_io_contracts_are_async_only()
    {
        var root = FindRepositoryRoot();
        var contractDirectories = new[]
        {
            "src/Cms/HypnoTech.Cms.Contracts/Interfaces",
            "src/Cms/HypnoTech.Cms/Services/Interfaces",
            "src/Common/HypnoTech.Common.Dal/Repositories",
            "src/Ecommerce/HypnoTech.Ecommerce.Dal/Repositories",
            "src/Ecommerce/HypnoTech.Ecommerce.Frontend/Services/Interfaces",
            "src/Media/HypnoTech.Media.Dal/Repositories"
        };
        var contractFiles = contractDirectories
            .SelectMany(directory => Directory.GetFiles(
                Path.Combine(root, directory),
                "*.cs",
                SearchOption.AllDirectories))
            .Concat(new[]
            {
                Path.Combine(root, "src/Common/HypnoTech.Common.Emails/IEmailService.cs"),
                Path.Combine(root, "src/Common/HypnoTech.Common/Services/IUploadedFilesService.cs")
            });
        var methodPattern = new Regex(
            @"^[ \t]*(?:[\w<>,\[\]?]+[ \t]+)+(?<name>\w+)[ \t]*\([^;{}]*\)[ \t]*;",
            RegexOptions.Compiled | RegexOptions.Multiline);

        var violations = contractFiles
            .SelectMany(path => methodPattern.Matches(File.ReadAllText(path))
                .Cast<Match>()
                .Where(match => !match.Groups["name"].Value.EndsWith(
                    "Async",
                    StringComparison.Ordinal))
                .Select(match =>
                    $"{Path.GetRelativePath(root, path)}: {match.Groups["name"].Value}"))
            .ToList();

        Assert.Empty(violations);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "HypnoTech.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find the HypnoTech repository root.");
    }
}
