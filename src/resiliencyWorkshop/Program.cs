using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using resiliencyWorkshop.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
var resiliencyOptions = builder.Configuration.GetSection("ResiliencyOptions").Get<ResiliencyOptions>();

builder.Services.AddHttpClient("ExternalServiceClient")
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(
            resiliencyOptions!.RetryCount,
            retryAttempt => TimeSpan.FromSeconds(resiliencyOptions.RetryDelaySeconds),
            (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan} due to: {outcome.Exception?.Message}");
            }))
    .AddTransientHttpErrorPolicy(policy =>
        policy.CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: resiliencyOptions!.CircuitBreakerFailures,
            durationOfBreak: TimeSpan.FromMinutes(resiliencyOptions.CircuitBreakerDurationMinutes),
            onBreak: (outcome, timespan) =>
            {
                Console.WriteLine($"Circuit breaker triggered for {timespan} due to: {outcome.Exception?.Message}");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit breaker reset");
            },
            onHalfOpen: () =>
            {
                Console.WriteLine("Circuit breaker is half-open, allowing test requests");
            }));


var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.MapGet("/v1/resiliency", async Task<IResult> (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("ExternalServiceClient");
        var response = await client.GetAsync("http://localhost:5252/weatherforecast");

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            return TypedResults.Ok(data);
        }

        return TypedResults.Problem(statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Request failed: {ex.Message}");
        return TypedResults.Problem("Unable to fetch data from external service. Please try again later.", statusCode: 503);
    }
});

app.MapGet("/v2/resiliency", async Task<IResult> () =>
{
    try
    {
        AsyncRetryPolicy RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                sleepDurations: Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(resiliencyOptions!.RetryDelaySeconds),
                    retryCount: resiliencyOptions.RetryCount),
                onRetry: static (_, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry count: {retryCount}, Delay: {timeSpan.TotalMilliseconds}ms");
                });

        return await RetryPolicy.ExecuteAsync<IResult>(async () =>
        {
            var response = await new HttpClient().GetAsync("http://localhost:5252/weatherforecast");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return TypedResults.Ok(data);
            }
            return TypedResults.Problem(statusCode: (int)response.StatusCode);
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Request failed: {ex.Message}");
        return TypedResults.Problem("Unable to fetch data from external service. Please try again later.", statusCode: 503);
    }
});

app.Run();