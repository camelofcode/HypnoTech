using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class AccountRepository : BaseMongoRepository, IAccountRepository
    {
        public AccountRepository(IOptions<MongoOptions> config) : base(config, "adminusers")
        {
        }

        public AdminUser GetUserByName(string name)
        {
            return GetOne<AdminUser>(new BsonDocument("username" , new BsonRegularExpression($"^{name}$", "i")));
        }

        public string UpdateAccount(AdminUser user)
        {
            var existing = GetByObjectId<AdminUser>(user.Id);
            var duplicate = Collection.Find(new BsonDocument("username", new BsonRegularExpression($"^{user.Username}$", "i"))).FirstOrDefault();

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Username is already in use";
            }

            duplicate = Collection.Find(new BsonDocument("email", user.Email)).FirstOrDefault();

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Email is already in use";
            }

            user.Password = existing.Password;
            user.Roles = existing.Roles;
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(user.Id)), user.ToBsonDocument());

            return null;
        }

        public void ChangePassword(string id, string password)
        {
            var user = GetByObjectId<AdminUser>(id);
            user.Password = password;
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(user.Id)), user.ToBsonDocument());
        }
    }
}
