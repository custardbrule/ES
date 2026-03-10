namespace Domain.Extract;

public record ExtractedChunk(
    Guid Id,
    Guid SectionId,
    Guid DiaryId,
    Guid DayId,
    DateTimeOffset EventTime,
    string RawText,
    string Category,
    string[] Tags,
    string Sentiment,
    Dictionary<string, object> Data,
    float[] Embedding,
    DateTimeOffset CreatedDate);
