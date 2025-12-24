using CQRS;
using Data;
using Infras.User.Services.Commands;
using Quartz;

namespace Infras.User.Services.Jobs
{
    [DisallowConcurrentExecution]
    public class LogLoginHistoryJob : IAppJob
    {
        public const string UserId = nameof(UserId);
        public const string IpAddress = nameof(IpAddress);
        public const string UserAgent = nameof(UserAgent);

        private readonly IPublisher _publisher;

        public LogLoginHistoryJob(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;

            var userId = Guid.Parse(dataMap.GetString(UserId)!);
            var ipAddress = dataMap.GetString(IpAddress)!;
            var userAgent = dataMap.GetString(UserAgent)!;

            await _publisher.Send(new LogLoginHistoryCommand(userId, ipAddress, userAgent));
        }
    }
}
