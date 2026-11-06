using System.Collections.Generic;
using System.Linq;
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

        public EmailTemplate GetTemplateForEdit(string id)
        {
            return GetByObjectId<EmailTemplate>(id);
        }

        public void UpdateTemplate(EmailTemplate profile)
        {
            Collection.ReplaceOne(new BsonDocument("_id", new ObjectId(profile.Id)), profile.ToBsonDocument());
        }
    }
}
