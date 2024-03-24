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
        private readonly IEnumerable<IProcessDiscoveredServices> _postProcessingMethods;

        public PostProcessingServiceDiscoveryProvider(IServiceDiscoveryProvider serviceDiscoveryProvider, IEnumerable<IProcessDiscoveredServices> postProcessingMethods)
        {
            _serviceDiscoveryProvider = serviceDiscoveryProvider;
            _postProcessingMethods = postProcessingMethods;
        }

        public Task<List<Service>> GetAsync()
        {
            var services = _serviceDiscoveryProvider.GetAsync();

            foreach (var processingMethod in _postProcessingMethods)
            {
                services = processingMethod.ProcessDiscoveredServices(services);
            }

            return services;
        }
    }
}
