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

        public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            // Get the handler for this request
            var handler = GetHandler<TRequest, TResponse>();

            // Get all pipeline behaviors for this request/response type
            var pipelines = GetPipelines<TRequest, TResponse>().ToArray();

            await ExecutePrePipelines(pipelines, request, cancellationToken);
            var response = await handler.Handle(request, cancellationToken);
            await ExecutePostPipelines(pipelines, request, response, cancellationToken);

            // Execute the pipeline
            return response;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IHandler<TRequest, TResponse> GetHandler<TRequest, TResponse>() where TRequest : IRequest<TResponse>
            => _serviceProvider.GetRequiredService<IHandler<TRequest, TResponse>>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<IPipeline<TRequest, TResponse>> GetPipelines<TRequest, TResponse>() where TRequest : IRequest<TResponse>
            => _serviceProvider.GetServices<IPipeline<TRequest, TResponse>>();

        /** 
         * Pipeline but not run async so there will be some problem
         * I will test and update later
         */
        private Task ExecutePrePipelines<TRequest, TResponse>(
            IPipeline<TRequest, TResponse>[] pipelines,
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
           => Task.WhenAll(pipelines.Select(pipeline => pipeline.Pre(request, cancellationToken)));

        /** 
         * Pipeline but not run async so there will be some problem
         * I will test and update later
         */
        private Task ExecutePostPipelines<TRequest, TResponse>(
            IPipeline<TRequest, TResponse>[] pipelines,
            TRequest request,
            TResponse response,
            CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
            => Task.WhenAll(pipelines.Reverse().Select(pipeline => pipeline.Post(request, response, cancellationToken)));
    }
}
