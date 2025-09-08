using Seed;
using System.Text.Json.Serialization;

namespace Domain.Diary.DiaryRoot
{
    public record DiarySection(Guid DiaryId, string Detail, bool IsPinned) : BaseAuditEntity<Guid>(Guid.NewGuid(), DateTimeOffset.UtcNow)
    {
        // Private parameterless constructor for deserialization
        [JsonConstructor]
        private DiarySection() : this(default, "", default) { }

        public static DiarySection Init(Guid diaryId, string detail, bool isPinned) => new DiarySection(diaryId, detail, isPinned);
    }
}
