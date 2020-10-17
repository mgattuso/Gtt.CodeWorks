using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Middleware;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks
{
    public abstract class BaseServiceInstance<TRequest, TResponse>
        : IServiceInstance<TRequest, TResponse>
        where TRequest : BaseRequest, new() where TResponse : new()
    {
        private readonly IList<IServiceMiddleware> _pipeline = new List<IServiceMiddleware>();

        protected BaseServiceInstance(CoreDependencies coreDependencies)
        {
            _pipeline.Add(new RateLimiterMiddleware(coreDependencies.RateLimiter));
            _pipeline.Add(new TokenizationMiddleware(coreDependencies.Tokenizer, coreDependencies.Environment));
            _pipeline.Add(new LoggingMiddleware(coreDependencies.ServiceLogger));
            _pipeline.Add(new DistributedLockMiddleware<TRequest>(coreDependencies.DistributedLockService, CreateDistributedLockKey));
        }

        private Guid _correlationId;
        private DateTimeOffset _startTime;

        public async Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _startTime = DateTimeOffset.UtcNow;
            _correlationId = request.CorrelationId;
            request.SyncCorrelationIds();
            ServiceResponse<TResponse> response = null;

            // ON REQUEST PIPELINE

            foreach (var middleware in _pipeline)
            {
                try
                {
                   var middlewareResult = await middleware.OnRequest(this, request, cancellationToken);
                   if (middlewareResult != null)
                   {
                       response = new ServiceResponse<TResponse>(default(TResponse), middlewareResult.MetaData);
                       break;
                   }
                }
                catch (Exception ex)
                {
                    if (!middleware.IgnoreExceptions)
                    {
                        return PermanentError(ex);
                    }
                }
            }

            // EXECUTE THE SERVICE
            try
            {
                response ??= await Implementation(request, cancellationToken);
            }
            catch (Exception ex)
            {
                response = PermanentError(ex);
            }

            // ON RESPONSE PIPELINE
            for (var i = _pipeline.Count - 1; i >= 0; i--)
            {
                var middleware = _pipeline[i];
                try
                {
                    await middleware.OnResponse(this, request, response);
                } catch (Exception ex)
                {
                    if (!middleware.IgnoreExceptions)
                    {
                        return PermanentError(ex);
                    }
                }
            }

            return response;
        }

        protected ServiceResponse<TResponse> PermanentError(Exception ex)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    ServiceResult.PermanentError,
                    _correlationId,
                    _startTime,
                    ex.ToString()
                )
            );
        }

#pragma warning disable IDE0060 // Remove unused parameter
        protected ServiceResponse<TResponse> Successful(TResponse response, ServiceResult result = ServiceResult.Successful)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (result.Category() != ResultCategory.Successful)
            {
                throw new Exception("Cannot return a non-successful result through the success response");
            }
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(ServiceResult.Successful, _correlationId, _startTime));
        }
        protected ServiceResponse<TResponse> Created(TResponse response)
        {
            return Successful(response, ServiceResult.Created);
        }
        protected ServiceResponse<TResponse> Queued(TResponse response)
        {
            return Successful(response, ServiceResult.Queued);
        }

        protected abstract Task<ServiceResponse<TResponse>> Implementation(TRequest request, CancellationToken cancellationToken);
        protected abstract Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken);

        public string Name => GetType().Name;
        public DateTimeOffset StartTime => _startTime;
        public Guid CorrelationId => _correlationId;
    }
}