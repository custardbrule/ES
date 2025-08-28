using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS
{
    public class Re:IRequest;
    public class Publisher : IPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        public TResponse Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private IHandler<TRequest, TResponse> GetHanlder<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse>
        {
            var requestType = request.GetType();
            var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler is null)
                throw new InvalidOperationException($"Handler not found for request type {requestType.Name}");

            return handler as IHandler<TRequest, TResponse>;
        }
    }
}
