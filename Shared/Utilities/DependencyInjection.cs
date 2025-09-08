using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Utilities
{
    /* 
     * Only support simple DI, not handle cycle | dependency tree | validation | etc ...
     * so only use when you make sure all the above fullfill
     * sorry for the inconvinient but can't do
     */
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterAttributeServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies) RegisterAttributeServices(services, assembly);
            return services;
        }
        public static IServiceCollection RegisterAttributeServices(this IServiceCollection services, Assembly assembly)
        {
            var typesWithAttributes = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<RegisterServiceAttribute>(true) is not null &&
                             !type.IsAbstract &&
                             !type.IsInterface)
                .ToList();

            foreach (var type in typesWithAttributes)
            {
                var attributes = type.GetCustomAttributes<RegisterServiceAttribute>(true);
                foreach (var attribute in attributes) services.Add(attribute.GetServiceDescriptor());
            }

            return services;
        }
    }
}
