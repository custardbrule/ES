using Microsoft.Extensions.DependencyInjection;

namespace Utilities
{
    /// <summary>
    /// use for register by class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterServiceAttribute : Attribute
    {
        public string? Key { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }

        public RegisterServiceAttribute(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "")
        {
            Key = key;
            Lifetime = serviceLifetime;
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public ServiceDescriptor GetServiceDescriptor()
        {
            return string.IsNullOrEmpty(Key) ? new ServiceDescriptor(ServiceType, ImplementationType, Lifetime) : new ServiceDescriptor(ServiceType, Key, ImplementationType, Lifetime);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterServiceAttribute<TService, TImplementation> : RegisterServiceAttribute
    {
        public RegisterServiceAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "") : base(typeof(TService), typeof(TImplementation), serviceLifetime, key)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterServiceAttribute<TService> : RegisterServiceAttribute<TService, TService>
    {
        public RegisterServiceAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "") : base(serviceLifetime, key)
        {
        }
    }
}
