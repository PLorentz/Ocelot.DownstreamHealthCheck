This project extends Ocelot to not consider downstream services known not to be healthy.
Discovering unhealthy services can happens in two ways, that can be configured and enabled separately in the ocelot.json. All times are configured in milliseconds.

- Periodic health checks:

A BackgroundWorker is used to make the calls for health checks and any "non-success" status code returned from a health check call (4XX, 5XX) is considered a failure.

Create a "DownstreamHealthCheck" section, configuring which URLs to call for health checks and the time between calls. 
Include a "HealthCheckId" in in the "DownstreamHostAndPorts" under "Routes" to associate with a health check URL.

Example:

```
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/weatherforecast",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5288,
          "HealthCheckId": "API_1"
        },
        {
          "Host": "localhost",
          "Port": 5062,
          "HealthCheckId": "API_2"
        }
      ],
      "UpstreamPathTemplate": "/weatherforecast",
      "UpstreamHttpMethod": [ "Get" ],
      "LoadBalancerOptions": {
        "Type": "RoundRobin"
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:7194"
  },
  "DownstreamHealthCheck": {
    "PeriodicChecks": {
      "Enabled": false,
      "Period": 10000
    },
    "HealthChecks": [
      {
        "Id": "API_1",
        "HealthCheckUrl": "http://localhost:5288/health",
        "TimeOut": 1000
      },
      {
        "Id": "API_2",
        "HealthCheckUrl": "http://localhost:5062/health",
        "TimeOut": 2000
      }
    ]
  }
}
```

- Bad response on normal calls:

It extends Ocelot using the "Quality of Service" (QoS) functionality to intercept the bad responses and automatically retry the request in another downstream host/port, allowing for uninterrupted service.

Create a "QoSOptions" section in the route, where the "TimeoutValue" value is mandatory. This "TimeoutValue" controls the overall timeout, so it needs to be sufficient time for both the first request to fail and the second request to be completed for an uninterrupted service, otherwise the called will receive the error (but the downstream will be ignored from being called in subsequent requests anyway).

"DurationOfBreak" controls for how long the downstream endpoint will be ignored (blocked from being called). It is per Route/DownstreamHostAndPorts, so a host:port can be blocked from being used for a route if misbehaving on that path template while still being available to process requests from another path. "DefaultDurationOfBreak" in the "GlobalConfiguration" controls the default value.

Lack of response from the downstream service is always considered a bad response if "QoSOptions" is configured. "BreakIf5XX" and "BreakIf4XX" allows you to control if status code from 500-599 and 400-499 should also be treated as a bad response.
    
Example:

```
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/weatherforecast",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5288
        },
        {
          "Host": "localhost",
          "Port": 5062
        }
      ],
      "UpstreamPathTemplate": "/weatherforecast",
      "UpstreamHttpMethod": [ "Get" ],
      "LoadBalancerOptions": {
        "Type": "RoundRobin"
      },
      "QoSOptions": {
        "TimeoutValue": 100000,
        "DurationOfBreak": 5000,
        "BreakIf5XX": true,
        "BreakIf4XX": true
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:7194",
    "DefaultDurationOfBreak": 10000
  }
}

```
