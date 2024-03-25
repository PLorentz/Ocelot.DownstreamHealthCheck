using Microsoft.Extensions.DependencyInjection;
using Ocelot.LoadBalancer.LoadBalancers;
using System;

namespace Ocelot.DownstreamHealthCheck
{
    internal class HealthCheckLocator
    {
        public static IServiceHealthTracker HealthTracker;
        public static ILoadBalancerFactory LoadBalancerFactory;

        public HealthCheckLocator(IServiceProvider serviceProvider)
        {
            HealthTracker = serviceProvider.GetService<IServiceHealthTracker>();
            LoadBalancerFactory = serviceProvider.GetService<ILoadBalancerFactory>();
        }
    }
}
