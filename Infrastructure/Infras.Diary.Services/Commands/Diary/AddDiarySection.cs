using Confluent.Kafka;
using CQRS;
using Domain.Diary.DiaryRoot;
using Infras.Diary.Services.Kafka;
using Infras.Diary.Services.Queries;
using KurrentDB.Client;
using RequestValidatior;
using Uuid = KurrentDB.Client.Uuid;

namespace Infras.Diary.Services.Commands.Diary
{
    public record AddDiarySectionRequest(Guid DiaryId, string TimeZoneId, string Detail, bool IsPinned) : IRequest<DiarySectionViewModel>;

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
        : IHandler<AddDiarySectionRequest, DiarySectionViewModel>
    {
        public async Task<DiarySectionViewModel> Handle(AddDiarySectionRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, request.TimeZoneId);
            var sectionId = Guid.NewGuid();
            var eventTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId));
            var sectionEvent = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(sectionId, request.Detail, request.IsPinned, eventTime).ObjectToBytes());

            try
            {
                await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [sectionEvent], cancellationToken: cancellationToken);
            }
            catch (WrongExpectedVersionException)
            {
                var date = DailyDiaryConstants.GetDateFromTimeZone(request.TimeZoneId);
                var initEvent = new EventData(Uuid.NewUuid(), nameof(InitDailyDiary), new InitDailyDiary(Guid.NewGuid(), request.DiaryId, request.TimeZoneId, date.ToDateTime(TimeOnly.MinValue), DateTimeOffset.UtcNow).ObjectToBytes());
                await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [initEvent, sectionEvent], cancellationToken: cancellationToken);
            }

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return new DiarySectionViewModel(sectionId, request.DiaryId, request.Detail, request.IsPinned, eventTime, DateTimeOffset.UtcNow);
        }
    }
}
