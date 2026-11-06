using System.Collections.Generic;
using HypnoTech.Media.Models;

namespace HypnoTech.Demo.Website.ViewModels;

public sealed class MediaViewModel
{
    public IReadOnlyCollection<PhotoAlbum> PhotoAlbums { get; init; }

    public IReadOnlyCollection<YouTubeVideo> Videos { get; init; }
}
