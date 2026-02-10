using Confluent.Kafka;
using Data;
using Microsoft.Extensions.Options;

namespace Infras.Diary.Services.Kafka
{
    [RegisterKafkaProducer<string, SyncMessage, DiaryProducer>(nameof(DiaryProducer))]
    public class DiaryProducer : IProducerFactory<string, SyncMessage>
    {
        private readonly IProducer<string, SyncMessage> _producer;

        public DiaryProducer(IOptions<ProducerConfig> config)
        {
            _producer = new ProducerBuilder<string, SyncMessage>(config.Value)
                .SetValueSerializer(new JsonSerializer<SyncMessage>())
                .Build();
        }

        public IProducer<string, SyncMessage> GetProducer() => _producer;
    }
}
