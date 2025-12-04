using Data;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Constants;
using KurrentDB.Client;
using Quartz;
using System.Collections;
using System.Runtime.Serialization;

namespace Infras.Diary.Services.Jobs
{
    public record SyncDiaryData(string StreamKey)
    {
        public JobDataMap GetJobDataMap() => new JobDataMap { { nameof(StreamKey), StreamKey } };
    }

    public class SyncDiaryJob : IAppJob
    {
        public const string KEY = nameof(SyncDiaryJob);
        public const string GROUP = JobConstants.SYNC_DATA_GROUPD;

        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IElasticSearchContext _elasticsearchContext;

        public SyncDiaryJob(IElasticSearchContext elasticsearchContext, KurrentDBClient kurrentDBClient)
        {
            _elasticsearchContext = elasticsearchContext;
            _kurrentDBClient = kurrentDBClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!context.MergedJobDataMap.TryGetString(nameof(SyncDiaryData.StreamKey), out var streamKey)) return;

            // rebuild model
            var ct = CancellationToken.None;
            var daily = await _kurrentDBClient.ReadStreamAsync(Direction.Forwards, streamKey!, StreamPosition.Start, cancellationToken: ct).AggregateAsync(Domain.Diary.DiaryRoot.Diary.Init(), (acc, e) => acc.Apply(e.OriginalEvent.Data.ToArray().GetDataFromBytes(e.OriginalEvent.EventType)!), ct);
            var res = await _elasticsearchContext.IndexAsync(DiaryConstants.ESIndex, DiaryConstants.GetId(daily.Id), daily);
        }
    }
}
