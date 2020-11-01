using System.Threading;
using Gtt.CodeWorks.AspNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Gtt.CodeWorks.SampleWeb
{
    public static class AutoWireServicesExtensions
    {
        public static void UseAutoWiredServicesAsPosts(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseEndpoints(endpoint =>
            {
                endpoint.Map("/{service}/{*action}", async context =>
                {
                    var runner = context.RequestServices.GetService<HttpRequestRunner>();
                    var resolver = context.RequestServices.GetService<IServiceResolver>();
                    var serviceName = context.GetRouteValue("service")?.ToString();
                    IServiceInstance svc;
                    try
                    {
                        svc = resolver.GetInstanceByName(serviceName);
                    }
                    catch (MultipleMatchingServicesException)
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync($"Multiple services found that match {serviceName}");
                        return;
                    }

                    if (svc == null)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync($"No service found called {serviceName}");
                        return;
                    }

                    await runner.Execute(context, svc, CancellationToken.None);

                });
            });
        }
    }
}
