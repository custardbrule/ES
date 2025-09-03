using Quartz;
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
        Task Fire<T>(JobDataMap data, CancellationToken ct) where T : IAppJob;
    }

    public class QuartzJobManager : IQuartzJobManager
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public QuartzJobManager(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public Task Fire<T>(JobDataMap data, CancellationToken ct) where T : IAppJob
        {
            throw new NotImplementedException();
        }
    }
}
