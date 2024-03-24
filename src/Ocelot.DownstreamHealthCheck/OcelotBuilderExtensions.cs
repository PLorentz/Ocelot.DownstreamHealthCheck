using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.DownstreamHealthCheck.PeriodicCheck;
using Ocelot.DownstreamHealthCheck.ServiceDiscovery;
using Ocelot.ServiceDiscovery;
using System;

namespace Ocelot.DownstreamHealthCheck
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddDownstreamHealthCheck(this IOcelotBuilder builder)
        {
            builder.Services.Configure<RootOcelotConfig>(builder.Configuration);
            builder.Services.Configure<HealthCheckConfig>(builder.Configuration.GetSection("DownstreamHealthCheck"));

            builder.Services
                .AddSingleton<IProcessDiscoveredServices, ServicesHealthTracker>()
                .AddSingleton<IServiceHealthTracker, ServicesHealthTracker>()
                .AddSingleton<IServiceDiscoveryProviderFactory, PostProcessingServiceDiscoveryProviderFactory>();

            builder.Services.AddHostedService<HealthCheckWorker>();

            return builder;
        }
    }
}
