using Domain.Diary.DiaryRoot;

namespace Infras.Diary.Services.Queries
{
    public record DiarySectionViewModel(Guid Id, Guid DiaryId, string Detail, bool IsPinned, DateTimeOffset EventTime, DateTimeOffset CreatedDate);
    public record DailyDiaryViewModel(Guid DiaryId, DateTimeOffset Date, DateTimeOffset CreatedDate, IReadOnlyList<DiarySection> Sections);
    public record DiaryViewModel(Guid Id, string Name, string Description, string AuthorId, string AuthorName, EDiaryVisibility Visibility, DateTimeOffset CreatedDate, DailyDiaryViewModel[] DailyDiaries);
}
