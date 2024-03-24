using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal class DownstreamHealthTracker : IProcessDiscoveredServices
    {
        public Task<List<Service>> ProcessDiscoveredServices(Task<List<Service>> services)
        {
            return services;
        }
    }
}
