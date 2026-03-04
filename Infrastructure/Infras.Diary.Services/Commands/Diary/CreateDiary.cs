using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record CreateDiaryRequest(string Name, string Description, string AuthorId, string AuthorName, EDiaryVisibility DiaryVisibility = EDiaryVisibility.Self) : IRequest<long>;

    public sealed class CreateDiaryValidator : BaseValidator<CreateDiaryRequest>
    {
        public CreateDiaryValidator()
        {
            RuleFor(x => x.Name).With(n => !string.IsNullOrWhiteSpace(n), "Name is required.");
            RuleFor(x => x.AuthorId).With(a => !string.IsNullOrWhiteSpace(a), "AuthorId is required.");
        }
    }

    public class CreateDiaryHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<CreateDiaryRequest, long>
    {
        public async Task<long> Handle(CreateDiaryRequest request, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var eventData = new EventData(Uuid.NewUuid(), nameof(InitDiary), new InitDiary(id, DateTimeOffset.UtcNow, request.Name, request.Description, request.AuthorId, request.AuthorName, request.DiaryVisibility).ObjectToBytes());
            var streamKey = DiaryConstants.GetStreamName(id);
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
