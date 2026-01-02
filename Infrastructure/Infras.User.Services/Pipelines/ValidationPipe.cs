using CQRS;
using Microsoft.Extensions.DependencyInjection;
using RequestValidatior;

namespace Infras.User.Services.Pipelines;

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
        // Try to get validator for this request type
        var validator = _serviceProvider.GetService<BaseValidator<TRequest>>();

        if (validator is null)
        {
            // No validator registered, skip validation
            return Task.CompletedTask;
        }

        // Validate the request
        if (!validator.Validate(request))
        {
            // Throw ValidationError directly (it extends Exception)
            throw new ValidationError([.. validator.Errors]);
        }

        return Task.CompletedTask;
    }

    public Task Post(TRequest request, TResponse response, CancellationToken cancellationToken = default)
    {
        // No post-validation needed
        return Task.CompletedTask;
    }
}
