using System.Linq;
using HypnoTech.Demo.Website.ViewModels;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Media.Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class MediaController : Controller
{
    private readonly IPhotoAlbumService _photos;
    private readonly IVideoService _videos;

    public MediaController(IPhotoAlbumService photos, IVideoService videos)
    {
        _photos = photos;
        _videos = videos;
    }

    [NavigationLink("media")]
    public IActionResult Index()
    {
        return View(new MediaViewModel
        {
            PhotoAlbums = _photos.GetPhotoAlbums().ToList(),
            Videos = _videos.GetVideos().ToList()
        });
    }
}
