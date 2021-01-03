using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Middleware;
using Gtt.CodeWorks.Validation;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks
{
    public abstract class BaseServiceInstance<TRequest, TResponse>
        : IServiceInstance<TRequest, TResponse>
        where TRequest : BaseRequest, new() where TResponse : new()
    {
        private readonly IList<IServiceMiddleware> _pipeline = new List<IServiceMiddleware>();
        private readonly ILogger _logger;

        protected BaseServiceInstance(CoreDependencies coreDependencies)
        {
            _pipeline.Add(new RateLimiterMiddleware(coreDependencies.RateLimiter));
            _pipeline.Add(new TokenizationMiddleware(coreDependencies.Tokenizer, coreDependencies.EnvironmentResolver));
            _pipeline.Add(new LoggingMiddleware(coreDependencies.ServiceLogger));
            _pipeline.Add(new DistributedLockMiddleware<TRequest>(coreDependencies.DistributedLockService, CreateDistributedLockKey));
            _pipeline.Add(new ValidationMiddleware(coreDependencies.RequestValidator));
            _logger = coreDependencies.LoggerFactory.CreateLogger(GetType());
        }

        private Guid _correlationId;
        private Guid? _sessionId;
        private int? _serviceHop;


        public async Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            StartTime = startTime;
            _correlationId = request.CorrelationId;
            _sessionId = request.SessionId;
            _serviceHop = request.ServiceHop;
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
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (response == null)
                    {
                        response = await Implementation(request, cancellationToken);
                    }
                }
                else
                {
                    response = TemporaryException("Cancellation Requested");
                }
            }
            catch (ValidationErrorException ex)
            {
                response = ValidationError(new ValidationErrorData(ex.Error.ErrorMessage, ex.Error.Members.ToArray()));
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
                    await middleware.OnResponse(this, request, response, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!middleware.IgnoreExceptions)
                    {
                        return PermanentError(ex);
                    }
                }
            }

            return response;
        }

        protected ServiceResponse<TResponse> TemporaryException(string reason)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.TransientError,
                    new ErrorData()
                ));
        }

        protected ServiceResponse<TResponse> ErrorCode(int code)
        {
            if (DefineErrorCodes().TryGetValue(code, out var msg))
            {
                return new ServiceResponse<TResponse>(default(TResponse),
                    new ResponseMetaData(
                        this,
                        ServiceResult.PermanentError,
                        new ErrorData(msg, code.ToString())
                    )
                );
            }

            _logger.LogWarning($"No error code defined for value {code}");

            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.PermanentError,
                    new ErrorData($"An unexpected error code {code} was returned", "")
                ));
        }

        protected ServiceResponse<TResponse> TemporaryException(Exception ex)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.TransientError,
                    new ErrorData()
                ));
        }

        protected ServiceResponse<TResponse> PermanentError(Exception ex)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.PermanentError,
                    new ErrorData(ex)
                )
            );
        }

        protected Task<string> NoDistributedLock()
        {
            return Task.FromResult(string.Empty);
        }

        protected Dictionary<int, string> NoErrorCodes()
        {
            return new Dictionary<int, string>();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        protected ServiceResponse<TResponse> Successful(TResponse response, ServiceResult result = ServiceResult.Successful, Dictionary<string, ResponseMetaData> dependencyMetaData = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (result.Category() != ResultCategory.Successful)
            {
                throw new Exception("Cannot return a non-successful result through the success response");
            }
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(this, ServiceResult.Successful, dependencyMetaData));
        }
        protected ServiceResponse<TResponse> Created(TResponse response)
        {
            return Successful(response, ServiceResult.Created);
        }
        protected ServiceResponse<TResponse> Queued(TResponse response)
        {
            return Successful(response, ServiceResult.Queued);
        }

        protected ServiceResponse<TResponse> ValidationError(ValidationErrorData error)
        {
            var ver = new ValidationErrorResponse();
            ver.AddValidationError(error);
            return new ServiceResponse<TResponse>(default(TResponse), new ResponseMetaData(this, ver));
        }

        protected ServiceResponse<TResponse> ValidationError(params ValidationErrorData[] validationErrors)
        {
            var ver = new ValidationErrorResponse();
            foreach (var error in validationErrors)
            {
                ver.AddValidationError(error);
            }
            return new ServiceResponse<TResponse>(default(TResponse), new ResponseMetaData(this, ver));
        }

        protected abstract Task<ServiceResponse<TResponse>> Implementation(TRequest request, CancellationToken cancellationToken);
        protected abstract Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken);
        protected abstract IDictionary<int, string> DefineErrorCodes();

        public string Name => GetType().Name;
        public string FullName => GetType().FullName?.Replace("+", ".") ?? GetType().Name;
        public DateTimeOffset StartTime { get; private set; }
        public Guid CorrelationId => _correlationId;
        public Guid? SessionId => _sessionId;
        public int? ServiceHop => _serviceHop;

        public IEnumerable<KeyValuePair<int, string>> AllErrorCodes() => DefineErrorCodes() ?? new Dictionary<int, string>();

        public abstract ServiceAction Action { get; }
        public async Task<ServiceResponse> Execute(BaseRequest request, DateTimeOffset startTime, CancellationToken cancellationToken)
        {
            var result = await Execute((TRequest)request, startTime, cancellationToken);
            return result;
        }

        public Type RequestType => typeof(TRequest);
        public Type ResponseType => typeof(ServiceResponse<TResponse>);
    }
}