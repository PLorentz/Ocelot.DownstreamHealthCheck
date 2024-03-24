using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal class PostProcessingServiceDiscoveryProvider : IServiceDiscoveryProvider
    {
        private readonly IServiceDiscoveryProvider _serviceDiscoveryProvider;
        private readonly Func<Task<List<Service>>, Task<List<Service>>> _postProcessingFunction;

        public PostProcessingServiceDiscoveryProvider(IServiceDiscoveryProvider serviceDiscoveryProvider, Func<Task<List<Service>>, Task<List<Service>>> postProcessingFunction)
        {
            _serviceDiscoveryProvider = serviceDiscoveryProvider;
            _postProcessingFunction = postProcessingFunction;
        }

        public Task<List<Service>> GetAsync()
        {
            var services = _serviceDiscoveryProvider.GetAsync();
            return _postProcessingFunction?.Invoke(services) ?? services;
        }
    }
}
