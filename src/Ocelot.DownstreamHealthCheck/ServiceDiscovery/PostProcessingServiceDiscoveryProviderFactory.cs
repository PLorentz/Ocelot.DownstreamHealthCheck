using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocelot.Configuration;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.Logging;
using Ocelot.Responses;
using Ocelot.ServiceDiscovery;
using Ocelot.ServiceDiscovery.Configuration;
using Ocelot.ServiceDiscovery.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck.ServiceDiscovery
{
    internal class PostProcessingServiceDiscoveryProviderFactory : IServiceDiscoveryProviderFactory
    {
        private readonly IOcelotLogger _logger;
        private readonly IServiceProvider _servicePovider;
        private readonly IEnumerable<IProcessDiscoveredServices> _postProcessingMethods;
        private readonly IOptions<RootOcelotConfig> _config;

        public PostProcessingServiceDiscoveryProviderFactory(IOcelotLoggerFactory factory, IServiceProvider serviceProvider, IEnumerable<IProcessDiscoveredServices> postProcessingMethods, IOptions<RootOcelotConfig> config)
        {
            _logger = factory.CreateLogger<ServiceDiscoveryProviderFactory>();
            _servicePovider = serviceProvider;
            _postProcessingMethods = postProcessingMethods;
            _config = config;
        }

        public Response<IServiceDiscoveryProvider> Get(ServiceProviderConfiguration serviceConfig, DownstreamRoute route)
        {
            var factories = _servicePovider.GetServices<IServiceDiscoveryProviderFactory>().ToArray();
            if (factories.Length < 2)
            {
                var message = $"Unable to find service discovery provider factory!";
                _logger.LogWarning(message);

                return new ErrorResponse<IServiceDiscoveryProvider>(new UnableToFindServiceDiscoveryProviderError(message));
            }

            var factoryToUse = factories[factories.Length - 2];
            return new OkResponse<IServiceDiscoveryProvider>(new PostProcessingServiceDiscoveryProvider(factoryToUse.Get(serviceConfig, route).Data, _postProcessingMethods));
        }
    }
}
