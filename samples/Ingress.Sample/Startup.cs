// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Core.Configuration.DependencyInjection;

using k8s;
using k8s.Models;


namespace Microsoft.ReverseProxy.Sample
{
    /// <summary>
    /// ASP .NET Core pipeline initialization.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
           

           // builder = services.AddReverseProxy().LoadFromConfig(_configuration.GetSection("ReverseProxy"), reloadOnChange: true);


        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IBackendsRepo backendsRepo,
            IRoutesRepo routesRepo,  IReverseProxyConfigManager proxyManager)
        {
            app.UseHttpsRedirection();

        
            Task.Wait(LoadFromIngress(backendsRepo, routesRepo, proxyManager))
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapReverseProxy(proxyPipeline =>
                {
                    proxyPipeline.UseProxyLoadBalancing();
                    // Customize the request before forwarding
                    proxyPipeline.Use((context, next) =>
                    {
                        var connection = context.Connection;
                        context.Request.Headers.AppendCommaSeparatedValues("X-Forwarded-For",
                            new IPEndPoint(connection.RemoteIpAddress, connection.RemotePort).ToString());
                        return next();
                    });
                });
            });
        }

        private async Task LoadFromIngress(IBackendsRepo backendsRepo,
            IRoutesRepo routesRepo,
            IReverseProxyConfigManager proxyManager)
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var client = new Kubernetes(config); //inject this? 

            await backendsRepo.SetBackendsAsync(config.Backends, CancellationToken.None);
            await routesRepo.SetRoutesAsync(config.Routes, CancellationToken.None);

            var errorReporter = new LoggerConfigErrorReporter(_logger);
            await _proxyManager.ApplyConfigurationsAsync(errorReporter, CancellationToken.None);
        }
    }
}
