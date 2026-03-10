using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Infras.Extract.Services.Ollama;

public record ChunkExtraction(
    int SourceIndex,
    string RawText,
    string Category,
    string[] Tags,
    string Sentiment,
    Dictionary<string, object> Data);

public class OllamaClient(HttpClient httpClient, IOptions<OllamaOptions> options)
{
    private readonly OllamaOptions _opts = options.Value;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Splits and extracts structured data from a list of raw text entries.
    /// Each entry may produce multiple chunks (one per atomic topic).
    /// SourceIndex maps each chunk back to its original entry in <paramref name="texts"/>.
    /// </summary>
    private const string ExtractionExample = """
        {
          "chunks": [
            {
              "sourceIndex": 0,
              "rawText": "ran 5km this morning, knees hurt",
              "category": "exercise",
              "tags": ["running", "morning", "pain"],
              "sentiment": "neutral",
              "data": { "distance_km": 5, "time_of_day": "morning", "body_note": "knees hurt" }
            },
            {
              "sourceIndex": 1,
              "rawText": "felt anxious all day",
              "category": "mood",
              "tags": ["anxiety"],
              "sentiment": "negative",
              "data": { "emotion": "anxiety" }
            }
          ]
        }
        """;

    public async Task<List<ChunkExtraction>> ExtractChunksAsync(IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        var numbered = texts.Select((t, i) => $"[{i}] {t}");
        var prompt = $"""
            Analyze these text entries and extract structured data.
            For each atomic topic found, create a separate chunk. Valid categories: exercise, work, mood, food, social, health, finance, other.

            Entries:
            {string.Join("\n", numbered)}

            Return a JSON object using this format (sourceIndex refers to the entry number above):
            {ExtractionExample}
            """;

        var request = new
        {
            model = _opts.ChatModel,
            format = "json",
            stream = false,
            messages = new[]
            {
                new { role = "system", content = "You are a structured data extractor. Extract structured data from text entries. Always return valid JSON only." },
                new { role = "user", content = prompt }
            }
        };

        using var response = await httpClient.PostAsync(
            $"{_opts.BaseUrl}/api/chat",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"), ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(body, JsonOpts);
        var content = chatResponse?.Message?.Content ?? "{}";

        var extracted = JsonSerializer.Deserialize<ExtractionResponse>(content, JsonOpts);
        if (extracted?.Chunks is null) return [];

        return extracted.Chunks
            .Where(c => c.SourceIndex >= 0 && c.SourceIndex < texts.Count)
            .Select(c => new ChunkExtraction(c.SourceIndex, c.RawText, c.Category, c.Tags ?? [], c.Sentiment, c.Data ?? []))
            .ToList();
    }

    /// <summary>
    /// Generates a dense embedding vector for the given text using the configured embed model.
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var request = new { model = _opts.EmbedModel, prompt = text };
        using var response = await httpClient.PostAsync(
            $"{_opts.BaseUrl}/api/embeddings",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"), ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var embedResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(body, JsonOpts);
        return embedResponse?.Embedding ?? [];
    }

    private record OllamaChatResponse(OllamaMessage? Message);
    private record OllamaMessage(string Content);
    private record OllamaEmbeddingResponse(float[] Embedding);
    private record ExtractionResponse(List<RawChunk>? Chunks);
    private record RawChunk(int SourceIndex, string RawText, string Category, string[]? Tags, string Sentiment, Dictionary<string, object>? Data);
}
