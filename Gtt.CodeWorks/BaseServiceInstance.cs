using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks
{
    public abstract class BaseServiceInstance<TRequest, TResponse>
        : IServiceInstance<TRequest, TResponse>
        where TRequest : BaseRequest, new() where TResponse : new()
    {
        private readonly CoreDependencies _coreDependencies;

        protected BaseServiceInstance(CoreDependencies coreDependencies)
        {
            _coreDependencies = coreDependencies;
        }

        private Guid _correlationId;
        private DateTimeOffset _startTime;

        public async Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _correlationId = request.CorrelationId;
            _startTime = DateTimeOffset.UtcNow;

            // TOKENIZE THE REQUEST OBJECT
            await _coreDependencies.Tokenizer.Tokenize(request, _correlationId);

            // LOG THE REQUEST AFTER TOKENIZATION HAS OCCURRED
            await LogRequest(request, cancellationToken);

            // EXECUTE THE SERVICE
            var response = await Implementation(request, cancellationToken);

            // LOG THE RESPONSE BEING RETURNED
            await LogResponse(response, request.CorrelationId, cancellationToken);
            return response;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        protected ServiceResponse<TResponse> Successful(TResponse response, ServiceResult result = ServiceResult.Successful)
#pragma warning restore IDE0060 // Remove unused parameter
        {
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

        public string Name => GetType().Name;

        protected Task LogRequest(TRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _coreDependencies.Logger.LogWarning($"Request: {request.CorrelationId} has been cancelled. Phase:LogRequest");
                return Task.CompletedTask;
            }

            try
            {
                return _coreDependencies.ServiceLogger.LogRequest(request.CorrelationId, Name, request);
            }
            catch (Exception ex)
            {
                _coreDependencies.Logger.LogWarning($"Error logging request {request.CorrelationId}", ex);
                return Task.CompletedTask;
            }
        }

        protected Task LogResponse(ServiceResponse<TResponse> response, Guid correlationId,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _coreDependencies.Logger.LogWarning($"Request: {correlationId} has been cancelled. Phase:LogResponse");
                return Task.CompletedTask;
            }
            try
            {
                return _coreDependencies.ServiceLogger.LogResponse(correlationId, Name, response);
            }
            catch (Exception ex)
            {
                _coreDependencies.Logger.LogWarning($"Error logging request {correlationId}", ex);
                return Task.CompletedTask;
            }
        }
    }
}