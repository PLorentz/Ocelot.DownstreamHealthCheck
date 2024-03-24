using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.DownstreamHealthCheck;

var builder = new WebHostBuilder();
builder .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config
                .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddJsonFile("ocelot.json", false, true)
                .AddEnvironmentVariables();
        })
        .ConfigureServices(s =>
        {
            s.AddOcelot().AddDownstreamHealthCheck();
        })
        .ConfigureLogging((hostingContext, logging) =>
        {
            //add your logging
        })
        .UseIISIntegration()
        .Configure(app =>
        {
            app.UseOcelot().Wait();
        });


var app = builder.Build();

app.Run();
