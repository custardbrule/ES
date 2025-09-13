using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IAppJob : IJob
    {
    }

    public interface IQuartzJobManager
    {
        Task<IScheduler> GetScheduler(CancellationToken ct = default);
        Task<JobKey> Fire<T>(JobDataMap data, CancellationToken ct = default) where T : IAppJob;
        Task<JobKey> Fire<T>(CancellationToken ct = default) where T : IAppJob;
        Task<JobKey> Fire<T>(Action<TriggerBuilder> configureTrigger, JobDataMap data, CancellationToken ct = default) where T : IAppJob;
        Task Trigger(JobKey key, CancellationToken ct = default);
        Task Trigger(JobKey key, JobDataMap data, CancellationToken ct = default);
        Task Trigger(string jobName, string group, JobDataMap data, CancellationToken ct = default);
    }

    public class QuartzJobManager : IQuartzJobManager
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public const string FIRE_FORGET = "FIRE_FORGET";
        public const string TRIGGER_PREDEFINE_JOB = "TRIGGER_PREDEFINE_JOB";
        public const string CUSTOM_JOB = "CUSTOM_JOB";

        public QuartzJobManager(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public Task<IScheduler> GetScheduler(CancellationToken ct = default) => _schedulerFactory.GetScheduler(ct);

        /// <summary>
        /// Fire and forget job
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<JobKey> Fire<T>(JobDataMap data, CancellationToken ct = default) where T : IAppJob
        {
            var scheduler = await GetScheduler(ct);

            var job = JobBuilder.Create<T>()
                        .WithIdentity(Guid.NewGuid().ToString(), FIRE_FORGET)
                        .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(GetTriggerKey<T>(), FIRE_FORGET)
                .UsingJobData(data)
                .WithSimpleSchedule()
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(job, trigger, ct);

            return job.Key;
        }

        public Task<JobKey> Fire<T>(CancellationToken ct = default) where T : IAppJob => Fire<T>([], ct);

        /// <summary>
        /// Use for triggering predefine job
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task Trigger(JobKey key, JobDataMap data, CancellationToken ct = default)
        {
            var scheduler = await GetScheduler(ct);
            await scheduler.TriggerJob(key, data, ct);
        }

        public async Task Trigger(JobKey key, CancellationToken ct = default)
        {
            var scheduler = await GetScheduler(ct);
            await scheduler.TriggerJob(key, ct);
        }

        public Task Trigger(string jobName, string group, JobDataMap data, CancellationToken ct = default) => Trigger(new JobKey(jobName, group), data, ct);

        /// <summary>
        /// Schedule customjob
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configureTrigger"> for custom schedule </param>
        /// <param name="data"></param>
        /// <param name="jobName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<JobKey> Fire<T>(Action<TriggerBuilder> configureTrigger, JobDataMap data, CancellationToken ct = default) where T : IAppJob
        {
            var scheduler = await GetScheduler(ct);

            var job = JobBuilder.Create<T>()
                .WithIdentity(Guid.NewGuid().ToString(), CUSTOM_JOB)
                .UsingJobData(data)
                .Build();

            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(GetTriggerKey<T>(), CUSTOM_JOB);

            configureTrigger(triggerBuilder);

            var trigger = triggerBuilder.Build();
            await scheduler.ScheduleJob(job, trigger, ct);

            return job.Key;
        }

        /// <summary>
        /// for delete job and triggers
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task DeleteJob(JobKey key, CancellationToken ct = default)
        {
            var scheduler = await GetScheduler(ct);
            await scheduler.DeleteJob(key, ct);
        }

        #region support method
        private string GetTriggerKey<T>() where T : IAppJob => $"Trigger_{typeof(T).Name}";
        #endregion
    }

    public static class JobDataMapExtension
    {
        public static JobDataMap AddPairs(this JobDataMap data, params KeyValuePair<string, object>[] pairs) => pairs.Aggregate(data, (acc, p) => { acc.Add(p); return acc; });
    }
}
