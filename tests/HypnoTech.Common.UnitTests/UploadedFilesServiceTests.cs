using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HypnoTech.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HypnoTech.Common.UnitTests;

public sealed class UploadedFilesServiceTests : IDisposable
{
    private readonly string _uploadPath = Directory.CreateTempSubdirectory("hypnotech-uploads-").FullName;

    [Fact]
    public async Task GetImageAsync_serves_a_file_below_the_configured_category()
    {
        var imageDirectory = Directory.CreateDirectory(Path.Combine(_uploadPath, "Images", "articles"));
        var expected = new byte[] { 1, 2, 3 };
        await File.WriteAllBytesAsync(Path.Combine(imageDirectory.FullName, "cover.jpg"), expected, TestContext.Current.CancellationToken);

        var result = await CreateService().GetImageAsync("articles", "cover.jpg", TestContext.Current.CancellationToken);

        var file = Assert.IsType<Microsoft.AspNetCore.Mvc.FileContentResult>(result);
        Assert.Equal(expected, file.FileContents);
        Assert.Equal("image/jpeg", file.ContentType);
    }

    [Fact]
    public async Task GetImageAsync_rejects_a_path_that_escapes_the_image_directory()
    {
        await File.WriteAllTextAsync(Path.Combine(_uploadPath, "secret.txt"), "not public", TestContext.Current.CancellationToken);

        var result = await CreateService().GetImageAsync("..", "secret.txt", TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    public void Dispose()
    {
        Directory.Delete(_uploadPath, true);
        GC.SuppressFinalize(this);
    }

    private UploadedFilesService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["UploadPath"] = _uploadPath
            })
            .Build();

        return new UploadedFilesService(configuration, NullLogger<UploadedFilesService>.Instance);
    }
}
