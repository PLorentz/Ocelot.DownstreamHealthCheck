using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.LoadBalancer.LoadBalancers;
using System;

namespace Ocelot.DownstreamHealthCheck
{
    internal class HealthCheckLocator
    {
        public static IServiceHealthTracker HealthTracker;
        public static ILoadBalancerFactory LoadBalancerFactory;
        public static RootOcelotConfig OcelotConfig;

        public HealthCheckLocator(IServiceProvider serviceProvider, IOptions<RootOcelotConfig> ocelotConfig)
        {
            HealthTracker = serviceProvider.GetService<IServiceHealthTracker>();
            LoadBalancerFactory = serviceProvider.GetService<ILoadBalancerFactory>();
            OcelotConfig = ocelotConfig.Value;
        }

    }
}
