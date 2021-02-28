using System;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients.HttpClient;
using Microsoft.Extensions.Logging;

namespace Gtt.CodeWorks.StateMachines.Clients.HttpClient
{
    public abstract class CodeWorksStatefulClientBase : CodeWorksClientBase
    {
        protected CodeWorksStatefulClientBase(
            CodeWorksClientEndpoint endpoint, 
            System.Net.Http.HttpClient client, 
            IHttpDataSerializer httpDataSerializer,
            IHttpSerializerOptionsResolver optionsResolver,
            ILoggerFactory loggerFactory) : base(endpoint, client, httpDataSerializer, optionsResolver, loggerFactory)
        {
        }

        protected Task<ServiceResponse<TResponse>> StatefulCall<TRequest, TResponse, TState, TTrigger, TData>(string serviceRoute, TRequest request, CancellationToken cancellationToken) 
            where TRequest : BaseStatefulRequest<TTrigger>
            where TResponse : BaseStatefulResponse<TState,TTrigger, TData>, new()
            where TTrigger : struct, IConvertible
            where TState : struct, IConvertible
            where TData : BaseStateDataModel<TState>, new()
        {
            return Call<TRequest, TResponse>(serviceRoute, request, cancellationToken);
        }
    }
}