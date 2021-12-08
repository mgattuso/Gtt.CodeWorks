using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Clients.HttpRequest;
using Gtt.CodeWorks.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Gtt.CodeWorks.Functions.Host
{
    public abstract class ServiceCallFunctionBase
    {
        private readonly HttpRequestMessageRunner _runner;
        private readonly IServiceResolver _serviceResolver;
        private readonly IHttpDataSerializer _serializer;
        private readonly ISerializationSchema _serializationSchema;
        private readonly IChainedServiceResolver _chainedServiceResolver;
        private readonly TelemetryClient _telemetryClient;
        private readonly IStateDiagram _stateDiagram;

        protected ServiceCallFunctionBase(HttpRequestMessageRunner runner,
            IServiceResolver serviceResolver,
            IHttpDataSerializer serializer,
            ISerializationSchema serializationSchema,
            IChainedServiceResolver chainedServiceResolver,
            IStateDiagram stateDiagram,
            TelemetryClient telemetryClient)
        {
            _runner = runner;
            _serviceResolver = serviceResolver;
            _serializer = serializer;
            _serializationSchema = serializationSchema;
            _chainedServiceResolver = chainedServiceResolver;
            _telemetryClient = telemetryClient;
            _stateDiagram = stateDiagram;
        }

        public virtual Task<HttpResponseMessage> Execute(
            HttpRequestMessage request,
            string action,
            string service,
            CancellationToken cancellationToken)
        {
            switch (action)
            {
                case "call":
                    if (request.Method == HttpMethod.Post)
                    {
                        return CallAction(request, service, cancellationToken);
                    }
                    else
                    {
                        var msg = new HttpResponseMessage(HttpStatusCode.NotFound)
                        {
                            Content = new StringContent("Use POST method to call service")
                        };
                        return Task.FromResult(msg);
                    }
                case "meta:state:diagram":
                    return StateDiagram(request, service, cancellationToken);
                case "meta:state:diagram:view":
                    return StateDiagramView(request, service, cancellationToken);
                case "meta:errors":
                    return ErrorsAction(service, cancellationToken);
                case "meta:request:schema":
                    return SchemaActions(service, "request", "schema");
                case "meta:request:example":
                    return SchemaActions(service, "request", "example");
                case "meta:response:schema":
                    return SchemaActions(service, "response", "schema");
                case "meta:response:example":
                    return SchemaActions(service, "response", "example");
            }

            throw new NotImplementedException();
        }

        private async Task<HttpResponseMessage> StateDiagram(HttpRequestMessage request, string service, CancellationToken cancellationToken)
        {
            IServiceInstance serviceInstance = _serviceResolver.GetInstanceByName(service);
            var d = await _stateDiagram.GetDiagram(serviceInstance);

            var msg = new HttpResponseMessage(HttpStatusCode.OK);
            if (string.IsNullOrWhiteSpace(d.contentType))
            {
                msg.Content = new StringContent(d.diagram, Encoding.UTF8);
            }
            else
            {
                msg.Content = new StringContent(d.diagram, Encoding.UTF8, d.contentType);
            }
            return msg;
        }

        private async Task<HttpResponseMessage> StateDiagramView(HttpRequestMessage request, string service, CancellationToken cancellationToken)
        {
            IServiceInstance serviceInstance = _serviceResolver.GetInstanceByName(service);
            var d = await _stateDiagram.GetDiagram(serviceInstance);

            var diagram = d.diagram.Replace("rankdir=\"LR\"", "rankdir=\"TB\"");

            var encoder = UrlEncoder.Default;
            var url = "https://dreampuf.github.io/GraphvizOnline/#" + encoder.Encode(diagram);

            var msg = new HttpResponseMessage(HttpStatusCode.Redirect);
            msg.Headers.Add("location", url);

            return msg;
        }

        private async Task<HttpResponseMessage> SchemaActions(string service, string model, string format)
        {
            var dict = new Dictionary<string, string> { ["Service"] = service };

            try
            {
                var serviceInstance = _serviceResolver.GetInstanceByName(service);
                if (serviceInstance == null)
                {
                    dict["Found"] = "false";
                    _telemetryClient.TrackTrace("ListErrors", SeverityLevel.Information, dict);
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent($"No service found called {service}")
                    };
                }

                string json;

                switch (model)
                {
                    case "request":
                        switch (format)
                        {
                            case "schema":
                                json = await _serializationSchema.SerializeSchema(serviceInstance.RequestType);
                                break;
                            case "example":
                                json = await _serializationSchema.SerializeExample(serviceInstance.RequestType);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(format));
                        }
                        break;
                    case "response":
                        switch (format)
                        {
                            case "schema":
                                json = await _serializationSchema.SerializeSchema(serviceInstance.ResponseType);
                                break;
                            case "example":
                                json = await _serializationSchema.SerializeExample(serviceInstance.ResponseType);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(format));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format));
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, _serializer.Encoding, _serializer.ContentType)
                };
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private async Task<HttpResponseMessage> ErrorsAction(string service, CancellationToken cancellationToken)
        {
            var dict = new Dictionary<string, string> { ["Service"] = service };

            var serviceInstance = _serviceResolver.GetInstanceByName(service);
            if (serviceInstance == null)
            {
                dict["Found"] = "false";
                _telemetryClient.TrackTrace("ListErrors", SeverityLevel.Information, dict);
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent($"No service found called {service}")
                };
            }

            HashSet<ErrorCodeData> errorsCodes = new HashSet<ErrorCodeData>();
            foreach (var v in serviceInstance.AllErrorCodes())
            {
                errorsCodes.Add(new ErrorCodeData(v.Key, v.Value, serviceInstance.Name));
            }

            foreach (var instance in _chainedServiceResolver.AllInstances())
            {
                foreach (var v in instance.AllErrorCodes())
                {
                    errorsCodes.Add(new ErrorCodeData(v.Key, v.Value, instance.Name));
                }
            }

            var payload = await _serializationSchema.SerializeErrorReport(errorsCodes.OrderBy(x => x.Service).ThenBy(x => x.ErrorCode).ToList());
            dict["Found"] = "true";
            dict["ServiceInstance"] = serviceInstance.Name;
            _telemetryClient.TrackTrace("ListErrors", SeverityLevel.Information, dict);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, _serializer.Encoding, _serializer.ContentType)
            };
        }

        protected async Task<HttpResponseMessage> CallAction(
            HttpRequestMessage request,
            string service,
            CancellationToken cancellationToken)
        {
            var dict = new Dictionary<string, string> { ["Service"] = service };
            var serviceInstance = _serviceResolver.GetInstanceByName(service);
            if (serviceInstance == null)
            {
                dict["Found"] = "false";
                _telemetryClient.TrackTrace("ServiceCall", SeverityLevel.Information, dict);
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent($"No service found called {service}")
                };
            }
            dict["Found"] = "true";
            dict["ServiceInstance"] = serviceInstance.Name;
            _telemetryClient.TrackTrace("ServiceCall", SeverityLevel.Information, dict);
            var result = await _runner.Handle(request, serviceInstance, cancellationToken);
            return result;
        }
    }
}
