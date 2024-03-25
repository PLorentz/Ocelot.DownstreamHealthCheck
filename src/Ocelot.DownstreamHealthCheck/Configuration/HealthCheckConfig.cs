namespace Ocelot.DownstreamHealthCheck.Configuration
{
    internal class HealthCheckConfig
    {
        public PeriodicChecks PeriodicChecks { get; set; }
        public HealthCheck[] HealthChecks { get; set; }
    }

    public class PeriodicChecks
    {
        public bool Enabled { get; set; }
        public int Period { get; set; }
    }

    public class HealthCheck
    {
        public string Id { get; set; }
        public string HealthCheckUrl { get; set; }
        public int? TimeOut { get; set; }
    }
}
