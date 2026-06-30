using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class PasswordResetRepository : BaseMongoRepository, IPasswordResetRepository
    {
        public PasswordResetRepository(IOptions<MongoOptions> config) : base(config, "passwordresets")
        {
        }

        public string CreateResetPasswordCode(AdminUser user)
        {
            var reset = new PasswordReset(user);

            Collection.InsertOne(reset.ToBsonDocument());

            return reset.Code;
        }

        public PasswordReset GetCode(string code)
        {
            return GetOne<PasswordReset>(new BsonDocument("code", code));
        }

        public void RemoveCode(string code)
        {
            Collection.DeleteOne(new BsonDocument("code", code));
        }
    }
}
