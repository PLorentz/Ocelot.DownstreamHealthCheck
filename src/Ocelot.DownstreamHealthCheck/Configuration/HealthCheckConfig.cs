using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int PeriodInSeconds { get; set; }
    }

    public class HealthCheck
    {
        public string Id { get; set; }
        public string HealthCheckUrl { get; set; }
        public int? TimeOutInMilliseconds { get; set; }
    }
}
