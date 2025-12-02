using Domain.Diary.DiaryRoot;

namespace Infras.Services.Queries
{
    public record DailyDiaryViewModel(Guid DiaryId, DateTimeOffset CreatedDate, IReadOnlyList<DiarySection> Sections);
    public record DiaryViewModel(Guid Id, string Name, string Description, string AuthorId, EDiaryVisibility Visibility, DateTimeOffset CreatedDate, DailyDiaryViewModel[] DailyDiaries);
}
