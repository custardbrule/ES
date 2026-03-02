using Confluent.Kafka;
using Data;
using Domain.Diary.DiaryRoot;
using KurrentDB.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infras.Diary.Services.Kafka
{
    [RegisterKafkaConsumer]
    public class SyncDiaryConsumer(
        IOptions<ConsumerConfig> config,
        KurrentDBClient kurrentDBClient,
        IElasticSearchContext elasticsearchContext,
        ILogger<SyncDiaryConsumer> logger)
        : KafkaConsumerBase<string, SyncMessage>(config, logger)
    {
        protected override string Topic => DiaryTopics.SyncDiary;

        protected override async Task HandleAsync(string key, SyncMessage value, Headers _, CancellationToken cancellationToken)
        {
            var diary = await kurrentDBClient
                .ReadStreamAsync(Direction.Forwards, value.StreamKey, StreamPosition.Start, cancellationToken: cancellationToken)
                .AggregateAsync(Domain.Diary.DiaryRoot.Diary.Init(), (acc, e) => acc.Apply(e.OriginalEvent.Data.ToArray().GetDataFromBytes(e.OriginalEvent.EventType)!), cancellationToken);

            await elasticsearchContext.IndexAsync(DiaryConstants.ESIndex, DiaryConstants.GetId(diary.Id), diary);
            logger.LogInformation("Synced diary {DiaryId} from stream {StreamKey}", diary.Id, value.StreamKey);
        }

        protected override Task OnMessageFailedAsync(string key, SyncMessage value, Exception exception, int retryCount, CancellationToken cancellationToken)
            => SendToDlqAsync(key, value, exception, retryCount, cancellationToken);
    }
}
