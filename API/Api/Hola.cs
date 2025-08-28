using CQRS;

namespace Api
{
    public class Hola
    {
    }

    public class Req : IRequest { }
    public class ReqHandler : IHandler<Req, Unit>
    {
        public Task<Unit> Handle(Req request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Unit.Value);
        }
    }
}
