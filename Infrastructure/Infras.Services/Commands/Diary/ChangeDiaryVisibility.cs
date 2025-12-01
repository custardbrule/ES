using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using Infras.Services.Jobs;
using KurrentDB.Client;
using Quartz;

namespace Infras.Services.Commands.Diary
{
    public record ChangeDiaryVisibilityRequest(Guid DiaryId, EDiaryVisibility Visibility) : IRequest<long>;

    public class ChangeDiaryVisibilityHandler : IHandler<ChangeDiaryVisibilityRequest, long>
    {
        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IQuartzJobManager _quartzJobManager;

        public ChangeDiaryVisibilityHandler(KurrentDBClient kurrentDBClient, IQuartzJobManager quartzJobManager)
        {
            _kurrentDBClient = kurrentDBClient;
            _quartzJobManager = quartzJobManager;
        }

        public async Task<long> Handle(ChangeDiaryVisibilityRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DiaryConstants.GetStreamName(request.DiaryId);

            var streamResult = _kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamKey, StreamPosition.End, 1, cancellationToken: cancellationToken);
            if (await streamResult.ReadState == ReadState.StreamNotFound) throw new KeyNotFoundException($"Diary with ID {request.DiaryId} not found.");

            var eventData = new EventData(Uuid.NewUuid(), nameof(ChangeDiaryVisibility), new ChangeDiaryVisibility(request.Visibility).ObjectToBytes());
            var res = await _kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            await _quartzJobManager.Trigger(new JobKey(SyncDiaryJob.KEY, SyncDiaryJob.GROUP), new SyncDiaryData(streamKey).GetJobDataMap(), cancellationToken);
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
