using Seed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Diary.DiaryRoot
{
    public record Diary(string Name, string Description, string AuthorId, EDiaryVisibility Visibility) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        public static Diary Init(string name, string description, string authorId, EDiaryVisibility visibility) => new Diary(name, description, authorId, visibility);

        public Diary Apply(InitDiary initDiary) => Init(initDiary.Name, initDiary.Description, initDiary.AuthorId, initDiary.Visibility);
        public Diary Apply(ChangeDiaryVisibility changeDiaryVisibility) => this with { Visibility = changeDiaryVisibility.Visibility };
        public Diary Apply(ChangeDiaryInfo changeDiaryInfo) => this with { Name = changeDiaryInfo.Name, Description = changeDiaryInfo.Description };
    }
}
