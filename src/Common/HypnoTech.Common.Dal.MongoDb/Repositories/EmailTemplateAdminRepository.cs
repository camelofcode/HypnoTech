using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace HypnoTech.Common.Dal.MongoDb.Repositories
{
    public class EmailTemplateAdminRepository : BaseMongoRepository, IEmailTemplateAdminRepository
    {
        public EmailTemplateAdminRepository(IOptions<MongoOptions> config) : base(config, "emailtemplates")
        {
        }

        public List<EmailTemplate> GetTemplates()
        {
            return GetAll<EmailTemplate>().ToList();
        }

        public async Task<List<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
        {
            return (await GetAllAsync<EmailTemplate>(cancellationToken: cancellationToken)).ToList();
        }

        public EmailTemplate GetTemplateForEdit(string id)
        {
            return GetByObjectId<EmailTemplate>(id);
        }

        public Task<EmailTemplate> GetTemplateForEditAsync(string id, CancellationToken cancellationToken = default)
        {
            return GetByObjectIdAsync<EmailTemplate>(id, cancellationToken);
        }

        public void UpdateTemplate(EmailTemplate profile)
        {
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(profile.Id)), profile.ToBsonDocument());
        }

        public async Task UpdateTemplateAsync(EmailTemplate profile, CancellationToken cancellationToken = default)
        {
            await Collection.ReplaceOneAsync(
                new BsonDocument("_id", new ObjectId(profile.Id)),
                profile.ToBsonDocument(),
                cancellationToken: cancellationToken);
        }
    }
}
