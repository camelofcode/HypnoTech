using System.Collections.Generic;
using System.Linq;
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
    public class UserRepository : BaseMongoRepository, IUserRepository
    {
        public UserRepository(IOptions<MongoOptions> config) : base(config, "adminusers")
        {
        }

        public Task<AdminUser> GetUserForLoginAsync(string username, CancellationToken cancellationToken = default) =>
            GetOneAsync<AdminUser>(
                new BsonDocument("username", new BsonRegularExpression($"^{username}$", "i")),
                cancellationToken);

        public async Task<List<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default) =>
            (await GetUserForLoginAsync(username, cancellationToken)).Roles;

        public async Task<List<AdminUser>> GetAllUsersAsync(CancellationToken cancellationToken = default) =>
            (await GetAllAsync<AdminUser>(cancellationToken: cancellationToken)).ToList();

        public Task<AdminUser> GetUserForEditAsync(string id, CancellationToken cancellationToken = default) =>
            GetByObjectIdAsync<AdminUser>(id, cancellationToken);

        public async Task<string> AddUserAsync(AdminUser user, CancellationToken cancellationToken = default)
        {
            var existing = await Collection
                .Find(new BsonDocument("username", new BsonRegularExpression($"^{user.Username}$", "i")))
                .FirstOrDefaultAsync(cancellationToken);

            if (existing != null)
            {
                return "Username is already in use";
            }

            existing = await Collection
                .Find(new BsonDocument("email", new BsonRegularExpression($"^{user.Email}$", "i")))
                .FirstOrDefaultAsync(cancellationToken);

            if (existing != null)
            {
                return "Email is already in use";
            }

            user.Id = ObjectId.GenerateNewId().ToString();
            await Collection.InsertOneAsync(
                user.ToBsonDocument(),
                cancellationToken: cancellationToken);

            return null;
        }

        public async Task<string> UpdateUserAsync(AdminUser user, CancellationToken cancellationToken = default)
        {
            var existing = await GetUserForEditAsync(user.Id, cancellationToken);
            var duplicate = await Collection
                .Find(new BsonDocument("username", new BsonRegularExpression($"^{user.Username}$", "i")))
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Username is already in use";
            }

            duplicate = await Collection
                .Find(new BsonDocument("email", new BsonRegularExpression($"^{user.Email}$", "i")))
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Email is already in use";
            }

            user.Password = existing.Password;
            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(user.Id)),
                user.ToBsonDocument(),
                cancellationToken: cancellationToken);

            return null;
        }

        public async Task<AdminUser> ChangePasswordAsync(string id, string password, CancellationToken cancellationToken = default)
        {
            var user = await GetUserForEditAsync(id, cancellationToken);
            user.Password = password;
            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(user.Id)),
                user.ToBsonDocument(),
                cancellationToken: cancellationToken);
            return user;
        }

        public async Task<AdminUser> ChangePasswordByEmailAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await GetOneAsync<AdminUser>(
                new BsonDocument("email", new BsonRegularExpression($"^{email}$", "i")),
                cancellationToken);
            user.Password = password;
            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(user.Id)),
                user.ToBsonDocument(),
                cancellationToken: cancellationToken);
            return user;
        }

        public async Task RemoveUserAsync(string id, CancellationToken cancellationToken = default)
        {
            await Collection.DeleteOneAsync(
                new BsonDocument("_id", new ObjectId(id)),
                cancellationToken);
        }

        public async Task<AdminUser> GetUserByUsernameOrEmailAsync(string nameOrEmail, CancellationToken cancellationToken = default)
        {
            var user = await GetOneAsync<AdminUser>(
                new BsonDocument("username", new BsonRegularExpression($"^{nameOrEmail}$", "i")),
                cancellationToken);
            return user ?? await GetOneAsync<AdminUser>(
                new BsonDocument("email", new BsonRegularExpression($"^{nameOrEmail}$", "i")),
                cancellationToken);
        }
    }
}
