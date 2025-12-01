using Seed;
using System.Text.Json.Serialization;

namespace Domain.Diary.DiaryRoot
{
    public record DailyDiary(Guid DiaryId, string TimeZoneId, IReadOnlyList<DiarySection> Sections) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        // Private parameterless constructor for deserialization
        [JsonConstructor]
        private DailyDiary() : this(default, String.Empty, []) { }

        public static DailyDiary Init() => new DailyDiary(Guid.Empty, String.Empty, []);

        public DailyDiary Apply(object e) => e switch
        {
            InitDailyDiary init => Apply(init),
            AddSection add => Apply(add),
            PinSection pin => Apply(pin),
            _ => this,
        };
        private DailyDiary Apply(InitDailyDiary init) => this with { Id = init.Id, DiaryId = init.DiaryId, TimeZoneId = init.TimeZoneId, CreatedDate = init.CreatedDate };
        private DailyDiary Apply(AddSection addSection) => this with { Sections = [.. Sections, DiarySection.Init(Id, addSection.Detail, addSection.IsPinned)] };
        private DailyDiary Apply(PinSection pinSection) => this with { Sections = [.. Sections.Select(s => s.Id == pinSection.Id ? s with { IsPinned = pinSection.IsPinned } : s)] };
    }
}
