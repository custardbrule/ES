using Data;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Transport;
using KurrentDB.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace App.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureCore(this IServiceCollection services)
        {
            services.AddKurrentDBClient(sp => sp.GetRequiredService<IConfiguration>().GetSection("KurrentDB").Value!);

            services.AddElasticsearchClient();
            services.AddSingleton<IElasticSearchContext, ElasticSearchContext>();

            return services;
        }

        private static IServiceCollection AddElasticsearchClient(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var options = new ElasticSearchContextOptions();
                sp.GetRequiredService<IConfiguration>().Bind("ElasticSearchSettings", options);

                var c = new X509Certificate2(options.CertPath);
                return new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(options.Host))
                                                        .ClientCertificate(c)
                                                        .Authentication(new BasicAuthentication(options.UserName, options.Password))
                                                        .ServerCertificateValidationCallback((sender, cert, chain, error) =>
                                                        {
                                                            return cert.GetCertHashString() == c.GetCertHashString();
                                                        }));
            });

            return services;
        }
    }
}
