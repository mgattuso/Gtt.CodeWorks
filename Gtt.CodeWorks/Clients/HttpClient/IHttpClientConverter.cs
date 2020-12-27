using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Clients.HttpClient
{
    public interface IHttpClientConverter : IClientConverter
    {
        Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, Uri url, CancellationToken cancellationToken)
            where TRequest : BaseRequest
            where TResponse : new();
    }
}
