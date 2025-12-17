using Data;
using Domain.User.UserRoot;
using Infras.User.Services.Constants;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Infras.User.Services.Jobs
{
    public record SyncUserData(Guid UserId)
    {
        public JobDataMap GetJobDataMap() => new JobDataMap { { nameof(UserId), UserId } };
    }

    public class SyncUserJob : IAppJob
    {
        public const string KEY = nameof(SyncUserJob);
        public const string GROUP = JobConstants.SYNC_DATA_GROUP;

        private readonly UserDbContext _context;
        private readonly IElasticSearchContext _elasticsearchContext;

        public SyncUserJob(UserDbContext context, IElasticSearchContext elasticsearchContext)
        {
            _context = context;
            _elasticsearchContext = elasticsearchContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!context.MergedJobDataMap.TryGetGuid(nameof(SyncUserData.UserId), out var userId)) return;

            var ct = CancellationToken.None;

            // Load user with all related data
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.Scopes)
                .Include(u => u.LoginHistories)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null) return;

            // Index user data to Elasticsearch
            var res = await _elasticsearchContext.IndexAsync(UserConstants.ESIndex, UserConstants.GetId(user.Id), user);
        }
    }
}
