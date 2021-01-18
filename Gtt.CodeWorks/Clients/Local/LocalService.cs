//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Gtt.CodeWorks.Clients.Local
//{
//    public class LocalClient<T> : ILocalClient<T> where T : IServiceInstance
//    {
//        private readonly T _serviceInstance;

//        public LocalClient(T serviceInstance, IChainedServiceResolver chainedServiceResolver)
//        {
//            _serviceInstance = serviceInstance;
//            chainedServiceResolver.AddChainedService(serviceInstance);
//        }

//        public Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(IServiceInstance source, TRequest request, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : new()
//        {
//            return Call<TRequest, TResponse>(request, new Dictionary<string, object> { { "source", source } }, cancellationToken);
//        }

//        public async Task<ServiceResponse<TResponse>> Call<TRequest, TResponse>(TRequest request, IDictionary<string, object> data, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : new()
//        {
//            var serviceRequest = typeof(TRequest);
//            var serviceResponse = typeof(ServiceResponse<TResponse>);

//            if (_serviceInstance.RequestType != serviceRequest || _serviceInstance.ResponseType != serviceResponse)
//            {
//                throw new Exception("The request or the response type does not match the service being invoked");
//            }

//            var callingService = GetCallingService(data);
//            request.CorrelationId = callingService.CorrelationId;
//            request.SessionId = callingService.SessionId;
//            request.SyncCorrelationIds();
//            ServiceResponse result = await _serviceInstance.Execute(request, ServiceClock.CurrentTime(), cancellationToken);
//            return (ServiceResponse<TResponse>)result;
//        }

//        private IServiceInstance GetCallingService(IDictionary<string, object> data)
//        {
//            if (data == null) throw new Exception("Expected a data dictionary to be passed in");
//            if (data.TryGetValue("source", out object svc))
//            {
//                IServiceInstance serviceInstance = svc as IServiceInstance;
//                if (serviceInstance == null)
//                {
//                    throw new Exception("Expected an entry in the data dictionary with key \"source\" of type IServiceInstance");
//                }

//                return serviceInstance;
//            }

//            throw new Exception("Expected an entry in the data dictionary with key \"source\" of type IServiceInstance");
//        }
//    }
//}
