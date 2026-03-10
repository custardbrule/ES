using Confluent.Kafka;
using Contracts.Kafka;
using Data;
using Domain.Diary.DiaryRoot;
using Domain.Extract;
using Infras.Extract.Services.Ollama;
using KurrentDB.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infras.Extract.Services.Kafka;

[RegisterKafkaConsumer]
public class ExtractDailyDiaryConsumer(
    IOptions<ConsumerConfig> config,
    KurrentDBClient kurrentDBClient,
    OllamaClient ollama,
    IElasticSearchContext es,
    ILogger<ExtractDailyDiaryConsumer> logger)
    : KafkaConsumerBase<string, SyncMessage>(config, logger)
{
    protected override string Topic => DiaryTopics.SyncDailyDiary;

    protected override async Task HandleAsync(string key, SyncMessage value, Headers _, CancellationToken ct)
    {
        var daily = await kurrentDBClient
            .ReadStreamAsync(Direction.Forwards, value.StreamKey, StreamPosition.Start, cancellationToken: ct)
            .AggregateAsync(DailyDiary.Init(), (acc, e) =>
                acc.Apply(e.OriginalEvent.Data.ToArray().GetDataFromBytes(e.OriginalEvent.EventType)!), ct);

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
        logger.LogInformation("Extracted {ChunkCount} chunks from stream {StreamKey}", docs.Count, value.StreamKey);
    }

    protected override Task OnMessageFailedAsync(string key, SyncMessage value, Exception ex, int retryCount, CancellationToken ct)
        => SendToDlqAsync(key, value, ex, retryCount, ct);
}
