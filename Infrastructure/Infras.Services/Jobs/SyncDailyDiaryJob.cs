using Data;
using Domain.Diary.DiaryRoot;
using Infras.Services.Constants;
using KurrentDB.Client;
using Quartz;

namespace Infras.Services.Jobs
{
    public class SyncDailyDiaryJob : IAppJob
    {
        public const string KEY = nameof(SyncDailyDiaryJob);
        public const string GROUP = JobConstants.SYNC_DATA_GROUPD;

        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IElasticSearchContext _elasticsearchContext;

        public SyncDailyDiaryJob(IElasticSearchContext elasticsearchContext, KurrentDBClient kurrentDBClient)
        {
            _elasticsearchContext = elasticsearchContext;
            _kurrentDBClient = kurrentDBClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!context.MergedJobDataMap.TryGetString("StreamKey", out var streamKey)) return;

            // rebuild model
            var ct = CancellationToken.None;
            var daily = await _kurrentDBClient.ReadStreamAsync(Direction.Forwards, streamKey!, StreamPosition.Start, cancellationToken: ct).AggregateAsync(DailyDiary.Init(), (acc, e) => acc.Apply(e.OriginalEvent.Data.ToArray().GetDataFromBytes(e.OriginalEvent.EventType)!), ct);
            var res = await _elasticsearchContext.IndexAsync(DiaryConstants.DailyDiaryIndex, DiaryConstants.GetId(daily.Id), daily);
        }
    }
}
