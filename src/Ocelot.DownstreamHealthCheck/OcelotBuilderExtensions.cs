using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.DownstreamHealthCheck.PeriodicCheck;
using Ocelot.DownstreamHealthCheck.ServiceDiscovery;
using Ocelot.Logging;
using Ocelot.Requester;
using Ocelot.ServiceDiscovery;
using System.Linq;
using System.Net.Http;

namespace Ocelot.DownstreamHealthCheck
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddDownstreamHealthCheck(this IOcelotBuilder builder)
        {
            builder.Services.Configure<RootOcelotConfig>(builder.Configuration);
            builder.Services.Configure<HealthCheckConfig>(builder.Configuration.GetSection("DownstreamHealthCheck"));

            builder.Services
                .AddSingleton<IServiceHealthTracker, ServicesHealthTracker>()
                .AddSingleton<IProcessDiscoveredServices>(services => services.GetServices<IServiceHealthTracker>().OfType<IProcessDiscoveredServices>().Last())
                .AddSingleton<IServiceDiscoveryProviderFactory, PostProcessingServiceDiscoveryProviderFactory>()
                .AddSingleton<HealthCheckLocator>()
                .AddSingleton((QosDelegatingHandlerDelegate)GetDelegatingHandler);

            builder.Services.AddHostedService<HealthCheckWorker>(); 

            return builder;
        }

        internal static DelegatingHandler GetDelegatingHandler(DownstreamRoute route, IHttpContextAccessor contextAccessor, IOcelotLoggerFactory loggerFactory)
        => new BadResponseDelegatingHandler(route, contextAccessor, loggerFactory);
    }
}
