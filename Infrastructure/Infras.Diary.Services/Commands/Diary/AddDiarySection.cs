using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDiarySectionRequest(Guid DiaryId, Guid DayId, string TimeZoneId, string Detail, bool IsPinned) : IRequest<long>;

    public class AddDiarySectionHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<AddDiarySectionRequest, long>
    {
        public async Task<long> Handle(AddDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var eventData = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(request.Detail, request.IsPinned).ObjectToBytes());
            var streamKey = DailyDiaryConstants.GetStreamName(request.DayId, request.TimeZoneId);
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
