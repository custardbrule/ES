using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Jobs;
using KurrentDB.Client;
using Quartz;

namespace Infras.Diary.Services.Commands.Diary
{
    public record PinSectionRequest(Guid DayId, string TimeZoneId, Guid SectionId, bool IsPinned) : IRequest<long>;

    public class PinSectionHandler : IHandler<PinSectionRequest, long>
    {
        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IQuartzJobManager _quartzJobManager;

        public PinSectionHandler(KurrentDBClient kurrentDBClient, IQuartzJobManager quartzJobManager)
        {
            _kurrentDBClient = kurrentDBClient;
            _quartzJobManager = quartzJobManager;
        }

        public async Task<long> Handle(PinSectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DayId, request.TimeZoneId);

            var streamResult = _kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamKey, StreamPosition.End, 1, cancellationToken: cancellationToken);
            if (await streamResult.ReadState == ReadState.StreamNotFound) throw new KeyNotFoundException($"Daily diary with ID {request.DayId} not found.");

            var eventData = new EventData(Uuid.NewUuid(), nameof(PinSection), new PinSection(request.SectionId, request.IsPinned).ObjectToBytes());
            var res = await _kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            await _quartzJobManager.Trigger(new JobKey(SyncDailyDiaryJob.KEY, SyncDailyDiaryJob.GROUP), new SyncDailyDiaryData(streamKey).GetJobDataMap(), cancellationToken);
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
