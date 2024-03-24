using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.ServiceDiscovery;

namespace Ocelot.DownstreamHealthCheck
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddDownstreamHealthCheck(this IOcelotBuilder builder)
        {
            builder.Services.AddSingleton<IServiceDiscoveryProviderFactory, PostProcessingServiceDiscoveryProviderFactory>();
            return builder;
        }
    }
}
