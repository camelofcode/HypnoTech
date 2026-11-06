using System.Linq;
using HypnoTech.Demo.Website.ViewModels;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using HypnoTech.Media.Frontend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HypnoTech.Demo.Website.Controllers;

public sealed class MediaController : SpaController
{
    private readonly IAudioTrackService _audio;
    private readonly IPhotoAlbumService _photos;
    private readonly IVideoService _videos;

    public MediaController(IAudioTrackService audio, IPhotoAlbumService photos, IVideoService videos)
    {
        _audio = audio;
        _photos = photos;
        _videos = videos;
    }

    [NavigationLink("media")]
    public async Task<IActionResult> Index()
    {
        return AjaxPartialView(new MediaViewModel
        {
            Audio = (await _audio.GetTracksForPlayerAsync(HttpContext.RequestAborted)).ToList(),
            PhotoAlbums = (await _photos.GetPhotoAlbumsAsync(HttpContext.RequestAborted)).ToList(),
            Videos = (await _videos.GetVideosAsync(HttpContext.RequestAborted)).ToList()
        });
    }
}
