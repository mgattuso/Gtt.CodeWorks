using System;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Web
{
    public interface IClientConverter
    {
        Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, Uri uri, HttpDataSerializerOptions options = null) 
            where TRequest : BaseRequest 
            where TResponse : new();
    }
}