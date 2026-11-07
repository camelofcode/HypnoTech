using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class PasswordResetRepository : BaseMongoRepository, IPasswordResetRepository
    {
        public PasswordResetRepository(IOptions<MongoOptions> config) : base(config, "passwordresets")
        {
        }

        public async Task<string> CreateResetPasswordCodeAsync(AdminUser user, CancellationToken cancellationToken = default)
        {
            var reset = new PasswordReset(user);
            await Collection.InsertOneAsync(
                reset.ToBsonDocument(),
                cancellationToken: cancellationToken);
            return reset.Code;
        }

        public Task<PasswordReset> GetCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<PasswordReset>(new BsonDocument("code", code), cancellationToken);
        }

        public async Task RemoveCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            await Collection.DeleteOneAsync(new BsonDocument("code", code), cancellationToken);
        }
    }
}
