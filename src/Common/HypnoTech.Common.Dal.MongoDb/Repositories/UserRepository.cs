using System.Collections.Generic;
using System.Linq;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class UserRepository : BaseMongoRepository, IUserRepository
    {
        public UserRepository(IOptions<MongoOptions> config) : base(config, "adminusers")
        {
        }

        public AdminUser GetUserForLogin(string username)
        {
            return GetOne<AdminUser>(new BsonDocument("username", new BsonRegularExpression($"^{username}$", "i")));
        }

        public List<string> GetUserRoles(string username)
        {
            return GetOne<AdminUser>(new BsonDocument("username", new BsonRegularExpression($"^{username}$", "i"))).Roles;
        }

        public List<AdminUser> GetAllUsers()
        {
            return GetAll<AdminUser>().ToList();
        }

        public AdminUser GetUserForEdit(string id)
        {
            return GetByObjectId<AdminUser>(id);
        }

        public string AddUser(AdminUser user)
        {
            var existing = Collection.Find(new BsonDocument("username", new BsonRegularExpression($"^{user.Username}$", "i"))).FirstOrDefault();

            if (existing != null)
            {
                return "Username is already in use";
            }

            existing = Collection.Find(new BsonDocument("email", new BsonRegularExpression($"^{user.Email}$", "i"))).FirstOrDefault();

            if (existing != null)
            {
                return "Email is already in use";
            }

            user.Id = ObjectId.GenerateNewId().ToString();
            Collection.InsertOne(user.ToBsonDocument());

            return null;
        }

        public string UpdateUser(AdminUser user)
        {
            var existing = GetUserForEdit(user.Id);
            var duplicate = Collection.Find(new BsonDocument("username", new BsonRegularExpression($"^{user.Username}$", "i"))).FirstOrDefault();
            
            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Username is already in use";
            }

            duplicate = Collection.Find(new BsonDocument("email", new BsonRegularExpression($"^{user.Email}$", "i"))).FirstOrDefault();

            if (duplicate != null && BsonSerializer.Deserialize<AdminUser>(duplicate).Id != user.Id)
            {
                return "Email is already in use";
            }

            user.Password = existing.Password;
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(user.Id)), user.ToBsonDocument());

            return null;
        }

        public AdminUser ChangePassword(string id, string password)
        {
            var user = GetUserForEdit(id);
            user.Password = password;
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(user.Id)), user.ToBsonDocument());
            return user;
        }

        public AdminUser ChangePasswordByEmail(string email, string password)
        {
            var user = GetOne<AdminUser>(new BsonDocument("email", new BsonRegularExpression($"^{email}$", "i")));
            user.Password = password;
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(user.Id)), user.ToBsonDocument());
            return user;
        }

        public void RemoveUser(string id)
        {
            Collection.DeleteOne(new BsonDocument("_id", new ObjectId(id)));
        }

        public AdminUser GetUserByUsernameOrEmail(string nameOrEmail)
        {
            var user = GetOne<AdminUser>(new BsonDocument("username", new BsonRegularExpression($"^{nameOrEmail}$", "i")));
            return user ?? GetOne<AdminUser>(new BsonDocument("email", new BsonRegularExpression($"^{nameOrEmail}$", "i")));
        }
    }
}