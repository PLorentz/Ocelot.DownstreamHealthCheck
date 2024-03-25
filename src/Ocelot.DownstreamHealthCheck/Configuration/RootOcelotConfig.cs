namespace Ocelot.DownstreamHealthCheck.Configuration
{
    public class RootOcelotConfig
    {
        public Route[] Routes { get; set; }
        public Globalconfiguration GlobalConfiguration { get; set; }
    }

    public class Globalconfiguration
    {
        public int? DefaultDurationOfBreak { get; set; }
    }

    public class Route
    {
        public string DownstreamScheme { get; set; }
        public Downstreamhostandport[] DownstreamHostAndPorts { get; set; }
        public string UpstreamPathTemplate { get; set; }
    }

    public class Downstreamhostandport
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string HealthCheckId { get; set; }
    }
}
