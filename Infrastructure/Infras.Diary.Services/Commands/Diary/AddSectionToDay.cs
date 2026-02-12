using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddSectionToDayRequest(Guid DiaryId, DateOnly Date, string Detail, bool IsPinned) : IRequest<long>;

    public sealed class AddSectionToDayValidator : BaseValidator<AddSectionToDayRequest>
    {
        public AddSectionToDayValidator()
        {
            RuleFor(x => x.DiaryId).With(id => id != Guid.Empty, "DiaryId is required.");
            RuleFor(x => x.Detail).With(d => !string.IsNullOrWhiteSpace(d), "Detail is required.");
        }
    }

    public class AddSectionToDayHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<AddSectionToDayRequest, long>
    {
        public async Task<long> Handle(AddSectionToDayRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, request.Date);
            var eventData = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(request.Detail, request.IsPinned).ObjectToBytes());
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
