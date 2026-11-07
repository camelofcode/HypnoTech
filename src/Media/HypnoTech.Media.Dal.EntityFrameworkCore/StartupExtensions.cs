using HypnoTech.Common.Models;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HypnoTech.Media.Dal.EntityFrameworkCore
{
    public static class StartupExtensions
    {
        public static IServiceCollection UseEntityFrameworkCoreForMedia(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration.GetValue("Media:AudioEnabled", false))
            {
                ModelRegistry.Register(typeof(AudioTrack), "audiotracks", "Audio Tracks");
            }

            if (configuration.GetValue("Media:YouTubeVideosEnabled", false))
            {
                ModelRegistry.Register(typeof(YouTubeVideo), "videos", "Videos");
            }

            if (configuration.GetValue("Media:PhotoAlbumsEnabled", false))
            {
                ModelRegistry.Register(typeof(PhotoAlbum), "photoalbums", "Photo Albums");
            }

            services.TryAddTransient<IAudioTrackRepository, AudioTrackRepository>();
            services.TryAddTransient<IVideoRepository, VideoRepository>();
            services.TryAddTransient<IPhotoAlbumRepository, PhotoAlbumRepository>();
            return services;
        }
    }
}
