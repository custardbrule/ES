using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDiarySectionRequestBody(Guid DiaryId, string TimeZoneId, string Detail, bool IsPinned) : IRequest<long>;
    public record AddDiarySectionRequest(Guid DiaryId, AddDiarySectionRequestBody Body) : IRequest<long>;

    public class AddDiarySectionHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<AddDiarySectionRequest, long>
    {
        public async Task<long> Handle(AddDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, request.Body.TimeZoneId);
            var sectionEvent = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(request.Body.Detail, request.Body.IsPinned).ObjectToBytes());

            long nextState;
            try
            {
                var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [sectionEvent], cancellationToken: cancellationToken);
                nextState = res.NextExpectedStreamState.ToInt64();
            }
            catch (WrongExpectedVersionException)
            {
                var initEvent = new EventData(Uuid.NewUuid(), nameof(InitDailyDiary), new InitDailyDiary(Guid.NewGuid(), request.DiaryId, request.Body.TimeZoneId, DateTimeOffset.UtcNow).ObjectToBytes());
                var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [initEvent, sectionEvent], cancellationToken: cancellationToken);
                nextState = res.NextExpectedStreamState.ToInt64();
            }

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return nextState;
        }
    }
}
