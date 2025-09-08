using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diary.DiaryRoot
{
    public record InitDiary(string Name, string Description, string AuthorId, EDiaryVisibility Visibility);
    public record ChangeDiaryVisibility(EDiaryVisibility Visibility);
    public record ChangeDiaryInfo(string Name, string Description);

    public record InitDailyDiary(Guid DiaryId);
    public record AddSection(string Detail, bool IsPinned);
    public record RemoveSection(Guid Id);
    public record PinSection(Guid Id, bool IsPinned);
}
