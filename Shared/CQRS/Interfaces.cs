namespace CQRS;

//void return 
public struct Unit
{
    public static readonly Unit Value = new Unit();
}
public interface IRequest<Response> { }
public interface IRequest : IRequest<Unit> { }

public interface IHandler<Request, Response> where Request : IRequest<Response>
{
    Task<Response> Handle(Request request, CancellationToken cancellationToken);
}
public interface IHandler<Request> : IHandler<Request, Unit> where Request : IRequest<Unit> { }

public interface IPipeline<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task Pre(TRequest request, CancellationToken cancellationToken = default);
    Task Post(TRequest request, CancellationToken cancellationToken = default);
}

public interface IPublisher
{
    TResponse Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    TResponse Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>;
}