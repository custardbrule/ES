using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Jobs;
using KurrentDB.Client;
using Quartz;
using System.Text.Json;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDayRequest(Guid DiaryId, string TimeZoneId) : IRequest<long>;

    public class AddDayHandler : IHandler<AddDayRequest, long>
    {
        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IQuartzJobManager _quartzJobManager;

        public AddDayHandler(KurrentDBClient kurrentDBClient, IQuartzJobManager quartzJobManager)
        {
            _kurrentDBClient = kurrentDBClient;
            _quartzJobManager = quartzJobManager;
        }

        async Task<long> IHandler<AddDayRequest, long>.Handle(AddDayRequest request, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var eventData = new EventData(Uuid.NewUuid(), nameof(InitDailyDiary), new InitDailyDiary(id, request.DiaryId, request.TimeZoneId, DateTimeOffset.UtcNow).ObjectToBytes());
            var streamKey = DailyDiaryConstants.GetStreamName(id, request.TimeZoneId);
            var res = await _kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [eventData], cancellationToken: cancellationToken);

            await _quartzJobManager.Trigger(new JobKey(SyncDailyDiaryJob.KEY, SyncDailyDiaryJob.GROUP), new SyncDailyDiaryData(streamKey).GetJobDataMap(), cancellationToken);
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
