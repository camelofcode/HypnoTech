using System.Collections.Generic;
using HypnoTech.Media.Models;

namespace HypnoTech.Demo.Website.ViewModels;

public sealed class MediaViewModel
{
    public IReadOnlyCollection<AudioTrack> Audio { get; init; }

    public IReadOnlyCollection<PhotoAlbum> PhotoAlbums { get; init; }
}
