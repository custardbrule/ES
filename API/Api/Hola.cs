using CQRS;
using Data;
using Quartz;

namespace Api
{
    public class Req : IRequest { }
    public class ReqHandler : IHandler<Req, Unit>
    {
        public Task<Unit> Handle(Req request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Unit.Value);
        }
    }

    public class Req2 : IRequest<int> { }
    public class Req2Handler : IHandler<Req2, int>
    {
        public Task<int> Handle(Req2 request, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class TJob : IAppJob
    {
        private readonly IPublisher _publisher;

        public TJob(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _publisher.Send(new Req());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
