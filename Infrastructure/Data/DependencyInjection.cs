using Confluent.Kafka;
using Data;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Reflection;

namespace App.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services)
        {
            services.AddKurrentDBClient(sp => sp.GetRequiredService<IConfiguration>().GetSection("KurrentDB").Value!);

            return services;
        }

        public static IServiceCollection AddElasticsearchCore(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<ElasticsearchOptions>()
                .Configure(options => configuration.GetSection(ElasticsearchOptions.SectionName).Bind(options));

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
                var settings = options.ToClientSettings();
                return new ElasticsearchClient(settings);
            });

            services.AddSingleton<IElasticSearchContext, ElasticSearchContext>();

            return services;
        }

        public static IServiceCollection RegisterQuartz(this IServiceCollection services, string connectionString, string schedulerName)
        {
            services.AddQuartz(cfg =>
            {
                cfg.UsePersistentStore(o =>
                {
                    o.UseSqlServer(c =>
                    {
                        c.ConnectionString = connectionString;
                        c.TablePrefix = "QRTZ_";
                    });
                    o.UseSystemTextJsonSerializer();
                });

                cfg.UseSimpleTypeLoader();
                cfg.UseDefaultThreadPool(5);
                cfg.SchedulerName = schedulerName;
            });

            services.AddQuartzHostedService(cfg =>
            {
                cfg.WaitForJobsToComplete = true;
                cfg.AwaitApplicationStarted = true;
            });

            services.AddSingleton<IQuartzJobManager, QuartzJobManager>();
            return services;
        }

        public static IServiceCollection RegisterKafkaServices(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            services
                .AddOptions<ProducerConfig>()
                .Configure(options => configuration.GetSection("KafkaSettings:ProducerSettings").Bind(options));

            var typesWithAttributes = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<RegisterKafkaProducerAttribute>(true) is not null &&
                             !type.IsAbstract &&
                             !type.IsInterface)
                .ToList();

            foreach (var type in typesWithAttributes)
            {
                var attribute = type.GetCustomAttribute<RegisterKafkaProducerAttribute>(true)!;
                // Add factory
                services.TryAdd(attribute.GetServiceDescriptor());
                // add IProducer<,>
                services.TryAdd(attribute.GetProducerServiceDescriptor());
            }

            return services;
        }

        public static IServiceCollection RegisterDbContextPool<T>(this IServiceCollection serviceCollection, string connectionString, int poolSize = 128, bool enableSensitiveDataLogging = false) where T : DbContext
        {
            serviceCollection.AddPooledDbContextFactory<T>(options =>
            {
                options.UseSqlServer(connectionString)
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging(enableSensitiveDataLogging)
                    .EnableDetailedErrors();
            }, poolSize);

            return serviceCollection;
        }
    }
}
