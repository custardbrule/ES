using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record RemoveDiarySectionRequest(Guid DiaryId, DateOnly Date, Guid SectionId) : IRequest<long>;

    public sealed class RemoveDiarySectionValidator : BaseValidator<RemoveDiarySectionRequest>
    {
        public RemoveDiarySectionValidator()
        {
            RuleFor(x => x.DiaryId).With(id => id != Guid.Empty, "DiaryId is required.");
            RuleFor(x => x.SectionId).With(id => id != Guid.Empty, "SectionId is required.");
        }
    }

    public class RemoveDiarySectionHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<RemoveDiarySectionRequest, long>
    {
        public async Task<long> Handle(RemoveDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, request.Date);
            var eventData = new EventData(Uuid.NewUuid(), nameof(RemoveSection), new RemoveSection(request.SectionId).ObjectToBytes());
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
