using CQRS;
using Data;
using Infras.User.Services.Jobs;
using Quartz;

namespace Infras.User.Services.Commands.User
{
    public record CreateUserRequest(string Account, string Password, string SecretKey) : IRequest<Guid>;

    public class CreateUserHandler : IHandler<CreateUserRequest, Guid>
    {
        private readonly UserDbContext _context;
        private readonly IQuartzJobManager _quartzJobManager;

        public CreateUserHandler(UserDbContext context, IQuartzJobManager quartzJobManager)
        {
            _context = context;
            _quartzJobManager = quartzJobManager;
        }

        public async Task<Guid> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            // Create user using factory method with password hashing
            var user = Domain.User.UserRoot.User.Create(request.Account, request.Password, request.SecretKey);

            // Add to context
            _context.Users.Add(user);

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            // Trigger sync job to index user data in Elasticsearch
            await _quartzJobManager.Trigger(
                new JobKey(SyncUserJob.KEY, SyncUserJob.GROUP),
                new SyncUserData(user.Id).GetJobDataMap(),
                cancellationToken);

            return user.Id;
        }
    }
}
