using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Common
{
    public interface IInitializer
    {
        void Initialise(IServiceCollection services, IConfiguration configuration);
    }
}
