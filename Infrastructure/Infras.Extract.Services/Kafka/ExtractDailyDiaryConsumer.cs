namespace Infras.Extract.Services.Kafka;

[RegisterKafkaConsumer]
public class ExtractDailyDiaryConsumer(
    IOptions<ConsumerConfig> config,
    IElasticSearchContext es,
    OllamaClient ollama,
    ILogger<ExtractDailyDiaryConsumer> logger)
    : KafkaConsumerBase<string, ExtractDailyDiaryMessage>(config, logger)
{
    protected override string Topic => DiaryTopics.ExtractDailyDiary;

    protected override async Task HandleAsync(string key, ExtractDailyDiaryMessage value, Headers _, CancellationToken ct)
    {
        var response = await es.Client.GetAsync<DailyDiary>(
            DailyDiaryConstants.ESIndex, DailyDiaryConstants.GetId(value.DayId), ct);

        if (!response.IsValidResponse || !response.Found) return;

        var daily = response.Source!;
        if (daily.Sections.Count == 0) return;

        var texts = daily.Sections.Select(s => s.Detail).ToList();
        var chunks = await ollama.ExtractChunksAsync(texts, ct);
        if (chunks.Count == 0) return;

        var idMap = new Dictionary<Guid, string>();
        var docs = new List<ExtractedChunk>();

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var section = daily.Sections[chunk.SourceIndex];
            var embedding = await ollama.GenerateEmbeddingAsync(chunk.RawText, ct);
            var doc = new ExtractedChunk(
                Guid.NewGuid(), section.Id, daily.DiaryId, daily.Id,
                section.EventTime, chunk.RawText, chunk.Category, chunk.Tags,
                chunk.Sentiment, chunk.Data, embedding, DateTimeOffset.UtcNow);
            idMap[doc.Id] = ExtractConstants.GetId(section.Id, i);
            docs.Add(doc);
        }

        await es.BulkUpsertAsync(ExtractConstants.ESIndex, docs, doc => idMap[doc.Id]);
        logger.LogInformation("Extracted {ChunkCount} chunks from day {DayId}", docs.Count, value.DayId);
    }

    protected override Task OnMessageFailedAsync(string key, ExtractDailyDiaryMessage value, Exception ex, int retryCount, CancellationToken ct)
        => SendToDlqAsync(key, value, ex, retryCount, ct);
}
