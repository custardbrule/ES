using Seed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diary.DiaryRoot
{
    public record DailyDiary(Guid DiaryId, IReadOnlyList<DiarySection> Sections) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        public static DailyDiary Init(Guid diaryId) => new DailyDiary(diaryId, []);

        public DailyDiary Apply(InitDailyDiary init) => Init(init.DiaryId);
        public DailyDiary Apply(AddSection addSection) => this with { Sections = [.. Sections, DiarySection.Init(Id, addSection.Detail, addSection.IsPinned)] };
        public DailyDiary Apply(RemoveSection removeSection) => this with { Sections = [.. Sections.Where(v => v.Id != removeSection.Id)] };
        public DailyDiary Apply(PinSection pinSection) => this with { Sections = [.. Sections.Select(s => s.Id == pinSection.Id ? s with { IsPinned = pinSection.IsPinned } : s)] };
    }
}
