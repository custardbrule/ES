using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record PinSectionBody(string TimeZoneId, Guid SectionId, bool IsPinned);
    public record PinSectionRequest(Guid DiaryId, string TimeZoneId, Guid SectionId, bool IsPinned) : IRequest<long>;

    public sealed class PinSectionValidator : BaseValidator<PinSectionRequest>
    {
        public PinSectionValidator()
        {
            RuleFor(x => x.TimeZoneId).With(tz => ValidatorHelper.IsValidTimeZone(tz), "TimeZoneId is invalid.");
            RuleFor(x => x.SectionId).With(id => id != Guid.Empty, "SectionId is required.");
        }
    }

    public class PinSectionHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<PinSectionRequest, long>
    {
        public async Task<long> Handle(PinSectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, request.TimeZoneId);

            var streamResult = kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamKey, StreamPosition.End, 1, cancellationToken: cancellationToken);
            if (await streamResult.ReadState == ReadState.StreamNotFound) throw new KeyNotFoundException($"Daily diary not found.");

            var eventData = new EventData(Uuid.NewUuid(), nameof(PinSection), new PinSection(request.SectionId, request.IsPinned).ObjectToBytes());
            var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [eventData], cancellationToken: cancellationToken);

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return res.NextExpectedStreamState.ToInt64();
        }
    }
}
