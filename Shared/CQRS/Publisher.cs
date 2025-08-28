using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS
{
    public class Re : IRequest;
    public class Publisher : IPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public Publisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse> => Send(request, cancellationToken);

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();
            var responseType = typeof(TResponse);

            // Get the handler for this request
            var handler = GetHandler<TResponse>(requestType, responseType);

            // Get all pipeline behaviors for this request/response type
            var pipelines = GetPipelines<TResponse>(requestType, responseType).ToArray();

            await ExecutePrePipelines(pipelines, request, cancellationToken);
            var response = await handler.Handle(request, cancellationToken);
            await ExecutePostPipelines(pipelines, request, response, cancellationToken);

            // Execute the pipeline
            return response;

        }

        private IHandler<IRequest<TResponse>, TResponse> GetHandler<TResponse>(Type requestType, Type responseType)
        {
            var handlerType = typeof(IHandler<,>).MakeGenericType(requestType, responseType);
            var handler = _serviceProvider.GetRequiredService(handlerType);
            return handler as IHandler<IRequest<TResponse>, TResponse>;
        }

        private IEnumerable<IPipeline<IRequest<TResponse>, TResponse>> GetPipelines<TResponse>(Type requestType, Type responseType)
        {
            var behaviorType = typeof(IPipeline<,>).MakeGenericType(requestType, responseType);
            var behaviors = _serviceProvider.GetServices(behaviorType);

            return behaviors.Cast<IPipeline<IRequest<TResponse>, TResponse>>();
        }

        /** 
         * Pipeline but not run async so there will be some problem
         * I will test and update later
         */
        private async Task ExecutePrePipelines<TRequest, TResponse>(
            IPipeline<TRequest, TResponse>[] pipelines,
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
           => await Task.WhenAll(pipelines.Select(pipeline => pipeline.Pre(request, cancellationToken)));

        /** 
         * Pipeline but not run async so there will be some problem
         * I will test and update later
         */
        private async Task ExecutePostPipelines<TRequest, TResponse>(
            IPipeline<TRequest, TResponse>[] pipelines,
            TRequest request,
            TResponse response,
            CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
            => await Task.WhenAll(pipelines.Reverse().Select(pipeline => pipeline.Post(request, response, cancellationToken)));
    }
}
