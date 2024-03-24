using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal interface IServiceHealthTracker
    {
        void MarkHealthyCheck(string healthCheckId);
        void MarkUnhealthyCheck(string healthCheckId);
    }
}
