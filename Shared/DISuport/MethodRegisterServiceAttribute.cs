using Microsoft.Extensions.DependencyInjection;

namespace DISuport
{
    /// <summary>
    /// use for register by class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterServiceAttribute : Attribute
    {
        public string? Key { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Type ServiceType { get; set; }
        public Type? ImplementationType { get; set; }
        public Func<IServiceProvider, object>? ImplementationFactory { get; set; }
        public Func<IServiceProvider, object?, object>? KeyImplementationFactory { get; set; }

        public RegisterServiceAttribute(Type serviceType, Func<IServiceProvider, object>? implementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            Lifetime = serviceLifetime;
            ServiceType = serviceType;
            ImplementationFactory = implementationFactory;
        }

        public RegisterServiceAttribute(Type serviceType, Func<IServiceProvider, object?, object>? keyImplementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "")
        {
            Lifetime = serviceLifetime;
            Key = key;
            ServiceType = serviceType;
            KeyImplementationFactory = keyImplementationFactory;
        }

        public RegisterServiceAttribute(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "")
        {
            Lifetime = serviceLifetime;
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public ServiceDescriptor GetServiceDescriptor()
        {
            if (KeyImplementationFactory is not null) return new ServiceDescriptor(ServiceType, Key, KeyImplementationFactory, Lifetime);
            if (ImplementationFactory is not null) return new ServiceDescriptor(ServiceType, ImplementationFactory, Lifetime);
            return string.IsNullOrEmpty(Key) ? new ServiceDescriptor(ServiceType, ImplementationType, Lifetime) : new ServiceDescriptor(ServiceType, Key, ImplementationType, Lifetime);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterServiceAttribute<TService, TImplementation> : RegisterServiceAttribute
    {
        public RegisterServiceAttribute(Func<IServiceProvider, object>? implementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) : base(typeof(TService), implementationFactory, serviceLifetime)
        {
        }

        public RegisterServiceAttribute(Func<IServiceProvider, object?, object>? keyImplementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "") : base(typeof(TService), keyImplementationFactory, serviceLifetime, key)
        {
        }

        public RegisterServiceAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "") : base(typeof(TService), typeof(TImplementation), serviceLifetime, key)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterServiceAttribute<TService> : RegisterServiceAttribute<TService, TService>
    {
        public RegisterServiceAttribute(Func<IServiceProvider, object>? implementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) : base(implementationFactory, serviceLifetime)
        {
        }

        public RegisterServiceAttribute(Func<IServiceProvider, object?, object>? keyImplementationFactory = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "") : base(keyImplementationFactory, serviceLifetime, key)
        {
        }

        public RegisterServiceAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, string key = "") : base(serviceLifetime, key)
        {
        }
    }


    /// <summary>
    /// use for register by method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodRegisterServiceAttribute : Attribute
    {
        public MethodRegisterServiceAttribute()
        {
        }
    }
}
