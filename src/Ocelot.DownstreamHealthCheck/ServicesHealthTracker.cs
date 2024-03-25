using Microsoft.Extensions.Options;
using Ocelot.Configuration;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.DownstreamHealthCheck.ServiceDiscovery;
using Ocelot.Values;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal class ServicesHealthTracker : IProcessDiscoveredServices, IServiceHealthTracker
    {
        private readonly ConcurrentDictionary<string, DateTime> _unhealthyCheckdIds = new();
        private readonly ConcurrentDictionary<string, DateTime> _unhealthyDownstreamIds = new();
        private readonly ILookup<string, string> _servicesByHealthCheckId;
        private readonly Dictionary<string, string> _healthCheckIdByService;

        public ServicesHealthTracker(IOptions<HealthCheckConfig> healthCheckConfig, IOptions<RootOcelotConfig> ocelotConfig)
        {
            _servicesByHealthCheckId = ocelotConfig.Value.Routes
                .SelectMany(route => route.DownstreamHostAndPorts.Select(downstream => new { route, downstream }))
                .ToLookup(route => route.downstream.HealthCheckId, route => GetDownstreamIdentifier(route.route, route.downstream));

            _healthCheckIdByService = ocelotConfig.Value.Routes
                .SelectMany(route => route.DownstreamHostAndPorts.Select(downstream => new { route, downstream }))
                .ToDictionary(route => GetDownstreamIdentifier(route.route, route.downstream), route => route.downstream.HealthCheckId);
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

        public void MarkBadResponse(DownstreamRoute route, Uri requestUri)
        {
            var downstreamIdentifier = GetDownstreamIdentifier(route, requestUri);
            var now = DateTime.UtcNow;
            _unhealthyDownstreamIds.AddOrUpdate(downstreamIdentifier, now, (_, _) => now);
        }

        private void ExpireBadResponse(string downstreamIdentifier, DateTime time)
        {
            _unhealthyDownstreamIds.TryRemove(KeyValuePair.Create(downstreamIdentifier, time));
        }

        public async Task<List<Service>> ProcessDiscoveredServices(DownstreamRoute route, Task<List<Service>> services)
        {
            var servicesList = await services;
            return servicesList.Where(service =>
            {
                var downstreamIdentifier = GetDownstreamIdentifier(route, service.HostAndPort);
                var healthCheckId = _healthCheckIdByService[downstreamIdentifier];

                if (_unhealthyDownstreamIds.TryGetValue(downstreamIdentifier, out DateTime time)){
                    if (DateTime.UtcNow.Subtract(time) < TimeSpan.FromMilliseconds(5000))
                    {
                        return false;
                    }
                    else
                    {
                        ExpireBadResponse(downstreamIdentifier, time);
                    }
                }

                return !_unhealthyCheckdIds.ContainsKey(healthCheckId);
            }).ToList();
        }

        private string GetDownstreamIdentifier(Configuration.Route route, Downstreamhostandport hostAndPort) => $"{route.UpstreamPathTemplate}:{hostAndPort.Host}:{hostAndPort.Port}";
        private string GetDownstreamIdentifier(DownstreamRoute route, ServiceHostAndPort hostAndPort) => $"{route.UpstreamPathTemplate.OriginalValue}:{hostAndPort.DownstreamHost}:{hostAndPort.DownstreamPort}";
        private string GetDownstreamIdentifier(DownstreamRoute route, Uri requestUri) => $"{route.UpstreamPathTemplate.OriginalValue}:{requestUri.Host}:{requestUri.Port}";
    }
}
