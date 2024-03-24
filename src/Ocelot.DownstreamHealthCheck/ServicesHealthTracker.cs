using Ocelot.DownstreamHealthCheck.ServiceDiscovery;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal class ServicesHealthTracker : IProcessDiscoveredServices, IServiceHealthTracker
    {
        public void MarkHealthyCheck(string healthCheckid)
        {
            
        }

        public void MarkUnhealthyCheck(string healthCheckid)
        {
            
        }

        public Task<List<Service>> ProcessDiscoveredServices(Task<List<Service>> services)
        {
            return services;
        }
    }
}
