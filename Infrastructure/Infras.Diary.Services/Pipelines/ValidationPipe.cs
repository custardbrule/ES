using CQRS;
using Microsoft.Extensions.DependencyInjection;
using RequestValidatior;

namespace Infras.Diary.Services.Pipelines;

public class ValidationPipe<TRequest, TResponse> : IPipeline<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationPipe(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task Pre(TRequest request, CancellationToken cancellationToken = default)
    {
        var validator = _serviceProvider.GetService<BaseValidator<TRequest>>();

        if (validator is null) return Task.CompletedTask;

        if (!validator.Validate(request))
            throw new ValidationError([.. validator.Errors]);

        return Task.CompletedTask;
    }

    public Task Post(TRequest request, TResponse response, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
