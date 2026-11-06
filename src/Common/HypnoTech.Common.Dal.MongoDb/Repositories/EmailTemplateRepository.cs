using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class EmailTemplateRepository : BaseMongoRepository, IEmailTemplateRepository
    {
        public EmailTemplateRepository(IOptions<MongoOptions> config) : base(config, "emailtemplates")
        {
        }

        public EmailTemplate GetEmailTemplate(string type)
        {
            return GetOne<EmailTemplate>(new BsonDocument("type", type));
        }

        public Task<EmailTemplate> GetEmailTemplateAsync(string type, CancellationToken cancellationToken = default)
        {
            return GetOneAsync<EmailTemplate>(new BsonDocument("type", type), cancellationToken);
        }
    }
}
