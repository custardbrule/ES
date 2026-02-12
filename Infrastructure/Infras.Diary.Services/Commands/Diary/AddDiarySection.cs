using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDiarySectionRequest(Guid DiaryId, string TimeZoneId, string Detail, bool IsPinned) : IRequest<long>;

    public sealed class AddDiarySectionValidator : BaseValidator<AddDiarySectionRequest>
    {
        public AddDiarySectionValidator()
        {
            RuleFor(x => x.DiaryId).With(id => id != Guid.Empty, "DiaryId is required.");
            RuleFor(x => x.TimeZoneId).With(tz => ValidatorHelper.IsValidTimeZone(tz), "TimeZoneId is invalid.");
            RuleFor(x => x.Detail).With(d => !string.IsNullOrWhiteSpace(d), "Detail is required.");
        }
    }

    public class AddDiarySectionHandler(
        KurrentDBClient kurrentDBClient,
        IProducer<string, SyncMessage> producer)
        : IHandler<AddDiarySectionRequest, long>
    {
        public async Task<long> Handle(AddDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, request.TimeZoneId);
            var sectionEvent = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(request.Detail, request.IsPinned).ObjectToBytes());

            long nextState;
            try
            {
                var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [sectionEvent], cancellationToken: cancellationToken);
                nextState = res.NextExpectedStreamState.ToInt64();
            }
            catch (WrongExpectedVersionException)
            {
                var date = DailyDiaryConstants.GetDateFromTimeZone(request.TimeZoneId);
                var initEvent = new EventData(Uuid.NewUuid(), nameof(InitDailyDiary), new InitDailyDiary(Guid.NewGuid(), request.DiaryId, request.TimeZoneId, date.ToDateTime(TimeOnly.MinValue), DateTimeOffset.UtcNow).ObjectToBytes());
                var res = await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [initEvent, sectionEvent], cancellationToken: cancellationToken);
                nextState = res.NextExpectedStreamState.ToInt64();
            }

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return nextState;
        }
    }
}
