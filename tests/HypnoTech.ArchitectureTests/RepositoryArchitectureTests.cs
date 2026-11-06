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
    public void Converted_request_path_source_does_not_block_on_tasks()
    {
        var root = FindRepositoryRoot();
        var scopedDirectories = new[]
        {
            "src/Cms",
            "src/Common",
            "src/Persistence",
            "src/Ecommerce",
            "src/Media",
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
            .Where(candidate => !IsApprovedPayPalV1CompatibilityAdapter(candidate.path, candidate.line))
            .Select(candidate =>
                $"{Path.GetRelativePath(root, candidate.path)}:{candidate.number}: {candidate.line.Trim()}")
            .ToList();

        Assert.Empty(violations);
    }

    private static bool IsApprovedPayPalV1CompatibilityAdapter(string path, string line)
    {
        return Path.GetFileName(path) == "PayPalService.cs" &&
               line.Contains("client.Execute(request).Result", StringComparison.Ordinal);
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
