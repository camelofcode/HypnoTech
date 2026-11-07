using HypnoTech.Media.Models;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Media.Dal.EntityFrameworkCore
{
    public static class MediaModelBuilderExtensions
    {
        public static ModelBuilder AddHypnoTechMediaModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AudioTrack>(entity =>
            {
                entity.ToTable("audiotracks");
                StringId(entity);
                entity.Property<long>("PersistenceOrder").ValueGeneratedNever();
                entity.Ignore(track => track.RelatedName);
                entity.Ignore(track => track.ImagePath);
                entity.Ignore(track => track.Url);
                entity.Ignore(track => track.Date);
            });
            modelBuilder.Entity<YouTubeVideo>(entity =>
            {
                entity.ToTable("videos");
                StringId(entity);
                entity.Property<long>("PersistenceOrder").ValueGeneratedNever();
                entity.Ignore(video => video.ThumbnailUrl);
                entity.Ignore(video => video.VideoUrl);
            });
            modelBuilder.Entity<PhotoAlbum>(entity =>
            {
                entity.ToTable("photoalbums");
                StringId(entity);
                entity.Ignore(album => album.CoverPhoto);
                var photos = entity.OwnsMany(album => album.Photos);
                photos.ToTable("photoalbums_Photos");
                photos.Property(photo => photo.Id).ValueGeneratedNever();
                photos.HasKey("PhotoAlbumId", nameof(Photo.Id));
            });
            return modelBuilder;
        }

        private static void StringId<TEntity>(
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity)
            where TEntity : class
        {
            entity.HasKey("Id");
            entity.Property<string>("Id").ValueGeneratedNever();
        }
    }
}
