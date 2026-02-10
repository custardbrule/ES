using Confluent.Kafka;
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
        ILogger<SyncDailyDiaryConsumer> logger)
        : KafkaConsumerBase<string, SyncMessage>(config, logger)
    {
        protected override string Topic => DiaryTopics.SyncDailyDiary;

        protected override async Task HandleAsync(string key, SyncMessage value, CancellationToken cancellationToken)
        {
            var daily = await kurrentDBClient
                .ReadStreamAsync(Direction.Forwards, value.StreamKey, StreamPosition.Start, cancellationToken: cancellationToken)
                .AggregateAsync(DailyDiary.Init(), (acc, e) => acc.Apply(e.OriginalEvent.Data.ToArray().GetDataFromBytes(e.OriginalEvent.EventType)!), cancellationToken);

            await elasticsearchContext.IndexAsync(DailyDiaryConstants.ESIndex, DailyDiaryConstants.GetId(daily.Id), daily);
            logger.LogInformation("Synced daily diary {DailyDiaryId} from stream {StreamKey}", daily.Id, value.StreamKey);
        }
    }
}
