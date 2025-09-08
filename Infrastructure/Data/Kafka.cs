using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Data
{
    /// <summary>
    /// declaration factory
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface IProducerFactory<TKey, TValue>
    {
        public IProducer<TKey, TValue> GetProducer();
    }

    /// <summary>
    /// the idea is register producer through attribute to reduce the work needed
    /// * and only follow this if you ONLY register as SINGLETON because this invoke reflection *
    /// If you have cleaner way please let me know.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterKafkaProducerAttribute : Attribute
    {
        /// <summary>
        /// recommend put key on these service
        /// </summary>
        public string Key { get; set; }
        public ServiceLifetime Lifetime { get; set; }
        public Type KeyType { get; set; }
        public Type ValueType { get; set; }
        public Type ImplementType { get; set; }

        private Type FactoryType => typeof(IProducerFactory<,>).MakeGenericType(KeyType, ValueType);
        private Type ProducerType => typeof(IProducer<,>).MakeGenericType(KeyType, ValueType);

        public RegisterKafkaProducerAttribute(string key, Type keyType, Type valueType, Type implement, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            Key = key;
            Lifetime = lifetime;
            KeyType = keyType;
            ValueType = valueType;
            ImplementType = implement;
        }

        public ServiceDescriptor GetServiceDescriptor()
        {
            return new ServiceDescriptor(FactoryType, Key, ImplementType, Lifetime);
        }

        public ServiceDescriptor GetProducerServiceDescriptor()
        {
            return ServiceDescriptor.DescribeKeyed(ProducerType, Key, (provider, key) =>
            {
                var factory = provider.GetRequiredKeyedService(FactoryType, key);
                var createMethod = FactoryType.GetMethod("GetProducer", Type.EmptyTypes);
                return createMethod.Invoke(factory, null);
            }, Lifetime);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterKafkaProducerAttribute<TKey, TValue, TImplement> : RegisterKafkaProducerAttribute
    {
        public RegisterKafkaProducerAttribute(string key, ServiceLifetime lifetime = ServiceLifetime.Singleton) : base(key, typeof(TKey), typeof(TValue), typeof(TImplement), lifetime)
        {
        }
    }

    public class SampleMessage { }

    [RegisterKafkaProducer<string, SampleMessage, SampleProducer>(nameof(SampleProducer))]
    public class SampleProducer : IProducerFactory<string, SampleMessage>
    {
        private readonly ProducerConfig _config;
        private readonly IProducer<string, SampleMessage> _producer;
        public SampleProducer(IOptions<ProducerConfig> config)
        {
            _config = config.Value;

            /**
             * Add more config before build if need to
             */
            _producer = new ProducerBuilder<string, SampleMessage>(_config).Build();
        }

        public IProducer<string, SampleMessage> GetProducer() => _producer;
    }
}
