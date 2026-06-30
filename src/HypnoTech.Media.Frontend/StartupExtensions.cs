using HypnoTech.Media.Frontend.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Media.Frontend
{
    public static class StartupExtensions
    {
        //Don't really like this interface for passing in a custom AudioTrackEnrichmentService, should look how .NET Core libraries do it.

        public static void AddMediaFrontend(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediaFrontend<DefaultAudioTrackEnrichmentService>(configuration);
        }

        public static void AddMediaFrontend<TAudioEnrichmentService>(this IServiceCollection services, IConfiguration configuration) where TAudioEnrichmentService : class, IAudioTrackEnrichmentService
        {
            services.AddTransient<IAudioTrackEnrichmentService, TAudioEnrichmentService>();
            services.AddTransient<IAudioTrackService, AudioTrackService>();
            services.AddTransient<IPhotoAlbumService, PhotoAlbumService>();
            services.AddTransient<IVideoService, VideoService>();
        }
    }
}
