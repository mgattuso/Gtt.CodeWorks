using System;
using System.Collections.Generic;
using System.Linq;
using Gtt.CodeWorks.AspNet;
using Gtt.CodeWorks.SampleWeb.Services;
using Gtt.CodeWorks.Serializers.TextJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gtt.CodeWorks.SampleWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHttpDataSerializer, HttpJsonDataSerializer>();
            services.AddTransient<HttpRequestConverter>();
            services.AddTransient<ServiceDirectory>();
            services.AddTransient<HttpRequestRunner>();
            services.AddTransient(x => new CoreDependencies());

            foreach (var svc in GetConcreteInstancesOf<IServiceInstance>())
            {
                services.AddTransient(svc);
                services.AddTransient(cfg => (IServiceInstance)cfg.GetService(svc));
            }

            services.AddTransient<IServiceResolver>(config =>
                new ServiceResolver(config.GetServices<IServiceInstance>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAutoWiredServicesAsPosts();
        }

        private IEnumerable<Type> GetConcreteInstancesOf<T>()
        {
            var a = AppDomain.CurrentDomain.GetAssemblies();
            var cls = a.SelectMany(x => x.GetTypes()).Where(p => typeof(T).IsAssignableFrom(p) && !p.IsAbstract);
            return cls;
        }
    }
}
