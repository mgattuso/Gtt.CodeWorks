using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.Simple.CodeWorks
{
    public abstract class BaseServiceInstance<TRequest, TResponse> : IServiceInstance<TRequest, TResponse>
        where TRequest : BaseRequest, new() where TResponse : class, new()
    {
        public BaseServiceInstance(ILogger logger)
        {
            Logger = logger;
        }

        public string Name => GetType().Name;

        public string FullName => GetType().FullName;

        public Type RequestType => typeof(TRequest);

        public Type ResponseType => typeof(TResponse);

        public UserInformation? User { get; set; }

        private DateTimeOffset _startTime = ServiceClock.CurrentTime();

#if DEBUG
        public CodeWorksEnvironment CurrentEnvironment => CodeWorksEnvironment.NonProduction;
#else
        public CodeWorksEnvironment CurrentEnvironment => CodeWorksEnvironment.Production;
#endif

        private IDictionary<int, string> _errors = new Dictionary<int, string>();
        public IEnumerable<KeyValuePair<int, string>> AllErrorCodes()
        {
            return _errors;
        }

        protected void AddErrorMessage(int code, string message)
        {
            _errors[code] = message;
        }

        public abstract Task<ServiceResponse<TResponse>> Run(TRequest request, CancellationToken cancellationToken);

        public async Task<ServiceResponse> Execute(BaseRequest request, CancellationToken cancellationToken)
        {
            var result = await Execute((TRequest)request, cancellationToken);
            return result;
        }

        public async Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken)
        {
            _startTime = ServiceClock.CurrentTime();
            Logger.LogTrace(new EventId(1, "request"), JsonSerializer.Serialize(request));
            CorrelationId = request.CorrelationId;
            var response = await Run(request, cancellationToken);
            Logger.LogTrace(new EventId(2, "response"), JsonSerializer.Serialize(response));
            return response;
        }

        protected ServiceResponse<TResponse> Successful(TResponse response)
        {
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(this, ServiceResult.Successful, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> Created(TResponse response)
        {
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(this, ServiceResult.Created, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> FulfilledByExistingResource(TResponse response)
        {
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(this, ServiceResult.FulfilledByExistingResource, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> Queued()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.Queued, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> ResourceNotFound()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.ResourceNotFound, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> ValidationErrors(
            IDictionary<int, string>? errorCodes = null, 
            IDictionary<string, string[]>? modelErrors = null, 
            string? publicMessage = null,
            string? exceptionMessage = null)
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(
                this, 
                ServiceResult.ValidationError, 
                Duration, 
                CorrelationId, 
                errorCodes: errorCodes,
                validationErrors: modelErrors,
                publicMessage: publicMessage,
                exceptionMessage: exceptionMessage));
        }

        protected ServiceResponse<TResponse> ValidationErrorFromErrorCodes(IDictionary<int, string> errorCodes)
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.ValidationError, Duration, CorrelationId, errorCodes ));
        }

        protected ServiceResponse<TResponse> ValidationErrorFromModel(IDictionary<string, string[]> modelErrors)
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.ValidationError, Duration, CorrelationId, validationErrors: modelErrors));
        }

        protected ServiceResponse<TResponse> ConflictingRequest()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.ConflictingRequest, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> TransientError(string exception)
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.TransientError, Duration, CorrelationId, exceptionMessage: exception));
        }

        protected ServiceResponse<TResponse> PermanentError(string exception)
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.PermanentError, Duration, CorrelationId, exceptionMessage: exception));
        }

        protected ServiceResponse<TResponse> RateLimited()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.RateLimited, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> UpstreamTimeout()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.UpstreamTimeout, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> UpstreamError()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.UpstreamError, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> NotAuthorized()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.NotAuthorized, Duration, CorrelationId));
        }

        protected ServiceResponse<TResponse> NotAuthenticated()
        {
            return new ServiceResponse<TResponse>(new ResponseMetaData(this, ServiceResult.NotAuthenticated, Duration, CorrelationId));
        }


        private TimeSpan Duration => ServiceClock.CurrentTime() - _startTime;
        private Guid CorrelationId { get; set; }
        public ILogger Logger { get; }
    }
}
