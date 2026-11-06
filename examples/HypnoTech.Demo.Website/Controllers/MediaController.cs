using System.Linq;
using HypnoTech.Demo.Website.ViewModels;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Media.Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class MediaController : Controller
{
    private readonly IAudioTrackService _audio;
    private readonly IPhotoAlbumService _photos;

    public MediaController(IAudioTrackService audio, IPhotoAlbumService photos)
    {
        _audio = audio;
        _photos = photos;
    }

    [NavigationLink("media")]
    public IActionResult Index()
    {
        return View(new MediaViewModel
        {
            Audio = _audio.GetTracksForPlayer().ToList(),
            PhotoAlbums = _photos.GetPhotoAlbums().ToList()
        });
    }
}
