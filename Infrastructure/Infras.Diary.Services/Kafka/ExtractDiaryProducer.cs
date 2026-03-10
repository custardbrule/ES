using Confluent.Kafka;
using Contracts.Kafka;
using Data;
using Microsoft.Extensions.Options;

namespace Infras.Diary.Services.Kafka
{
    [RegisterKafkaProducer<string, ExtractDailyDiaryMessage, ExtractDiaryProducer>(nameof(ExtractDiaryProducer))]
    public class ExtractDiaryProducer : IProducerFactory<string, ExtractDailyDiaryMessage>
    {
        private readonly IProducer<string, ExtractDailyDiaryMessage> _producer;

        public ExtractDiaryProducer(IOptions<ProducerConfig> config)
        {
            _producer = new ProducerBuilder<string, ExtractDailyDiaryMessage>(config.Value)
                .SetValueSerializer(new JsonSerializer<ExtractDailyDiaryMessage>())
                .Build();
        }

        public IProducer<string, ExtractDailyDiaryMessage> GetProducer() => _producer;
    }
}
