using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck.ServiceDiscovery
{
    internal interface IProcessDiscoveredServices
    {
        Task<List<Service>> ProcessDiscoveredServices(Task<List<Service>> services);
    }
}
