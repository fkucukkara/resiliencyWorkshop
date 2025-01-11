namespace resiliencyWorkshop.Options;

public class ResiliencyOptions
{
    public int RetryCount { get; set; }
    public int RetryDelaySeconds { get; set; }
    public int CircuitBreakerFailures { get; set; }
    public int CircuitBreakerDurationMinutes { get; set; }
}
