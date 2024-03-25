using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DownstreamHealthCheck.Configuration;
using System;
using System.Net.Http;
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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _healthCheckConfig.PeriodicChecks.Enabled)
            {
                await Task.Delay(_healthCheckConfig.PeriodicChecks.Period, cancellationToken);

                await Parallel.ForEachAsync(_healthCheckConfig.HealthChecks, cancellationToken, async (healthCheck, subCancellationToken) =>
                {
                    using var client = new HttpClient();
                    try
                    {
                        if (healthCheck.TimeOut.HasValue)
                        {
                            client.Timeout = TimeSpan.FromMilliseconds(healthCheck.TimeOut.Value);
                        }

                        _logger.LogInformation("Checking health of " + healthCheck.Id);
                        var result = await client.GetAsync(healthCheck.HealthCheckUrl, subCancellationToken);
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

                        if (ex is HttpRequestException || ex is TaskCanceledException)
                        {
                            _logger.LogWarning($"{ex.GetType()} when checking health of {healthCheck.Id}");
                        }
                        else if (ex is InvalidOperationException)
                        {
                            _logger.LogError($"{ex.GetType()} when checking health of {healthCheck.Id}. Check the Ocelot configuration.");
                        }
                        else
                        {
                            _logger.LogCritical($"Unexpected {ex.GetType()} when checking health of {healthCheck.Id}.");
                            throw;
                        }
                    }
                });
            }
        }
    }
}
