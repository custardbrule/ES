using CQRS;
using Microsoft.Extensions.Logging;
using Utilities;

namespace Infras.Diary.Services.Pipelines
{
    public class LogPipe<TRequest, TResponse> : IPipeline<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<IHandler<TRequest, TResponse>> _logger;
        private Guid _id;

        public LogPipe(ILogger<IHandler<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public Task Pre(TRequest request, CancellationToken cancellationToken = default)
        {
            _id = Guid.NewGuid();
            _logger.LogInformation("{id} - {name} at {date} : {value} ", _id.Serialize(), typeof(TRequest).Name, DateTimeOffset.UtcNow.ToString("dd-MMMM-yyyy HH:mm:ss"), request.Serialize());
            return Task.CompletedTask;
        }

        public Task Post(TRequest request, TResponse response, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("{id} - {name} at {date} : {value} ", _id.Serialize(), typeof(TResponse).Name, DateTimeOffset.UtcNow.ToString("dd-MMMM-yyyy HH:mm:ss"), response is null ? "{}" : response.Serialize());
            return Task.CompletedTask;
        }
    }
}
