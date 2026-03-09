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
    public record AddSectionToDayRequest(Guid DiaryId, DateTimeOffset Date, string Detail, bool IsPinned, DateTimeOffset EventTime) : IRequest<DiarySectionViewModel>;

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
        : IHandler<AddSectionToDayRequest, DiarySectionViewModel>
    {
        public async Task<DiarySectionViewModel> Handle(AddSectionToDayRequest request, CancellationToken cancellationToken)
        {
            var streamKey = DailyDiaryConstants.GetStreamName(request.DiaryId, DateOnly.FromDateTime(request.Date.DateTime));
            var sectionId = Guid.NewGuid();
            var sectionEvent = new EventData(Uuid.NewUuid(), nameof(AddSection), new AddSection(sectionId, request.Detail, request.IsPinned, request.EventTime).ObjectToBytes());

            try
            {
                await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.StreamExists, [sectionEvent], cancellationToken: cancellationToken);
            }
            catch (WrongExpectedVersionException)
            {
                var initEvent = new EventData(Uuid.NewUuid(), nameof(InitDailyDiary), new InitDailyDiary(Guid.NewGuid(), request.DiaryId, string.Empty, request.Date, DateTimeOffset.UtcNow).ObjectToBytes());
                await kurrentDBClient.AppendToStreamAsync(streamKey, StreamState.NoStream, [initEvent, sectionEvent], cancellationToken: cancellationToken);
            }

            producer.Produce(DiaryTopics.SyncDailyDiary, new Message<string, SyncMessage> { Key = streamKey, Value = new SyncMessage(streamKey) });
            return new DiarySectionViewModel(sectionId, Guid.Empty, request.Detail, request.IsPinned, request.EventTime, DateTimeOffset.UtcNow);
        }
    }
}
