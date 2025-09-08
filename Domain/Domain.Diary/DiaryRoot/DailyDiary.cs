using Seed;
using System.Text.Json.Serialization;

namespace Domain.Diary.DiaryRoot
{
    public record DailyDiary(Guid DiaryId, IReadOnlyList<DiarySection> Sections) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        // Private parameterless constructor for deserialization
        [JsonConstructor]
        private DailyDiary() : this(default, []) { }

        public static DailyDiary Init() => new DailyDiary(Guid.Empty, []);

        public DailyDiary Apply(object e)
        {
            return e switch
            {
                InitDailyDiary init => Apply(init),
                AddSection add => Apply(add),
                RemoveSection remove => Apply(remove),
                PinSection pin => Apply(pin),
                _ => this,
            };
        }
        private DailyDiary Apply(InitDailyDiary init) => this with { Id = init.Id, DiaryId = init.DiaryId, CreatedDate = init.CreatedDate };
        private DailyDiary Apply(AddSection addSection) => this with { Sections = [.. Sections, DiarySection.Init(Id, addSection.Detail, addSection.IsPinned)] };
        private DailyDiary Apply(RemoveSection removeSection) => this with { Sections = [.. Sections.Where(v => v.Id != removeSection.Id)] };
        private DailyDiary Apply(PinSection pinSection) => this with { Sections = [.. Sections.Select(s => s.Id == pinSection.Id ? s with { IsPinned = pinSection.IsPinned } : s)] };
    }
}
