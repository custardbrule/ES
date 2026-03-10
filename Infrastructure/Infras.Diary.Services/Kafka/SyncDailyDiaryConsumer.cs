using Confluent.Kafka;
using Contracts.Kafka;
using Data;
using Domain.Diary.DiaryRoot;
using KurrentDB.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infras.Diary.Services.Kafka
{
    [RegisterKafkaConsumer]
    public class SyncDailyDiaryConsumer(
        IOptions<ConsumerConfig> config,
        KurrentDBClient kurrentDBClient,
        IElasticSearchContext elasticsearchContext,
        ExtractDiaryProducer extractProducer,
        ILogger<SyncDailyDiaryConsumer> logger)
        : KafkaConsumerBase<string, SyncMessage>(config, logger)
    {
        protected override string Topic => DiaryTopics.SyncDailyDiary;

        protected override async Task HandleAsync(string key, SyncMessage value, Headers _, CancellationToken cancellationToken)
        {
            var daily = await kurrentDBClient
                .ReadStreamAsync(Direction.Forwards, value.StreamKey, StreamPosition.Start, cancellationToken: cancellationToken)
                .AggregateAsync(DailyDiary.Init(), (acc, e) => acc.Apply(e.OriginalEvent.Data.ToArray().GetDataFromBytes(e.OriginalEvent.EventType)!), cancellationToken);

            var indexResult = await elasticsearchContext.IndexAsync(DailyDiaryConstants.ESIndex, DailyDiaryConstants.GetId(daily.Id), daily);
            logger.LogInformation("Synced daily diary {DailyDiaryId} from stream {StreamKey}", daily.Id, value.StreamKey);

            await extractProducer.GetProducer().ProduceAsync(
                DiaryTopics.ExtractDailyDiary,
                new Message<string, ExtractDailyDiaryMessage> { Key = key, Value = new ExtractDailyDiaryMessage(daily.Id, indexResult.Result.ToString()) },
                cancellationToken);
        }
    }
}
