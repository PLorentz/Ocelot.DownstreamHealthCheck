using Microsoft.Extensions.Options;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.DownstreamHealthCheck.ServiceDiscovery;
using Ocelot.Values;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal class ServicesHealthTracker : IProcessDiscoveredServices, IServiceHealthTracker
    {
        private readonly ConcurrentDictionary<string, DateTime> _unhealthyCheckdIds = new();

        public ServicesHealthTracker(IOptions<HealthCheckConfig> healthCheckConfig, IOptions<RootOcelotConfig> ocelotConfig)
        {

        }

        public void MarkHealthyCheck(string healthCheckId)
        {
            _unhealthyCheckdIds.TryRemove(healthCheckId, out _);
        }

        public void MarkUnhealthyCheck(string healthCheckId)
        {
            var now = DateTime.UtcNow;
            _unhealthyCheckdIds.AddOrUpdate(healthCheckId, now, (_, _) => now);
        }

        public Task<List<Service>> ProcessDiscoveredServices(Task<List<Service>> services)
        {
            return services;
        }
    }
}
