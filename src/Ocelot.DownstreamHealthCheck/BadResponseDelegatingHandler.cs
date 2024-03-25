using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.DownstreamHealthCheck.Configuration;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Logging;
using Ocelot.Middleware;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.DownstreamHealthCheck
{
    internal class BadResponseDelegatingHandler : DelegatingHandler
    {
        private readonly DownstreamRoute _route;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IServiceHealthTracker _healthTracker;
        private readonly ILoadBalancerFactory _loadBalancerFactory;
        private readonly RootOcelotConfig _ocelotConfig;
        private readonly IOcelotLogger _logger;

        public BadResponseDelegatingHandler(
            DownstreamRoute route,
            IHttpContextAccessor contextAccessor,
            IOcelotLoggerFactory loggerFactory)
        {
            _route = route;
            _contextAccessor = contextAccessor;
            _healthTracker = HealthCheckLocator.HealthTracker;
            _loadBalancerFactory = HealthCheckLocator.LoadBalancerFactory;
            _ocelotConfig = HealthCheckLocator.OcelotConfig;
            _logger = loggerFactory.CreateLogger<BadResponseDelegatingHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogDebug(() => $"Sending request with downstream URL '{request.RequestUri}'.");
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var routeConfig = _ocelotConfig.Routes.First(route => route.UpstreamPathTemplate == _route.UpstreamPathTemplate.OriginalValue);
                    var statusCode = (int)response.StatusCode;

                    var badResponse = routeConfig.QoSOptions?.BreakIf5XX == true && statusCode >= 500 && statusCode < 600
                                   || routeConfig.QoSOptions?.BreakIf4XX == true && statusCode >= 400 && statusCode < 500;
                    
                    if (badResponse)
                    {
                        return (await MarkAsBadAndTryNextDownstreamService(request, cancellationToken)) ?? response;
                    }
                }

                return response;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is InvalidOperationException)
            {
                var response = await MarkAsBadAndTryNextDownstreamService(request, cancellationToken);
                if (response == null)
                {
                    throw;
                }

                return response;
            }
        }

        private async Task<HttpResponseMessage> MarkAsBadAndTryNextDownstreamService(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _healthTracker.MarkBadResponse(_route, request.RequestUri);

            if (!cancellationToken.IsCancellationRequested)
            {
                var httpContext = _contextAccessor.HttpContext;
                var loadBalancer = _loadBalancerFactory.Get(_route, httpContext.Items.IInternalConfiguration().ServiceProviderConfiguration);
                if (!loadBalancer.IsError)
                {
                    var next = await loadBalancer.Data.Lease(httpContext);
                    if (!next.IsError)
                    {
                        var uriBuilder = new UriBuilder(request.RequestUri);
                        uriBuilder.Host = next.Data.DownstreamHost;
                        uriBuilder.Port = next.Data.DownstreamPort;

                        request.RequestUri = uriBuilder.Uri;
                        return await base.SendAsync(request, cancellationToken);
                    }
                }
            }

            return null;
        }
    }
}
