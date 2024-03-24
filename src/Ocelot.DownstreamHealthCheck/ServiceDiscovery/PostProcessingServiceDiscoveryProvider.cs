using Ocelot.Configuration;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck.ServiceDiscovery
{
    internal class PostProcessingServiceDiscoveryProvider : IServiceDiscoveryProvider
    {
        private readonly IServiceDiscoveryProvider _serviceDiscoveryProvider;
        private readonly DownstreamRoute _route;
        private readonly IEnumerable<IProcessDiscoveredServices> _postProcessingMethods;

        public PostProcessingServiceDiscoveryProvider(IServiceDiscoveryProvider serviceDiscoveryProvider, DownstreamRoute route, IEnumerable<IProcessDiscoveredServices> postProcessingMethods)
        {
            _serviceDiscoveryProvider = serviceDiscoveryProvider;
            _route = route;
            _postProcessingMethods = postProcessingMethods;
        }

        public Task<List<Service>> GetAsync()
        {
            var services = _serviceDiscoveryProvider.GetAsync();

            foreach (var processingMethod in _postProcessingMethods)
            {
                services = processingMethod.ProcessDiscoveredServices(_route, services);
            }

            return services;
        }
    }
}
