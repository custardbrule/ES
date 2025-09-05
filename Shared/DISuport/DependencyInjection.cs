using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace DISuport
{
    /* 
     * Only support simple DI, not handle cycle | dependency tree | validation | etc ...
     * so only use when you make sure all the above fullfill
     * sorry for the inconvinient but can't do
     * and for method invoke - need to create static method to use this
     */
    public static class DependencyInjection
    {
        public static void MethodRegisterAttributeServices(IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies) MethodRegisterAttributeServices(services, assembly);
        }
        public static void MethodRegisterAttributeServices(IServiceCollection services, Assembly assembly)
        {
            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.GetCustomAttribute<MethodRegisterServiceAttribute>() is not null);

            methods.ToList().ForEach(method =>
            {
                var attribute = method.GetCustomAttribute<MethodRegisterServiceAttribute>()!;

                if (method.ReturnType == typeof(IServiceCollection) && method.GetParameters().FirstOrDefault()?.ParameterType == typeof(IServiceCollection))
                {
                    method.Invoke(null, [services]);
                }
            });
        }

        public static IServiceCollection RegisterAttributeServices(IServiceCollection services, params Assembly[] assemblies)
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
                var attribute = type.GetCustomAttribute<RegisterServiceAttribute>(true)!;
                services.Add(attribute.GetServiceDescriptor());
            }

            return services;
        }
    }
}
