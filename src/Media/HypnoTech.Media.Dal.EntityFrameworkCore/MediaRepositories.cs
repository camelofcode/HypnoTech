using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.EntityFrameworkCore;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Media.Dal.EntityFrameworkCore
{
    public sealed class AudioTrackRepository : IAudioTrackRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public AudioTrackRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<ICollection<AudioTrack>> GetTracksForPlayerAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<AudioTrack>().AsNoTracking()
                .OrderBy(track => EF.Property<long>(track, "PersistenceOrder"))
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class VideoRepository : IVideoRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public VideoRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<ICollection<YouTubeVideo>> GetVideosAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<YouTubeVideo>().AsNoTracking()
                .OrderBy(video => EF.Property<long>(video, "PersistenceOrder"))
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class PhotoAlbumRepository : IPhotoAlbumRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        public PhotoAlbumRepository(IHypnoTechDbContextFactory factory) => _factory = factory;

        public async Task<ICollection<PhotoAlbum>> GetPhotoAlbumsAsync(
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<PhotoAlbum>().AsNoTracking()
                .Include(album => album.Photos)
                .OrderByDescending(album => album.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<PhotoAlbum> GetPhotoAlbumAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<PhotoAlbum>().AsNoTracking()
                .Include(album => album.Photos)
                .SingleOrDefaultAsync(album => album.Url == url, cancellationToken);
        }
    }
}
