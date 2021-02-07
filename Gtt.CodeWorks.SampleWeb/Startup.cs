using System;
using System.Collections.Generic;
using System.Linq;
using Gtt.CodeWorks.AspNet;
using Gtt.CodeWorks.Clients.HttpClient;
using Gtt.CodeWorks.DataAnnotations;
using Gtt.CodeWorks.SampleWeb.Services;
using Gtt.CodeWorks.Serializers.TextJson;
using Gtt.CodeWorks.Tokenizer;
using Gtt.CodeWorks.Validation;
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
            services.AddLogging();

            services.AddTransient<IHttpDataSerializer, HttpJsonDataSerializer>();
            services.AddTransient<HttpRequestConverter>();
            services.AddTransient<ServiceDirectory>();
            services.AddTransient<HttpRequestRunner>();

            services.AddTransient<ILogObjectSerializer, JsonLogObjectSerializer>();
            services.AddTransient<IServiceLogger, ServiceLogger>();
            services.AddTransient<ICodeWorksTokenizer>(cfg => new CodeWorksTokenizer(new NullTokenService()));
            services.AddTransient<IRateLimiter>(cfg => new InMemoryRateLimiter());
            services.AddTransient<IDistributedLockService>(cfg => new InMemoryDistributedLock());
            services.AddTransient<IServiceEnvironmentResolver>(cfg => new NonProductionEnvironmentResolver());
            services.AddTransient<IRequestValidator, DataAnnotationsRequestValidator>();
            services.AddTransient<IHttpSerializerOptionsResolver, DefaultHttpSerializerOptionsResolver>();

            services.AddTransient<CoreDependencies>();

            foreach (var svc in GetConcreteInstancesOf<IServiceInstance>())
            {
                services.AddTransient(svc);
                services.AddTransient(cfg => (IServiceInstance)cfg.GetService(svc));
            }

            services.AddTransient<IServiceResolver>(config =>
                new ServiceResolver(config.GetServices<IServiceInstance>(), new ServiceResolverOptions
                {
                    NamespacePrefixToIgnore = "Gtt.CodeWorks.SampleWeb.Services"
                }));
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
