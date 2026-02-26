using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Util;
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

        private async Task EnsureTopicExistsAsync()
        {
            var adminConfig = new AdminClientConfig { BootstrapServers = _config.BootstrapServers };
            using var adminClient = new AdminClientBuilder(adminConfig).Build();
            try
            {
                await adminClient.CreateTopicsAsync(
                [
                    new TopicSpecification { Name = Topic }
                ]);
                _logger.LogInformation("Created Kafka topic: {Topic}", Topic);
            }
            catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                // Topic already exists, nothing to do
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            await EnsureTopicExistsAsync();

            using var consumer = new ConsumerBuilder<TKey, TValue>(_config)
                .SetValueDeserializer(new JsonDeserializer<TValue>())
                .Build();

            consumer.Subscribe(Topic);
            _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", Topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<TKey, TValue>? result = null;
                try
                {
                    result = consumer.Consume(stoppingToken);
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
                catch (Exception ex) when (result?.Message != null)
                {
                    _logger.LogError(ex, "Error handling Kafka message on topic {Topic}", Topic);
                    await OnMessageFailedAsync(result.Message.Key, result.Message.Value, ex, stoppingToken);
                    consumer.Commit(result);
                }
            }

            consumer.Close();
        }

        /// <summary>
        /// Called when HandleAsync throws. Override to send the message to a DLQ.
        /// Default behaviour: log and skip (offset is committed by the base class after this returns).
        /// </summary>
        protected virtual Task OnMessageFailedAsync(TKey key, TValue value, Exception exception, CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Produces the failed message to {Topic}.dlq as JSON.
        /// Call this from an OnMessageFailedAsync override.
        /// </summary>
        protected async Task SendToDlqAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var dlqTopic = $"{Topic}.dlq";
            var producerConfig = new ProducerConfig { BootstrapServers = _config.BootstrapServers };
            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            await producer.ProduceAsync(dlqTopic, new Message<string, string>
            {
                Key = JsonSerializer.Serialize(key),
                Value = JsonSerializer.Serialize(value)
            }, cancellationToken);
            _logger.LogWarning("Sent failed message to DLQ: {DlqTopic}", dlqTopic);
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
