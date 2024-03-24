using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DownstreamHealthCheck.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck.PeriodicCheck
{
    internal class HealthCheckWorker : BackgroundService
    {
        private readonly ILogger<HealthCheckWorker> _logger;
        private readonly IServiceHealthTracker _healthTracker;
        private readonly HealthCheckConfig _healthCheckConfig;

        public HealthCheckWorker(ILogger<HealthCheckWorker> logger, IOptions<HealthCheckConfig> healthCheckConfig, IServiceHealthTracker healthTracker)
        {
            _logger = logger;
            _healthTracker = healthTracker;
            _healthCheckConfig = healthCheckConfig.Value;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _healthCheckConfig.PeriodicChecks.Enabled)
            {
                await Parallel.ForEachAsync(_healthCheckConfig.HealthChecks, cancellationToken, async (healthCheck, subCancellationToken) =>
                {
                    using var client = new HttpClient();
                    HttpResponseMessage result;
                    try
                    {
                        _logger.LogInformation("Checking health of " + healthCheck.Id);
                        result = await client.GetAsync(healthCheck.HealthCheckUrl, subCancellationToken);
                        if (result.IsSuccessStatusCode)
                        {
                            _healthTracker.MarkHealthyCheck(healthCheck.Id);
                        }
                        else
                        {
                            _healthTracker.MarkUnhealthyCheck(healthCheck.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _healthTracker.MarkUnhealthyCheck(healthCheck.Id);
                    }
                });

                await Task.Delay(_healthCheckConfig.PeriodicChecks.PeriodInSeconds * 1000, cancellationToken);
            }
        }
    }
}
