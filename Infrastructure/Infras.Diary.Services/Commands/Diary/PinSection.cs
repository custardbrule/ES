using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record PinSectionRequest(Guid DayId, string TimeZoneId, Guid SectionId, bool IsPinned) : IRequest<long>;

    public class PinSectionHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<PinSectionRequest, long>
    {
        public async Task<long> Handle(PinSectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DayId, request.TimeZoneId);

            var streamResult = kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamKey, StreamPosition.End, 1, cancellationToken: cancellationToken);
            if (await streamResult.ReadState == ReadState.StreamNotFound) throw new KeyNotFoundException($"Daily diary with ID {request.DayId} not found.");

            var eventData = new EventData(Uuid.NewUuid(), nameof(PinSection), new PinSection(request.SectionId, request.IsPinned).ObjectToBytes());
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
