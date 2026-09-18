using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HypnoTech.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPublicImplementedInterfaces(
            this IServiceCollection services,
            Assembly assembly,
            Func<Type, bool> predicate)
        {
            var implementationTypes = assembly.ExportedTypes
                .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters)
                .Where(predicate);

            foreach (var implementationType in implementationTypes)
            {
                foreach (var serviceType in implementationType.GetInterfaces().Where(type => type.IsPublic))
                {
                    services.AddTransient(serviceType, implementationType);
                }
            }

            return services;
        }
    }
}
