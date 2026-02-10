using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record CreateDiaryRequest(string Name, string Description, string AuthorId, EDiaryVisibility DiaryVisibility = EDiaryVisibility.Self) : IRequest<long>;

    public class CreateDiaryHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<CreateDiaryRequest, long>
    {
        public async Task<long> Handle(CreateDiaryRequest request, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();
            var eventData = new EventData(Uuid.NewUuid(), nameof(InitDiary), new InitDiary(id, DateTimeOffset.UtcNow, request.Name, request.Description, request.AuthorId, request.DiaryVisibility).ObjectToBytes());
            var streamKey = DiaryConstants.GetStreamName(id);
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
