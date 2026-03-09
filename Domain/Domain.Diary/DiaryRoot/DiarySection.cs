using Seed;
using System.Text.Json.Serialization;

namespace Domain.Diary.DiaryRoot
{
    public record DiarySection(Guid DayId, string Detail, bool IsPinned, DateTimeOffset EventTime) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        // Private parameterless constructor for deserialization
        [JsonConstructor]
        private DiarySection() : this(default, "", default, default) { }

        public static DiarySection Init(Guid dayId, string detail, bool isPinned, DateTimeOffset eventTime) => new(dayId, detail, isPinned, eventTime);
    }
}
