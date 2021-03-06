﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Middleware;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks
{
    public abstract class BaseServiceInstance<TRequest, TResponse>
        : IServiceInstance<TRequest, TResponse>,
            IAuthenticatedServiceInstance,
            ILocalServiceInstance,
            ILocalServiceInstance<TRequest, TResponse>
        where TRequest : BaseRequest, new() where TResponse : new()
    {
        private readonly IList<IServiceMiddleware> _pipeline = new List<IServiceMiddleware>();
        private readonly ILogger _logger;
        private readonly IDictionary<int, string> _acceptableErrors = new Dictionary<int, string>();
        private readonly IChainedServiceResolver _chainedServiceResolver;

        protected BaseServiceInstance(CoreDependencies coreDependencies)
        {
            CurrentEnvironment = coreDependencies.EnvironmentResolver.Environment;
            _logger = coreDependencies.LoggerFactory.CreateLogger(GetType());
            _pipeline.Add(new RateLimiterMiddleware(coreDependencies.RateLimiter));
            _pipeline.Add(new TokenizationMiddleware(coreDependencies.Tokenizer, coreDependencies.EnvironmentResolver));
            _pipeline.Add(new LoggingMiddleware(coreDependencies.ServiceLogger));
            _pipeline.Add(new AuthenticationMiddleware(coreDependencies.UserResolver, coreDependencies.LoggerFactory.CreateLogger<AuthenticationMiddleware>()));
            _pipeline.Add(new DistributedLockMiddleware<TRequest>(coreDependencies.DistributedLockService, coreDependencies.LoggerFactory.CreateLogger(typeof(DistributedLockMiddleware<TRequest>)), CreateDistributedLockKey));
            _pipeline.Add(new ValidationMiddleware(coreDependencies.RequestValidator));

            _chainedServiceResolver = coreDependencies.ChainedServiceResolver;
            _chainedServiceResolver?.AddChainedService(this);
        }

        private Guid _correlationId;
        private Guid? _sessionId;
        private int? _serviceHop;

        public virtual Task<ServiceResponse<TResponse>> ExecuteTakeCached(
            TRequest request, 
            CancellationToken cancellationToken)
        {
            var cached = GetCachedResponse<TRequest, TResponse>();
            if (cached != null)
            {
                _logger.LogDebug($"Found cached version for {cached.MetaData.ServiceName} {_correlationId}");
                return Task.FromResult(cached);
            }
            return Execute(request, cancellationToken);
        }

        public Task<ServiceResponse<TResponse>> ExecuteTakeCached(
            TRequest request, 
            Func<TRequest, bool> predicate, 
            CancellationToken cancellationToken)
        {
            var cached = GetCachedResponse<TRequest, TResponse>(predicate);
            if (cached != null)
            {
                _logger.LogDebug($"Found cached version matching predicate for {cached.MetaData.ServiceName} {_correlationId}");
                return Task.FromResult(cached);
            }
            return Execute(request, cancellationToken);
        }

        public async Task<ServiceResponse> Execute(BaseRequest request, CancellationToken cancellationToken)
        {
            var r = await this.Execute((TRequest) request, cancellationToken);
            return r;
        }

        public virtual async Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            StartTime = ServiceClock.CurrentTime();
            _chainedServiceResolver?.AddCallToChain(this);
            _correlationId = CalculateCorrelationId(request);
            _sessionId = request.SessionId;
            _serviceHop = CalculateServiceHop();
            request.CorrelationId = _correlationId;
            request.SyncCorrelationIds();
            ServiceResponse<TResponse> response = null;

            // ON REQUEST PIPELINE

            _logger.LogTrace($"Start request pipeline for {request.CorrelationId}");
            foreach (var middleware in _pipeline)
            {
                try
                {
                    if (_serviceHop > 0 &&
                        middleware.SkipOnInternalCall)
                    {
                        continue;
                    }
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
                        var errorResponse = PermanentError(ex);
                        StoredResponse = errorResponse;
                        return errorResponse;
                    }
                }
            }
            _logger.LogTrace($"End request pipeline for {request.CorrelationId}");

            // EXECUTE THE SERVICE
            _logger.LogTrace($"Start execute for {request.CorrelationId}");
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    if (response == null)
                    {
                        response = await BeforeImplementation(request, cancellationToken);
                    }
                }
                else
                {
                    response = TemporaryException("Cancellation Request:BeforeImplementation");
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    if (response == null)
                    {
                        response = await Implementation(request, cancellationToken);
                    }
                }
                else
                {
                    response = TemporaryException("Cancellation Requested:Implementation");
                }
            }
            catch (NotImplementedException ex)
            {
                _logger.LogInformation(ex, $"Call path is not implemented: {request.CorrelationId}");
                response = new ServiceResponse<TResponse>(
                    default(TResponse),
                    new ResponseMetaData(this, ServiceResult.NotImplemented));
            }
            catch (BusinessLogicException ex)
            {
                response = new ServiceResponse<TResponse>(
                    default(TResponse),
                    new ResponseMetaData(this, ex.Result, publicMessage: ex.PublicMessage, exceptionMessage: ex.ToString()));
            }
            catch (ValidationErrorException ex)
            {
                response = ValidationError(ex.Error, ex.Member);
            }
            catch (Exception ex)
            {
                response = PermanentError(ex);
            }

            _logger.LogTrace($"End execute for {request.CorrelationId}");


            _logger.LogTrace($"Start response pipeline for {request.CorrelationId}");
            // ON RESPONSE PIPELINE
            for (var i = _pipeline.Count - 1; i >= 0; i--)
            {
                var middleware = _pipeline[i];

                if (_serviceHop > 0 && middleware.SkipOnInternalCall)
                {
                    continue;
                }

                try
                {
                    await middleware.OnResponse(this, request, response, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (!middleware.IgnoreExceptions)
                    {
                        var errorResponse = PermanentError(ex);
                        StoredResponse = errorResponse;
                        return errorResponse;
                    }
                }
            }
            _logger.LogTrace($"End response pipeline for {request.CorrelationId}");

            await BeforeResponse(response, cancellationToken);
            StoredResponse = response;
            return response;
        }

        protected virtual Task BeforeResponse(ServiceResponse<TResponse> response, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected ServiceResponse<TResponse> TemporaryException(string reason)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.TransientError,
                    publicMessage: reason
                ));
        }

        protected virtual Task<ServiceResponse<TResponse>> BeforeImplementation(TRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult((ServiceResponse<TResponse>)null);
        }

        protected ErrorCodeData GetErrorData(int code)
        {
            var codes = DefineErrorCodes() ?? _acceptableErrors ?? new Dictionary<int, string>();
            if (codes.TryGetValue(code, out var msg))
            {
                return new ErrorCodeData(code, msg, FullName);
            }

            return null;
        }

        protected ServiceResponse<TResponse> ErrorCode(int code)
        {
            var error = GetErrorData(code);

            if (error != null)
            {

                return new ServiceResponse<TResponse>(default(TResponse),
                    new ResponseMetaData(
                        this,
                        ServiceResult.ValidationError,
                        error.ToDictionary()
                    )
                );
            }

            _logger.LogWarning($"No error code defined for value {code}");

            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.PermanentError,
                    publicMessage: $"An unexpected error code {code} was returned"
                ));
        }

        protected ServiceResponse<TResponse> TemporaryException(Exception ex)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.TransientError,
                    exceptionMessage: ex.ToString()
                ));
        }

        protected ServiceResponse<TResponse> PermanentError(Exception ex)
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.PermanentError,
                    exceptionMessage: ex.ToString()
                )
            );
        }

        protected ServiceResponse<TResponse> NotFound()
        {
            return new ServiceResponse<TResponse>(default(TResponse),
                new ResponseMetaData(
                    this,
                    ServiceResult.ResourceNotFound)
                );
        }

        protected Task<string> NoDistributedLock()
        {
            return Task.FromResult(string.Empty);
        }

        protected IDictionary<int, string> AddErrorCode(int code, string description)
        {
            return AddErrorCodes((code, description));
        }

        protected IDictionary<int, string> AddErrorCodes(params (int code, string description)[] errors)
        {
            foreach (var error in errors)
            {
                _acceptableErrors[error.code] = error.description;
            }

            return _acceptableErrors;
        }

        protected IDictionary<int, string> NoErrorCodes()
        {
            _acceptableErrors.Clear();
            return _acceptableErrors;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        protected ServiceResponse<TResponse> Successful(TResponse response, ServiceResult result = ServiceResult.Successful, Dictionary<string, ResponseMetaData> dependencyMetaData = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (result.Category() != ResultCategory.Successful)
            {
                throw new Exception("Cannot return a non-successful result through the success response");
            }
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(this, ServiceResult.Successful, dependencies: dependencyMetaData));
        }

        protected Task<ServiceResponse<TResponse>> SuccessfulTask(TResponse response, ServiceResult result = ServiceResult.Successful, Dictionary<string, ResponseMetaData> dependencyMetaData = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (result.Category() != ResultCategory.Successful)
            {
                throw new Exception("Cannot return a non-successful result through the success response");
            }
            return Task.FromResult(
                new ServiceResponse<TResponse>(
                    response,
                    new ResponseMetaData(this, ServiceResult.Successful, dependencies: dependencyMetaData))
                );
        }

        protected ServiceResponse<TResponse> Created(TResponse response)
        {
            return Successful(response, ServiceResult.Created);
        }
        protected ServiceResponse<TResponse> Queued(TResponse response)
        {
            return Successful(response, ServiceResult.Queued);
        }

        protected ServiceResponse<TResponse> ValidationError(string error, string members)
        {
            return ValidationError((error, new[] { members }));
        }

        protected ServiceResponse<TResponse> ValidationError(string error, string[] members)
        {
            return ValidationError((error, members));
        }

        protected ServiceResponse<TResponse> ValidationError(params (string error, string[] members)[] validationErrors)
        {
            Dictionary<string, string[]> validationDictionary = validationErrors.ToDictionary(k => k.error, v => v.members);
            var inverted = validationDictionary.FlipKeysAndValues();
            return new ServiceResponse<TResponse>(default(TResponse), new ResponseMetaData(this, ServiceResult.ValidationError, validationErrors: inverted));
        }

        private Guid CalculateCorrelationId(TRequest request)
        {
            var instances = _chainedServiceResolver?.AllInstances();
            if (instances != null)
            {
                Guid[] entries = instances.Select(x => x.CorrelationId).ToArray();
                var nonDefault = entries.Where(x => x != Guid.Empty);
                var guids = nonDefault as Guid[] ?? nonDefault.ToArray();
                if (guids.Any()) return guids.First();
            }

            if (request.CorrelationId != default(Guid))
            {
                return request.CorrelationId;
            }

            return Guid.NewGuid();
        }

        private int CalculateServiceHop()
        {
            int? maxHop = _chainedServiceResolver?.AllInstances()
                .Max(x => x.ServiceHop);

            if (maxHop != null) return maxHop.Value + 1;
            return 0;
        }

        protected abstract Task<ServiceResponse<TResponse>> Implementation(TRequest request, CancellationToken cancellationToken);
        protected abstract Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken);
        protected abstract IDictionary<int, string> DefineErrorCodes();
        public CodeWorksEnvironment CurrentEnvironment { get; }
        public virtual IAccessPolicy AccessPolicy => new AllowAnonymousAccessPolicy();
        protected ILogger Logger => _logger;
        public string Name => GetType().Name;
        public string FullName => GetType().FullName?.Replace("+", ".") ?? GetType().Name;
        public DateTimeOffset StartTime { get; private set; }
        public Guid CorrelationId => _correlationId;
        public Guid? SessionId => _sessionId;
        public int? ServiceHop => _serviceHop;

        public IEnumerable<KeyValuePair<int, string>> AllErrorCodes() => DefineErrorCodes() ?? new Dictionary<int, string>();
        public UserInformation User { get; set; }

        public Type RequestType => typeof(TRequest);
        public Type ResponseType => typeof(ServiceResponse<TResponse>);

        protected IList<IServiceMiddleware> PipeLine => _pipeline;

        protected BaseRequest StoredRequest { get; private set; }
        BaseRequest ILocalServiceInstance.StoredRequest => StoredRequest;
        protected ServiceResponse<TResponse> StoredResponse { get; private set; }
        ServiceResponse ILocalServiceInstance.StoredResponse => StoredResponse;

        protected ServiceResponse<TOtherResponse> GetCachedResponse<TOtherRequest, TOtherResponse>(Func<TOtherRequest, bool> predicate = null)
            where TOtherRequest : BaseRequest, new()
            where TOtherResponse : new()
        {
            var matches = _chainedServiceResolver.GetCurrentCallChain()
                .OfType<ILocalServiceInstance>()
                .Where(x => x.StoredResponse != null && x.ResponseType == typeof(ServiceResponse<TOtherResponse>));

            if (predicate != null)
            {
                matches = matches.Where(x =>
                {
                    var req = x.StoredRequest as TOtherRequest;
                    if (req == null) return false;
                    return predicate(req);
                });
            }

            var lastMatch = matches.LastOrDefault();
            return lastMatch?.StoredResponse as ServiceResponse<TOtherResponse>;
        }
    }
}