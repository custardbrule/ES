using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RequestValidatior
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers all validators that inherit from BaseValidator&lt;T&gt; in the specified assembly.
        /// </summary>
        /// <param name="services">The service collection to add validators to.</param>
        /// <param name="assembly">The assembly to scan for validators.</param>
        /// <param name="lifetime">The service lifetime (default: Scoped).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// Each validator is registered twice:
        /// 1. As its concrete type (e.g., UserValidator)
        /// 2. As BaseValidator&lt;TSource&gt; (e.g., BaseValidator&lt;User&gt;)
        ///
        /// Lifetime recommendations:
        /// - Scoped: Best for web applications (one instance per request)
        /// - Singleton: Use if validators are stateless and thread-safe
        /// - Transient: Rarely needed, creates overhead
        /// </remarks>
        public static IServiceCollection AddValidators(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            // Find all classes that inherit from BaseValidator<T>
            var validatorTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(BaseValidator<>));

            foreach (var validatorType in validatorTypes)
            {
                var baseType = validatorType.BaseType!;
                var sourceType = baseType.GetGenericArguments()[0];
                var interfaceType = typeof(BaseValidator<>).MakeGenericType(sourceType);

                // Register concrete type (e.g., UserValidator)
                services.Add(new ServiceDescriptor(validatorType, validatorType, lifetime));

                // Register base type that forwards to concrete (e.g., BaseValidator<User> -> UserValidator)
                services.Add(new ServiceDescriptor(interfaceType, sp => sp.GetRequiredService(validatorType), lifetime));
            }

            return services;
        }

        /// <summary>
        /// Registers all validators from multiple assemblies with the specified lifetime.
        /// </summary>
        public static IServiceCollection AddValidators(this IServiceCollection services, ServiceLifetime lifetime, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
                services.AddValidators(assembly, lifetime);

            return services;
        }

        /// <summary>
        /// Registers all validators from multiple assemblies with Scoped lifetime (default).
        /// </summary>
        public static IServiceCollection AddValidators(this IServiceCollection services, params Assembly[] assemblies)
            => services.AddValidators(ServiceLifetime.Scoped, assemblies);
    }
}
