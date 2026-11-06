using System.Collections.Generic;
using HypnoTech.Media.Dal.Repositories;
using HypnoTech.Media.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Media.Dal.MongoDb.Repositories
{
    public class AudioTrackRepository : BaseMongoRepository, IAudioTrackRepository
    {
        public AudioTrackRepository(IOptions<MongoOptions> config) : base(config, "audiotracks")
        {
        }

        public ICollection<AudioTrack> GetTracksForPlayer()
        {
            return GetAll<AudioTrack>();
        }

        public Task<ICollection<AudioTrack>> GetTracksForPlayerAsync(CancellationToken cancellationToken = default)
        {
            return GetAllAsync<AudioTrack>(cancellationToken: cancellationToken);
        }
    }
}
