using Ocelot.Configuration;
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
        Task<List<Service>> ProcessDiscoveredServices(DownstreamRoute route, Task<List<Service>> services);
    }
}
