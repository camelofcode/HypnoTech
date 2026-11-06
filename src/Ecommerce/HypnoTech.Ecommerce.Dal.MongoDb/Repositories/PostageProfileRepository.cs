using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class PostageProfileRepository : BaseMongoRepository, IPostageProfileRepository
    {
        private readonly IMerchRepository _merchRepository;

        public PostageProfileRepository(IOptions<MongoOptions> config, IMerchRepository merchRepository) : base(config, "postageprofiles")
        {
            _merchRepository = merchRepository;
        }

        public PostageProfile GetProfile(string name)
        {
            return GetOne<PostageProfile>(new BsonDocument("name", name));
        }

        public Task<PostageProfile> GetProfileAsync(string name, CancellationToken cancellationToken = default) =>
            GetOneAsync<PostageProfile>(new BsonDocument("name", name), cancellationToken);

        public List<PostageProfile> GetProfiles()
        {
            var profiles = GetAll<PostageProfile>();
            var merch = _merchRepository.GetMerchFlattened();

            foreach (var profile in profiles)
            {
                profile.CanDelete = merch.All(p => !string.Equals(p.PostageProfile, profile.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            return profiles.ToList();
        }

        public async Task<List<PostageProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
        {
            var profiles = await GetAllAsync<PostageProfile>(cancellationToken: cancellationToken);
            var merch = await _merchRepository.GetMerchFlattenedAsync(cancellationToken);

            foreach (var profile in profiles)
            {
                profile.CanDelete = merch.All(p => !string.Equals(
                    p.PostageProfile,
                    profile.Name,
                    StringComparison.InvariantCultureIgnoreCase));
            }

            return profiles.ToList();
        }

        public void AddProfile(PostageProfile profile)
        {
            profile.Id = ObjectId.GenerateNewId().ToString();
            Collection.InsertOne(profile.ToBsonDocument());
        }

        public async Task AddProfileAsync(PostageProfile profile, CancellationToken cancellationToken = default)
        {
            profile.Id = ObjectId.GenerateNewId().ToString();
            await Collection.InsertOneAsync(
                profile.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }

        public PostageProfile GetProfileForEdit(string id)
        {
            return GetByObjectId<PostageProfile>(id);
        }

        public Task<PostageProfile> GetProfileForEditAsync(string id, CancellationToken cancellationToken = default) =>
            GetByObjectIdAsync<PostageProfile>(id, cancellationToken);

        public void UpdateProfile(PostageProfile profile)
        {
            var existing = GetByObjectId<PostageProfile>(profile.Id);

            existing.UnitedKingdom = profile.UnitedKingdom;
            existing.EuropeanUnion = profile.EuropeanUnion;
            existing.RestOfWorld1 = profile.RestOfWorld1;
            existing.RestOfWorld2 = profile.RestOfWorld2;
            existing.RestOfWorld3 = profile.RestOfWorld3;

            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(existing.Id)), existing.ToBsonDocument());
        }

        public async Task UpdateProfileAsync(PostageProfile profile, CancellationToken cancellationToken = default)
        {
            var existing = await GetByObjectIdAsync<PostageProfile>(profile.Id, cancellationToken);

            existing.UnitedKingdom = profile.UnitedKingdom;
            existing.EuropeanUnion = profile.EuropeanUnion;
            existing.RestOfWorld1 = profile.RestOfWorld1;
            existing.RestOfWorld2 = profile.RestOfWorld2;
            existing.RestOfWorld3 = profile.RestOfWorld3;

            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(existing.Id)),
                existing.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }

        public string RemoveProfile(string id)
        {
            var profile = GetByObjectId<PostageProfile>(id);
            var merch = _merchRepository.GetMerchFlattened();

            if (merch.Any(p => string.Equals(p.PostageProfile, profile.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return $"'{profile.Name}' cannot be removed as it has merch associated. Move all merch to a different profile before removing.";
            }

            Collection.DeleteOne(new BsonDocument("_id", new ObjectId(id)));
            return null;
        }

        public async Task<string> RemoveProfileAsync(string id, CancellationToken cancellationToken = default)
        {
            var profile = await GetByObjectIdAsync<PostageProfile>(id, cancellationToken);
            var merch = await _merchRepository.GetMerchFlattenedAsync(cancellationToken);

            if (merch.Any(p => string.Equals(
                    p.PostageProfile,
                    profile.Name,
                    StringComparison.InvariantCultureIgnoreCase)))
            {
                return $"'{profile.Name}' cannot be removed as it has merch associated. Move all merch to a different profile before removing.";
            }

            await Collection.DeleteOneAsync(
                new BsonDocument("_id", new ObjectId(id)),
                cancellationToken);
            return null;
        }
    }
}
