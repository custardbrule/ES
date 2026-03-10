using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Contracts.Kafka;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record ChangeDiaryInfoBody(string Name, string Description);
    public record ChangeDiaryInfoRequest(Guid DiaryId, string Name, string Description) : IRequest<long>;

    public sealed class ChangeDiaryInfoValidator : BaseValidator<ChangeDiaryInfoRequest>
    {
        public ChangeDiaryInfoValidator()
        {
            RuleFor(x => x.Name).With(n => !string.IsNullOrWhiteSpace(n), "Name is required.");
        }
    }

    public class ChangeDiaryInfoHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<ChangeDiaryInfoRequest, long>
    {
        public async Task<long> Handle(ChangeDiaryInfoRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DiaryConstants.GetStreamName(request.DiaryId);

            var streamResult = kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamKey, StreamPosition.End, 1, cancellationToken: cancellationToken);
            if (await streamResult.ReadState == ReadState.StreamNotFound) throw new KeyNotFoundException($"Diary with ID {request.DiaryId} not found.");

            var eventData = new EventData(Uuid.NewUuid(), nameof(ChangeDiaryInfo), new ChangeDiaryInfo(request.Name, request.Description).ObjectToBytes());
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
