using App.Extensions.DependencyInjection;
using Data;
using Infras.Extract.Services.Kafka;
using Infras.Extract.Services.Ollama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infras.Extract.Services;

public static class Config
{
    public static IServiceCollection ConfigExtractInfras(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStore();
        services.AddElasticsearchCore(configuration);
        services.AddOptions<GenericDlqOptions>();
        services.RegisterKafkaServices(configuration, typeof(ExtractDailyDiaryConsumer).Assembly);
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddHttpClient<OllamaClient>();
        services.AddHostedService<ElasticsearchExtractIndexInitializer>();
        return services;
    }
}
