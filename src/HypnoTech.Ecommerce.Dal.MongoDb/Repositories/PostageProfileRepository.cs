using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Ecommerce.Dal.MongoDb.Repositories
{
    public class PostageProfileRepository : BaseMongoRepository, IPostageProfileRepository
    {
        private readonly List<MerchItem> _merch;

        public PostageProfileRepository(IOptions<MongoOptions> config, IMerchRepository merchRepository) : base(config, "postageprofiles")
        {
            _merch = merchRepository.GetMerchFlattened();
        }

        public PostageProfile GetProfile(string name)
        {
            return GetOne<PostageProfile>(new BsonDocument("name", name));
        }

        public List<PostageProfile> GetProfiles()
        {
            var profiles = GetAll<PostageProfile>();

            foreach (var profile in profiles)
            {
                profile.CanDelete = _merch.All(p => !string.Equals(p.PostageProfile, profile.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            return profiles.ToList();
        }

        public void AddProfile(PostageProfile profile)
        {
            profile.Id = ObjectId.GenerateNewId().ToString();
            Collection.InsertOne(profile.ToBsonDocument());
        }

        public PostageProfile GetProfileForEdit(string id)
        {
            return GetByObjectId<PostageProfile>(id);
        }

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

        public string RemoveProfile(string id)
        {
            var profile = GetByObjectId<PostageProfile>(id);

            if (_merch.Any(p => string.Equals(p.PostageProfile, profile.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return $"'{profile.Name}' cannot be removed as it has merch associated. Move all merch to a different profile before removing.";
            }

            Collection.DeleteOne(new BsonDocument("_id", new ObjectId(id)));
            return null;
        }
    }
}
