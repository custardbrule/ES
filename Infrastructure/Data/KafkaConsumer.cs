using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Data
{
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
