using App.Extensions.DependencyInjection;
using Infras.Services.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Infras.Services
{
    public static class Config
    {
        public static IServiceCollection ConfigInfras(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEventStore();
            services.AddElasticsearchCore(configuration);
            services.AddCQRS(ServiceLifetime.Transient, Assembly.GetExecutingAssembly());
            services.RegisterQuartz(configuration);
            services.RegisterKafkaServices(configuration, Assembly.GetExecutingAssembly());

            services.AddHostedService<RegisterJobHostService>();
            return services;
        }
    }
}
