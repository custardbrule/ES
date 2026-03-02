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
        private IProducer<string, string>? _dlqProducer;

        protected KafkaConsumerBase(IOptions<ConsumerConfig> config, ILogger logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        protected string BootstrapServers => _config.BootstrapServers;
        protected ILogger Logger => _logger;

        private IProducer<string, string> GetDlqProducer()
            => _dlqProducer ??= new ProducerBuilder<string, string>(
                new ProducerConfig { BootstrapServers = _config.BootstrapServers }).Build();

        public override void Dispose()
        {
            _dlqProducer?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        protected abstract string Topic { get; }
        protected abstract Task HandleAsync(TKey key, TValue value, Headers headers, CancellationToken cancellationToken);

        /// <summary>
        /// Set to false for consumers that subscribe via regex — topic auto-creation is skipped.
        /// </summary>
        protected virtual bool EnsureTopicOnStart => true;

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

            if (EnsureTopicOnStart) await EnsureTopicExistsAsync();

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

                    await HandleAsync(result.Message.Key, result.Message.Value, result.Message.Headers, stoppingToken);
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
                    var retryHeader = result.Message.Headers.FirstOrDefault(h => h.Key == "x-dead-letter-retry-count");
                    var retryCount = retryHeader != null && int.TryParse(Encoding.UTF8.GetString(retryHeader.GetValueBytes()), out var parsed) ? parsed : 0;
                    await OnMessageFailedAsync(result.Message.Key, result.Message.Value, ex, retryCount, stoppingToken);
                    consumer.Commit(result);
                }
            }

            consumer.Close();
        }

        /// <summary>
        /// Called when HandleAsync throws. Override to send the message to a DLQ.
        /// Default behaviour: log and skip (offset is committed by the base class after this returns).
        /// </summary>
        protected virtual Task OnMessageFailedAsync(TKey key, TValue value, Exception exception, int retryCount, CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Produces the failed message to {Topic}.dlq with error metadata in headers.
        /// Pass retryCount from OnMessageFailedAsync so it increments across attempts.
        /// Call this from an OnMessageFailedAsync override.
        /// </summary>
        protected async Task SendToDlqAsync(TKey key, TValue value, Exception exception, int retryCount, CancellationToken cancellationToken)
        {
            var dlqTopic = $"{Topic}.dlq";
            var headers = new Headers
            {
                { "x-dead-letter-original-topic", Encoding.UTF8.GetBytes(Topic) },
                { "x-dead-letter-reason", Encoding.UTF8.GetBytes(exception.Message) },
                { "x-dead-letter-exception", Encoding.UTF8.GetBytes(exception.GetType().FullName ?? exception.GetType().Name) },
                { "x-dead-letter-timestamp", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")) },
                { "x-dead-letter-retry-count", Encoding.UTF8.GetBytes((retryCount + 1).ToString()) }
            };

            await GetDlqProducer().ProduceAsync(dlqTopic, new Message<string, string>
            {
                Key = JsonSerializer.Serialize(key),
                Value = JsonSerializer.Serialize(value),
                Headers = headers
            }, cancellationToken);
            _logger.LogWarning("Sent failed message to DLQ: {DlqTopic}", dlqTopic);
        }
    }

    /// <summary>
    /// Configure source topics whose DLQ messages the GenericDlqConsumer should skip.
    /// Add entries when a topic has its own dedicated DLQ consumer with custom logic.
    /// </summary>
    public class GenericDlqOptions
    {
        public HashSet<string> IgnoredSourceTopics { get; set; } = [];
    }

    /// <summary>
    /// Single consumer that handles ALL *.dlq topics via regex subscription.
    /// Reads x-dead-letter-original-topic from headers, applies exponential backoff, then requeues.
    /// Register once — no per-topic subclass needed.
    /// To opt a topic out, add its name to GenericDlqOptions.IgnoredSourceTopics.
    /// </summary>
    [RegisterKafkaConsumer]
    public class GenericDlqConsumer(
        IOptions<ConsumerConfig> config,
        ILogger<GenericDlqConsumer> logger,
        IOptions<GenericDlqOptions> options)
        : KafkaConsumerBase<string, string>(config, logger)
    {
        private const int MaxRetries = 3;
        private static readonly TimeSpan[] BackoffDelays =
        [
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(120)
        ];

        private readonly HashSet<string> _ignoredSourceTopics = options.Value.IgnoredSourceTopics;
        private IProducer<string, string>? _requeueProducer;

        protected override string Topic => "^.*\\.dlq$";
        protected override bool EnsureTopicOnStart => false;

        private IProducer<string, string> GetRequeueProducer()
            => _requeueProducer ??= new ProducerBuilder<string, string>(
                new ProducerConfig { BootstrapServers = BootstrapServers }).Build();

        protected override async Task HandleAsync(string key, string value, Headers headers, CancellationToken cancellationToken)
        {
            var sourceTopicHeader = headers.FirstOrDefault(h => h.Key == "x-dead-letter-original-topic");
            if (sourceTopicHeader == null)
            {
                Logger.LogWarning("DLQ message missing x-dead-letter-original-topic header, skipping. Key={Key}", key);
                return;
            }

            var sourceTopic = Encoding.UTF8.GetString(sourceTopicHeader.GetValueBytes());

            if (_ignoredSourceTopics.Contains(sourceTopic))
            {
                Logger.LogDebug("Skipping DLQ message for opted-out topic {Topic}", sourceTopic);
                return;
            }

            var retryHeader = headers.FirstOrDefault(h => h.Key == "x-dead-letter-retry-count");
            var retryCount = retryHeader != null && int.TryParse(Encoding.UTF8.GetString(retryHeader.GetValueBytes()), out var parsed)
                ? parsed
                : 1;

            if (retryCount >= MaxRetries)
            {
                Logger.LogError("Message permanently dead after {MaxRetries} retries on topic {Topic}. Key={Key}",
                    MaxRetries, sourceTopic, key);
                return;
            }

            var delay = BackoffDelays[Math.Min(retryCount - 1, BackoffDelays.Length - 1)];
            Logger.LogWarning("Requeuing to {Topic} (attempt {Count}/{Max}) after {Delay}s delay",
                sourceTopic, retryCount, MaxRetries, delay.TotalSeconds);

            await Task.Delay(delay, cancellationToken);

            await GetRequeueProducer().ProduceAsync(sourceTopic, new Message<string, string>
            {
                Key = key,
                Value = value,
                Headers = new Headers
                {
                    { "x-dead-letter-retry-count", Encoding.UTF8.GetBytes(retryCount.ToString()) }
                }
            }, cancellationToken);
        }

        public override void Dispose()
        {
            _requeueProducer?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
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
