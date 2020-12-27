using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients
{
    public interface IClientConverter
    {
        Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, IDictionary<string, object> data, CancellationToken cancellationToken) 
            where TRequest : BaseRequest 
            where TResponse : new();
    }
}