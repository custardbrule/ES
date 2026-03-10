namespace Contracts.Kafka;

public record ExtractDailyDiaryMessage(Guid DayId, string Operation);
