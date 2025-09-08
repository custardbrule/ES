using Seed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Diary.DiaryRoot
{
    public record Diary(string Name, string Description, string AuthorId, EDiaryVisibility Visibility) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        // Private parameterless constructor for deserialization
        [JsonConstructor]
        private Diary() : this("", "", "", EDiaryVisibility.Public) { }

        public static Diary Init() => new Diary("", "", "", EDiaryVisibility.Public);

        public Diary Apply(InitDiary initDiary) => this with { Id = initDiary.Id, CreatedDate = initDiary.CreatedDate, Name = initDiary.Name, Description = initDiary.Description, AuthorId = initDiary.AuthorId, Visibility = initDiary.Visibility };
        public Diary Apply(ChangeDiaryVisibility changeDiaryVisibility) => this with { Visibility = changeDiaryVisibility.Visibility };
        public Diary Apply(ChangeDiaryInfo changeDiaryInfo) => this with { Name = changeDiaryInfo.Name, Description = changeDiaryInfo.Description };
    }
}
