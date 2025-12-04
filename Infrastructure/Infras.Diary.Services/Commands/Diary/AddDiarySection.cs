using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Jobs;
using KurrentDB.Client;
using Quartz;
using System.Text.Json;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDiarySectionRequest(Guid DiaryId, Guid DayId, string TimeZoneId, string Detail, bool IsPinned) : IRequest<long>;

    public class AddDiarySectionHandler : IHandler<AddDiarySectionRequest, long>
    {
        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IQuartzJobManager _quartzJobManager;

        public AddDiarySectionHandler(KurrentDBClient kurrentDBClient, IQuartzJobManager quartzJobManager)
        {
            _kurrentDBClient = kurrentDBClient;
            _quartzJobManager = quartzJobManager;
        }

        async Task<long> IHandler<AddDiarySectionRequest, long>.Handle(AddDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var eventData = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(request.Detail, request.IsPinned).ObjectToBytes());
            var streamKey = DailyDiaryConstants.GetStreamName(request.DayId, request.TimeZoneId);
            var res = await _kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            await _quartzJobManager.Trigger(new JobKey(SyncDailyDiaryJob.KEY, SyncDailyDiaryJob.GROUP), new SyncDailyDiaryData(streamKey).GetJobDataMap(), cancellationToken);
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
