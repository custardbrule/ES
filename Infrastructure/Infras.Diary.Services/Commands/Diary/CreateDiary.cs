using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Contracts.Kafka;
using Infras.Diary.Services.Queries;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record CreateDiaryRequest(string Name, string Description, string AuthorId, string AuthorName, EDiaryVisibility DiaryVisibility = EDiaryVisibility.Self) : IRequest<DiaryViewModel>;

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
        : IHandler<CreateDiaryRequest, DiaryViewModel>
    {
        public async Task<DiaryViewModel> Handle(CreateDiaryRequest request, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var createdDate = DateTimeOffset.UtcNow;
            var initEvent = new InitDiary(id, createdDate, request.Name, request.Description, request.AuthorId, request.AuthorName, request.DiaryVisibility);
            var eventData = new EventData(Uuid.NewUuid(), nameof(InitDiary), initEvent.ObjectToBytes());
            var streamKey = DiaryConstants.GetStreamName(id);
            await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return new DiaryViewModel(id, initEvent.Name, initEvent.Description, initEvent.AuthorId, initEvent.AuthorName, initEvent.Visibility, createdDate, []);
        }
    }
}
