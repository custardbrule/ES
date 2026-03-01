using Data;
using Domain.Diary.DiaryRoot;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infras.Diary.Services
{
    internal sealed class ElasticsearchIndexInitializer(
        IElasticSearchContext elasticsearchContext,
        ILogger<ElasticsearchIndexInitializer> logger) : IHostedService
    {
        // DateTimeOffset serializes with up to 7 fractional second digits, which Elasticsearch's
        // default date detection (strict_date_optional_time, max 3 digits) cannot parse.
        // It would auto-map createdDate as text, making sort fail with "all shards failed".
        // Explicit date mapping with nanos format fixes this.
        private const string DateFormat = "strict_date_optional_time_nanos||strict_date_optional_time||epoch_millis";

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await EnsureIndexAsync(DiaryConstants.ESIndex, cancellationToken);
            logger.LogInformation("Elasticsearch index '{Index}' is ready.", DiaryConstants.ESIndex);

            await EnsureIndexAsync(DailyDiaryConstants.ESIndex, cancellationToken);
            logger.LogInformation("Elasticsearch index '{Index}' is ready.", DailyDiaryConstants.ESIndex);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task EnsureIndexAsync(string indexName, CancellationToken cancellationToken)
        {
            var response = await elasticsearchContext.Client.Indices.CreateAsync(indexName, c => c
                .Mappings(m => m
                    .Properties(p => p
                        .Date("createdDate", d => d.Format(DateFormat))
                    )
                ), cancellationToken);

            if (!response.IsSuccess() && response.ElasticsearchServerError?.Error?.Type != "resource_already_exists_exception")
                throw new Exception($"Failed to create index '{indexName}': {response.ElasticsearchServerError?.Error?.Reason}");
        }
    }
}
