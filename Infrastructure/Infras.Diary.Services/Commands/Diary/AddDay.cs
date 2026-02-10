using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDayRequest(Guid DiaryId, string TimeZoneId) : IRequest<long>;

    public class AddDayHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<AddDayRequest, long>
    {
        public async Task<long> Handle(AddDayRequest request, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var eventData = new EventData(Uuid.NewUuid(), nameof(InitDailyDiary), new InitDailyDiary(id, request.DiaryId, request.TimeZoneId, DateTimeOffset.UtcNow).ObjectToBytes());
            var streamKey = DailyDiaryConstants.GetStreamName(id, request.TimeZoneId);
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
