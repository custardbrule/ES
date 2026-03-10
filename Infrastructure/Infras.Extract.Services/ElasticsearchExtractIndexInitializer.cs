namespace Infras.Extract.Services;

internal sealed class ElasticsearchExtractIndexInitializer(
    IElasticSearchContext elasticsearchContext,
    ILogger<ElasticsearchExtractIndexInitializer> logger) : IHostedService
{
    private const string DateFormat = "strict_date_optional_time_nanos||strict_date_optional_time||epoch_millis";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var response = await elasticsearchContext.Client.Indices.CreateAsync(ExtractConstants.ESIndex, c => c
            .Mappings(m => m
                .Properties(p => p
                    .Keyword("sectionId")
                    .Keyword("diaryId")
                    .Keyword("dayId")
                    .Date("eventTime", d => d.Format(DateFormat))
                    .Date("createdDate", d => d.Format(DateFormat))
                    .Text("rawText")
                    .Keyword("category")
                    .Keyword("tags")
                    .Keyword("sentiment")
                    .DenseVector("embedding", dv => dv
                        .Dims(768)
                        .Index(true)
                        .Similarity(DenseVectorSimilarity.Cosine))
                )
            ), cancellationToken);

        if (!response.IsSuccess() && response.ElasticsearchServerError?.Error?.Type != "resource_already_exists_exception")
            throw new Exception($"Failed to create index '{ExtractConstants.ESIndex}': {response.ElasticsearchServerError?.Error?.Reason}");

        logger.LogInformation("Elasticsearch index '{Index}' is ready.", ExtractConstants.ESIndex);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
