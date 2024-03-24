var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

int callCount = 0;

app.MapGet("/weatherforecast", () =>
{
    ++callCount;

    Console.WriteLine("Call number " + callCount);

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            callCount,
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/health", () =>
{
    if (DateTime.Now.Minute % 2 == 0)
    {
        Console.WriteLine("Health checked - throwing exception");
        throw new Exception("I throw during even minutes");
    }
    Console.WriteLine("Health checked - I sent true");
    return true;
})
.WithName("GetHealth")
.WithOpenApi();

app.Run();

internal record WeatherForecast(int CallCount, DateOnly Date, int TemperatureC, string? Summary)
{
    public string Source => "API 2";
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
