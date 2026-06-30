using System;
using System.Collections.Generic;
using HypnoTech.Cms;
using HypnoTech.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HypnoTech.Common.Extensions;

namespace HypnoTech.Media.Startup
{
    public static class StartupExtensions
    {
        public static void AddMediaAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterMediaModels(configuration);
            services.Configure<MediaOptions>(configuration.GetSection("media"));
        }

        private static void RegisterMediaModels(IConfiguration configuration)
        {
            if (configuration.GetValue("Media:AudioEnabled", false))
            {
                FormClassMapping.RegisterClassMapping<AudioTrack>(cm =>
                {
                    cm.SetDisplayName("Audio");
                    cm.SetControllerName("Audio");
                    cm.RequiresRole("audio");
                    cm.AddDummyDataSource("audiotracks");
                    cm.AddDataSource("audiotracks");
                    cm.MapHiddenInputMember(mm => mm.Id);
                    cm.MapTextInputMember(mm => mm.Title).IncludeInTable(0).Searchable().Required();
                    cm.MapFileUploadInputMember(mm => mm.Path).SetTargetDirectory("AudioTracks").WithDisplayText("Mp3").Required();
                    cm.SetIcon("fa fa-music");
                });
            }

            if (configuration.GetValue("Media:PhotoAlbumsEnabled", false))
            {
                FormClassMapping.RegisterClassMapping<PhotoAlbum>(cm =>
                {
                    cm.SetDisplayName("Photo Albums");
                    cm.SetControllerName("PhotoAlbum");
                    cm.RequiresRole("photos");
                    cm.AddDummyDataSource("photoalbums");
                    cm.AddDataSource("photoalbums");
                    cm.MapHiddenInputMember(mm => mm.Id);
                    cm.MapTextInputMember(mm => mm.Title).IncludeInTable(0).Searchable().Required();
                    cm.MapPhotoGalleryInputMember<Photo, List<Photo>>(mm => mm.Photos)
                        .MapMembers(p => p.Id, p => p.ImagePath, p => p.Caption)
                        .SetImageSize(2000, 2000).Required(true);
                    cm.MapGeneratedMember(mm => mm.Url, album => album.Title.GenerateSlug());
                    cm.MapGeneratedMember(mm => mm.Date, album => DateTime.UtcNow);

                    cm.SetIcon("fa fa-camera");
                });
            }

            if (configuration.GetValue("Media:YouTubeVideosEnabled", false))
            {
                FormClassMapping.RegisterClassMapping<YouTubeVideo>(cm =>
                {
                    cm.SetDisplayName("Video");
                    cm.SetControllerName("Video");
                    cm.RequiresRole("videos");
                    cm.AddDummyDataSource("videos");
                    cm.AddDataSource("videos");
                    cm.MapHiddenInputMember(mm => mm.Id);
                    cm.MapTextInputMember(mm => mm.Title).IncludeInTable(0).Searchable().Required();
                    cm.MapYouTubeInputMember(mm => mm.YouTubeId).IncludeInTable(1).WithDisplayText("YouTube Url").Required();
                    cm.SetIcon("fa fa-video");
                });
            }
        }
    }
}
