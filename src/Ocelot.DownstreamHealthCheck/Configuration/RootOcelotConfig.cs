using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck.Configuration
{
    public class RootOcelotConfig
    {
        public Route[] Routes { get; set; }
        public Globalconfiguration GlobalConfiguration { get; set; }
    }

    public class Globalconfiguration
    {
        public string BaseUrl { get; set; }
    }

    public class Route
    {
        public string DownstreamPathTemplate { get; set; }
        public string DownstreamScheme { get; set; }
        public Downstreamhostandport[] DownstreamHostAndPorts { get; set; }
        public string UpstreamPathTemplate { get; set; }
        public string[] UpstreamHttpMethod { get; set; }
        public Loadbalanceroptions LoadBalancerOptions { get; set; }
    }

    public class Loadbalanceroptions
    {
        public string Type { get; set; }
    }

    public class Downstreamhostandport
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string HealthCheckId { get; set; }
    }
}
