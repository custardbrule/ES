using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record ChangeDiaryVisibilityRequest(Guid DiaryId, EDiaryVisibility Visibility) : IRequest<long>;

    public class ChangeDiaryVisibilityHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<ChangeDiaryVisibilityRequest, long>
    {
        public async Task<long> Handle(ChangeDiaryVisibilityRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DiaryConstants.GetStreamName(request.DiaryId);

            var streamResult = kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamKey, StreamPosition.End, 1, cancellationToken: cancellationToken);
            if (await streamResult.ReadState == ReadState.StreamNotFound) throw new KeyNotFoundException($"Diary with ID {request.DiaryId} not found.");

            var eventData = new EventData(Uuid.NewUuid(), nameof(ChangeDiaryVisibility), new ChangeDiaryVisibility(request.Visibility).ObjectToBytes());
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
