using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class AccountRepository : BaseMongoRepository, IAccountRepository
    {
        public AccountRepository(IOptions<MongoOptions> config) : base(config, "adminusers")
        {
        }

        public Task<AdminUser> GetUserByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<AdminUser>(
                new BsonDocument("username", new BsonRegularExpression($"^{name}$", "i")),
                cancellationToken);
        }

        public async Task<string> UpdateAccountAsync(AdminUser user, CancellationToken cancellationToken = default)
        {
            var existing = await GetByObjectIdAsync<AdminUser>(user.Id, cancellationToken);
            var duplicate = await Collection
                .Find(new BsonDocument("username", new BsonRegularExpression($"^{user.Username}$", "i")))
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Username is already in use";
            }

            duplicate = await Collection.Find(new BsonDocument("email", user.Email))
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Email is already in use";
            }

            user.Password = existing.Password;
            user.Roles = existing.Roles;
            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(user.Id)),
                user.ToBsonDocument(),
                cancellationToken: cancellationToken);

            return null;
        }

        public async Task ChangePasswordAsync(string id, string password, CancellationToken cancellationToken = default)
        {
            var user = await GetByObjectIdAsync<AdminUser>(id, cancellationToken);
            user.Password = password;
            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(user.Id)),
                user.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }
    }
}
