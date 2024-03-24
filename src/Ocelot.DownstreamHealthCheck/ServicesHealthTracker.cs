using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ocelot.Configuration;
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
        private readonly ILookup<string, string> _servicesByHealthCheckId;
        private readonly Dictionary<string, string> _healthCheckIdByService;

        public ServicesHealthTracker(IOptions<HealthCheckConfig> healthCheckConfig, IOptions<RootOcelotConfig> ocelotConfig)
        {
            _servicesByHealthCheckId = ocelotConfig.Value.Routes
                .SelectMany(route => route.DownstreamHostAndPorts.Select(downstream => new { route, downstream }))
                .ToLookup(route => route.downstream.HealthCheckId, route => GetIdentifier(route.route, route.downstream));

            _healthCheckIdByService = ocelotConfig.Value.Routes
                .SelectMany(route => route.DownstreamHostAndPorts.Select(downstream => new { route, downstream }))
                .ToDictionary(route => GetIdentifier(route.route, route.downstream), route => route.downstream.HealthCheckId);
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

        public async Task<List<Service>> ProcessDiscoveredServices(DownstreamRoute route, Task<List<Service>> services)
        {
            var servicesList = await services;
            return servicesList.Where(service =>
            {
                var identifier = GetIdentifier(route, service.HostAndPort);
                var healthCheckId = _healthCheckIdByService[identifier];
                return !_unhealthyCheckdIds.ContainsKey(healthCheckId);
            }).ToList();
        }

        private string GetIdentifier(Configuration.Route route, Downstreamhostandport hostAndPort) => $"{route.UpstreamPathTemplate}:{hostAndPort.Host}:{hostAndPort.Port}";
        private string GetIdentifier(DownstreamRoute route, ServiceHostAndPort hostAndPort) => $"{route.UpstreamPathTemplate.OriginalValue}:{hostAndPort.DownstreamHost}:{hostAndPort.DownstreamPort}";
    }
}
