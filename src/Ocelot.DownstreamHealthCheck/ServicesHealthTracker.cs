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
        private readonly Dictionary<string, string> _healthCheckIdByService;
        private readonly TimeSpan _defaultServiceTimeout;

        public ServicesHealthTracker(IOptions<HealthCheckConfig> healthCheckConfig, IOptions<RootOcelotConfig> ocelotConfig)
        {
            _defaultServiceTimeout = TimeSpan.FromMilliseconds(ocelotConfig.Value.GlobalConfiguration?.DefaultDurationOfBreak ?? 5 * 60 * 1000); // 5 minutes.

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
                return !CheckBlockedByBadResponse(route, downstreamIdentifier) && !CheckBlockedByHealthCheck(downstreamIdentifier);
            }).ToList();
        }

        private bool CheckBlockedByHealthCheck(string downstreamIdentifier)
        {
            var healthCheckId = _healthCheckIdByService[downstreamIdentifier];
            return _unhealthyCheckdIds.ContainsKey(healthCheckId);
        }

        private bool CheckBlockedByBadResponse(DownstreamRoute route, string downstreamIdentifier)
        {
            if (_unhealthyDownstreamIds.TryGetValue(downstreamIdentifier, out DateTime time))
            {
                var routeTimeOut = route?.QosOptions?.DurationOfBreak;
                if (DateTime.UtcNow.Subtract(time) < (routeTimeOut.HasValue ? TimeSpan.FromMilliseconds(routeTimeOut.Value) : _defaultServiceTimeout))
                {
                    return true;
                }
                else
                {
                    ExpireBadResponse(downstreamIdentifier, time);
                }
            }

            return false;
        }

        private string GetDownstreamIdentifier(Configuration.Route route, Downstreamhostandport hostAndPort) => $"{route.UpstreamPathTemplate}:{hostAndPort.Host}:{hostAndPort.Port}";
        private string GetDownstreamIdentifier(DownstreamRoute route, ServiceHostAndPort hostAndPort) => $"{route.UpstreamPathTemplate.OriginalValue}:{hostAndPort.DownstreamHost}:{hostAndPort.DownstreamPort}";
        private string GetDownstreamIdentifier(DownstreamRoute route, Uri requestUri) => $"{route.UpstreamPathTemplate.OriginalValue}:{requestUri.Host}:{requestUri.Port}";
    }
}
