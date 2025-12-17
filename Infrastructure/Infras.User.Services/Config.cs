using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CQRS;
using App.Extensions.DependencyInjection;
using System.Reflection;
using Infras.User.Services.Pipelines;

namespace Infras.User.Services
{
    public static class Config
    {
        public static IServiceCollection ConfigInfras(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterDbContextPool<UserDbContext>(configuration.GetConnectionString("UserConnection")!);
            services.AddElasticsearchCore(configuration);
            services.AddCQRS(ServiceLifetime.Transient, Assembly.GetExecutingAssembly());
            services.AddScoped(typeof(IPipeline<,>), typeof(LogPipe<,>));
            services.RegisterQuartz(configuration);
            services.RegisterKafkaServices(configuration, Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
