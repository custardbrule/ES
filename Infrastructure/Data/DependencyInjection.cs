using System.Reflection;
using Confluent.Kafka;
using Data;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

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

            services
                .AddOptions<ConsumerConfig>()
                .Configure(options => configuration.GetSection("KafkaSettings:ConsumerSettings").Bind(options));

            var types = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface)
                .Select(type => (type, producer: type.GetCustomAttribute<RegisterKafkaProducerAttribute>(true), consumer: type.GetCustomAttribute<RegisterKafkaConsumerAttribute>(true)))
                .Where(x => x.producer != null || x.consumer != null);

            foreach (var (type, producerAttr, consumerAttr) in types)
            {
                if (producerAttr != null)
                {
                    // Keyed registration (for multiple producers of same type)
                    services.TryAdd(producerAttr.GetServiceDescriptor());
                    services.TryAdd(producerAttr.GetProducerServiceDescriptor());
                    // Non-keyed registration (for direct injection)
                    var factoryType = typeof(IProducerFactory<,>).MakeGenericType(producerAttr.KeyType, producerAttr.ValueType);
                    var producerType = typeof(IProducer<,>).MakeGenericType(producerAttr.KeyType, producerAttr.ValueType);
                    services.TryAdd(new ServiceDescriptor(factoryType, producerAttr.ImplementType, producerAttr.Lifetime));
                    services.TryAdd(new ServiceDescriptor(producerType, sp =>
                    {
                        var factory = (dynamic)sp.GetRequiredService(factoryType);
                        return factory.GetProducer();
                    }, producerAttr.Lifetime));
                }

                if (consumerAttr != null)
                {
                    services.AddSingleton(typeof(IHostedService), type);
                }
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

            // not ideal if you use pool
            serviceCollection.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<T>>().CreateDbContext());

            return serviceCollection;
        }
    }
}
