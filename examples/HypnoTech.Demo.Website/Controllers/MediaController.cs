using System.Linq;
using System.Threading.Tasks;
using HypnoTech.Demo.Website.ViewModels;
using HypnoTech.Frontend.Attributes;
using HypnoTech.Frontend.Controllers;
using HypnoTech.Media.Frontend.Services;
using Microsoft.AspNetCore.Mvc;

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
        var cancellationToken = HttpContext.RequestAborted;
        var audioTask = _audio.GetTracksForPlayerAsync(cancellationToken);
        var photoAlbumsTask = _photos.GetPhotoAlbumsAsync(cancellationToken);
        var videosTask = _videos.GetVideosAsync(cancellationToken);

        await Task.WhenAll(audioTask, photoAlbumsTask, videosTask);

        var audio = await audioTask;
        var photoAlbums = await photoAlbumsTask;
        var videos = await videosTask;

        return AjaxPartialView(new MediaViewModel
        {
            Audio = audio.ToList(),
            PhotoAlbums = photoAlbums.ToList(),
            Videos = videos.ToList()
        });
    }
}
