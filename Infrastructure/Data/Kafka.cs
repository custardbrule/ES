using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
                var createMethod = FactoryType.GetMethod("GetProducer", Type.EmptyTypes)!;
                return createMethod.Invoke(factory, null)!;
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

    /// <summary>
    /// Base class for Kafka consumers. Each consumer is a self-contained BackgroundService.
    /// Subclass this, provide Topic and HandleAsync, then register with [RegisterKafkaConsumer].
    /// </summary>
    public abstract class KafkaConsumerBase<TKey, TValue> : BackgroundService
    {
        private readonly ConsumerConfig _config;
        private readonly ILogger _logger;

        protected KafkaConsumerBase(IOptions<ConsumerConfig> config, ILogger logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        protected abstract string Topic { get; }
        protected abstract Task HandleAsync(TKey key, TValue value, CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            using var consumer = new ConsumerBuilder<TKey, TValue>(_config)
                .SetValueDeserializer(new JsonDeserializer<TValue>())
                .Build();

            consumer.Subscribe(Topic);
            _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message == null) continue;

                    await HandleAsync(result.Message.Key, result.Message.Value, stoppingToken);
                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error on topic {Topic}", Topic);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling Kafka message on topic {Topic}", Topic);
                }
            }

            consumer.Close();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterKafkaConsumerAttribute(int instances = 1) : Attribute
    {
        public int Instances { get; } = instances;
    }

    public class JsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull) return default!;
            return JsonSerializer.Deserialize<T>(data)!;
        }
    }

    public class JsonSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
