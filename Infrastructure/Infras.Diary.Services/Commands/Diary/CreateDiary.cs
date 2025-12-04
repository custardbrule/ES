using CQRS;
using Data;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Jobs;
using KurrentDB.Client;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infras.Diary.Services.Commands.Diary
{
    public record CreateDiaryRequest(string Name, string Description, string AuthorId, EDiaryVisibility DiaryVisibility = EDiaryVisibility.Self) : IRequest<long>;

    public class CreateDiaryHandler : IHandler<CreateDiaryRequest, long>
    {
        private readonly KurrentDBClient _kurrentDBClient;
        private readonly IQuartzJobManager _quartzJobManager;


        public CreateDiaryHandler(KurrentDBClient kurrentDBClient, IQuartzJobManager quartzJobManager)
        {
            _kurrentDBClient = kurrentDBClient;
            _quartzJobManager = quartzJobManager;
        }

        public async Task<long> Handle(CreateDiaryRequest request, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var eventData = new EventData(Uuid.NewUuid(), nameof(InitDiary), new InitDiary(id, DateTimeOffset.UtcNow, request.Name, request.Description, request.AuthorId, request.DiaryVisibility).ObjectToBytes());
            var streamKey = DiaryConstants.GetStreamName(id);
            var res = await _kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            await _quartzJobManager.Trigger(new JobKey(SyncDiaryJob.KEY, SyncDiaryJob.GROUP), new SyncDiaryData(streamKey).GetJobDataMap(), cancellationToken);
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
