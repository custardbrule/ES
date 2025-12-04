using Data;
using Microsoft.Extensions.Hosting;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infras.Diary.Services.Jobs
{
    public class RegisterJobHostService : IHostedService
    {
        private readonly IQuartzJobManager _quartzJobManager;

        public RegisterJobHostService(IQuartzJobManager quartzJobManager)
        {
            _quartzJobManager = quartzJobManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var scheduler = await _quartzJobManager.GetScheduler(cancellationToken);

            await scheduler.AddJob(JobBuilder.Create<SyncDailyDiaryJob>().WithIdentity(SyncDailyDiaryJob.KEY, SyncDailyDiaryJob.GROUP).StoreDurably().WithDescription("Sync Daily Diary").DisallowConcurrentExecution().Build(), true, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
