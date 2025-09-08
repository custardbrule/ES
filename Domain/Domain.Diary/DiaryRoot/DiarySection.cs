using Seed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diary.DiaryRoot
{
    public record DiarySection(Guid DiaryId, string Detail, bool IsPinned) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        public static DiarySection Init(Guid diaryId, string detail, bool isPinned) => new DiarySection(diaryId, detail, isPinned);
    }
}
