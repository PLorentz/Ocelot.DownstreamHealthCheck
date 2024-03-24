using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;

namespace Ocelot.DownstreamHealthCheck
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddDownstreamHealthCheck(this IOcelotBuilder builder)
        {
            return builder;
        }
    }
}
