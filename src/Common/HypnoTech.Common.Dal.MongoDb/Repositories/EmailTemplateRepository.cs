using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

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
    }
}
