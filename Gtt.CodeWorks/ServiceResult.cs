using System;

namespace Gtt.CodeWorks
{
    /// <summary>
    /// Service results
    /// </summary>
    public enum ServiceResult
    {
        // SUCCESSFUL STATUS

        /// <summary>
        /// Successful service result
        /// </summary>
        Successful,
        /// <summary>
        /// Successful service result and resource was created 
        /// </summary>
        Created,

        /// <summary>
        /// Service result was successful due to existing resource that met the criteria
        /// </summary>
        FulfilledByExistingResource,

        // QUEUED STATUS

        /// <summary>
        /// Resource is queued up for for further processing
        /// </summary>
        Queued,

        // BUSINESS / LOGIC ERRORS

        /// <summary>
        /// A resource expected by the request was not found
        /// </summary>
        ResourceNotFound,

        /// <summary>
        /// The service request resulted in a validation error due to the request details or
        /// due to 
        /// </summary>
        ValidationError,

        /// <summary>
        /// The service request was not fulfilled as it conflicts with the
        /// existing state of the service
        /// </summary>
        ConflictingRequest,
        
        /// <summary>
        /// The request resulted in an error in the service that is expected to be
        /// transient in nature. The called should retry the request using the same parameters.
        /// </summary>
        TransientError,

        /// <summary>
        /// The request resulted in a permanent error that is not expected to be recoverable in the present
        /// state. A retry need not be attempted as subsequent attempts are not expected to result in a successful
        /// outcome. 
        /// </summary>
        PermanentError,

        /// <summary>
        /// The request was denied due to a rate limiting event. The called should try again.
        /// </summary>
        RateLimited,

        /// <summary>
        /// The request timed out on an upstream dependent service. The current state of the upstream service
        /// is unknown. The called should retry the request paying attention to any side-effects of the unknown status.
        /// </summary>
        UpstreamTimeout,

        /// <summary>
        /// The request resulted in an error in an upstream dependent service. The caller should review
        /// the errors being returned to determine the root cause.
        /// </summary>
        UpstreamError

    }
}
