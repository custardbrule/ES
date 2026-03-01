using Data;
using Domain.Diary.DiaryRoot;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infras.Diary.Services
{
    internal sealed class ElasticsearchIndexInitializer(
        IElasticSearchContext elasticsearchContext,
        ILogger<ElasticsearchIndexInitializer> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await elasticsearchContext.EnsureIndexExistsAsync(DiaryConstants.ESIndex);
            logger.LogInformation("Elasticsearch index '{Index}' is ready.", DiaryConstants.ESIndex);

            await elasticsearchContext.EnsureIndexExistsAsync(DailyDiaryConstants.ESIndex);
            logger.LogInformation("Elasticsearch index '{Index}' is ready.", DailyDiaryConstants.ESIndex);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
