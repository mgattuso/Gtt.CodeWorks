using System.ComponentModel;
using System.Net;

namespace Gtt.CodeWorks
{
    /// <summary>
    /// Service results
    /// </summary>
    public enum ServiceResult
    {
        // FAILED STATUS
        
        /// <summary>
        /// The request resulted in a permanent error that is not expected to be recoverable in the present
        /// state. A retry need not be attempted as subsequent attempts are not expected to result in a successful
        /// outcome. 
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.SystemError,
            httpStatusCode: (int)HttpStatusCode.InternalServerError)]
        [Description("Permanent Error")]
        PermanentError = 0,

        /// <summary>
        /// The request resulted in an error in the service that is expected to be
        /// transient in nature. The called should retry the request using the same parameters.
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.SystemError,
            httpStatusCode: (int)HttpStatusCode.ServiceUnavailable)]
        [Description("Transient Error")]
        TransientError = 1,


        /// <summary>
        /// The request was denied due to a rate limiting event. The called should try again.
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.SystemError,
            httpStatusCode: (int)HttpStatusCode.TooManyRequests)]
        [Description("Rate Limited")]
        RateLimited = 2,

        /// <summary>
        /// The request timed out on an upstream dependent service. The current state of the upstream service
        /// is unknown. The called should retry the request paying attention to any side-effects of the unknown status.
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.DependencyError,
            httpStatusCode: (int)HttpStatusCode.GatewayTimeout)]
        [Description("Upstream Timeout")]
        UpstreamTimeout = 3,

        /// <summary>
        /// The request resulted in an error in an upstream dependent service. The caller should review
        /// the errors being returned to determine the root cause.
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.DependencyError,
            httpStatusCode: (int)HttpStatusCode.BadGateway)]
        [Description("Upstream Error")]
        UpstreamError = 4,

        // BUSINESS / LOGIC ERRORS

        /// <summary>
        /// A resource expected by the request was not found
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.BusinessLogicError,
            httpStatusCode: (int)HttpStatusCode.NotFound)]
        [Description("Resource Not Found")]
        ResourceNotFound = 5,

        /// <summary>
        /// The service request resulted in a validation error due to the request details or
        /// due to 
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.BusinessLogicError,
            httpStatusCode: (int)HttpStatusCode.BadRequest)]
        [Description("Validation Error")]
        ValidationError = 6,

        /// <summary>
        /// The service request was not fulfilled as it conflicts with the
        /// existing state of the service
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Failed,
            ResultCategory.BusinessLogicError,
            httpStatusCode: (int)HttpStatusCode.Conflict)]
        [Description("Conflicting Request")]
        ConflictingRequest = 7,


        // SUCCESSFUL STATUS

        /// <summary>
        /// Successful service result
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Successful, 
            ResultCategory.Successful, 
            httpStatusCode: (int)HttpStatusCode.OK)]
        Successful = 8,

        /// <summary>
        /// Successful service result and resource was created 
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Successful, 
            ResultCategory.Successful, 
            httpStatusCode: (int)HttpStatusCode.Created)]
        Created = 9,

        /// <summary>
        /// Service result was successful due to existing resource that met the criteria
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Successful, 
            ResultCategory.Successful, 
            httpStatusCode: (int)HttpStatusCode.OK)]
        [Description("Fulfilled by Existing Resource")]
        FulfilledByExistingResource = 10,

        // QUEUED STATUS

        /// <summary>
        /// Resource is queued up for for further processing
        /// </summary>
        [ServiceResultMetadata(
            ResultOutcome.Successful, 
            ResultCategory.Successful, 
            httpStatusCode: (int)HttpStatusCode.Accepted)]
        Queued = 11
    }
}
