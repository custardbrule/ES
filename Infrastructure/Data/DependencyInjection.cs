using Data;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Transport;
using KurrentDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace App.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddKurrentDBClient(sp => sp.GetRequiredService<IConfiguration>().GetSection("KurrentDB").Value!);
            services.AddElasticsearchCore(configuration);
            services.RegisterQuartz(configuration);

            return services;
        }

        internal static IServiceCollection AddElasticsearchCore(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<ElasticSearchContextOptions>()
                .Configure(options => configuration.GetSection("ElasticSearch").Bind(options));

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ElasticSearchContextOptions>>().Value;

                var c = new X509Certificate2(options.CertPath);
                return new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(options.Host))
                                                        .ClientCertificate(c)
                                                        .Authentication(new BasicAuthentication(options.UserName, options.Password))
                                                        .ServerCertificateValidationCallback((sender, cert, chain, error) =>
                                                        {
                                                            return cert.GetCertHashString() == c.GetCertHashString();
                                                        }));
            });

            services.AddSingleton<IElasticSearchContext, ElasticSearchContext>();

            return services;
        }

        internal static IServiceCollection RegisterQuartz(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(cfg =>
            {
                cfg.UsePersistentStore(o =>
                {
                    o.UseSqlServer(c =>
                    {
                        c.ConnectionString = configuration.GetRequiredSection("Quartz:ConnectionString").Value!;
                        c.TablePrefix = "QRTZ_";
                    });
                    o.UseSystemTextJsonSerializer();
                });

                cfg.UseSimpleTypeLoader();
                cfg.UseDefaultThreadPool(5);
                cfg.SchedulerName = "App_Scheduler";
            });

            services.AddQuartzHostedService(cfg =>
            {
                cfg.WaitForJobsToComplete = true;
                cfg.AwaitApplicationStarted = true;
            });

            services.AddSingleton<IQuartzJobManager, QuartzJobManager>();
            return services;
        }
    }
}
