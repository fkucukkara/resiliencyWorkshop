# Resiliency Workshop: Using Polly for Resilient HTTP Requests

This application demonstrates how to implement resilient HTTP requests using Polly, focusing on retry and circuit breaker policies. These strategies help ensure that your application gracefully handles transient faults and prevents cascading failures.

---

## Features

1. **Retry Policy**
   - Retries HTTP requests a specified number of times with a delay between attempts.
   - Logs retry attempts, including the reason for each retry.

2. **Circuit Breaker Policy**
   - Prevents repeated requests to a failing service by temporarily "breaking" the circuit after a threshold of failures.
   - Logs circuit state changes (open, half-open, reset).

3. **Structured Configuration**
   - Resiliency options (e.g., retry count, delay, circuit breaker settings) are configurable via `appsettings.json`.

4. **Error Handling**
   - Handles HTTP response status codes and unexpected exceptions gracefully.

---

## Prerequisites

- .NET 9
- Polly NuGet packages:
  ```bash
  dotnet add package Polly
  dotnet add package Polly.Contrib.WaitAndRetry
  ```

---

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/fkucukkara/resiliencyWorkshop.git
   cd resiliencyWorkshop
   ```

2. Install dependencies:
   ```bash
   dotnet restore
   ```

3. Update the configuration in `appsettings.json`:
   ```json
   {
     "ResiliencyOptions": {
       "RetryCount": 5,
       "RetryDelaySeconds": 2,
       "CircuitBreakerFailures": 3,
       "CircuitBreakerDurationMinutes": 1
     }
   }
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

---

## Example Endpoint

### Endpoint: `/v1/resiliency`

This endpoint demonstrates a resilient HTTP request to an external service. It uses Polly's retry and circuit breaker policies to handle transient errors and prevent cascading failures.

#### Request
```http
GET /v1/resiliency HTTP/1.1
Host: localhost:5000
```

#### Response
- **200 OK**: If the external service responds successfully.
- **Problem Details**: If there is an error, returns a structured problem response with details.

---

## Resiliency Options

These settings can be modified in `appsettings.json`:

- `RetryCount`: Number of retry attempts.
- `RetryDelaySeconds`: Delay between retry attempts (in seconds).
- `CircuitBreakerFailures`: Number of consecutive failures before opening the circuit.
- `CircuitBreakerDurationMinutes`: Duration the circuit remains open before transitioning to half-open.

---

## Logs and Debugging

The application logs details of retries and circuit breaker state changes to the console:

- **Retry Logs**:
  ```
  Retry {retryCount} after {timespan} due to: {exceptionMessage}
  ```

- **Circuit Breaker Logs**:
  - On Break: 
    ```
    Circuit breaker triggered for {timespan} due to: {exceptionMessage}
    ```
  - On Reset:
    ```
    Circuit breaker reset
    ```
  - On Half-Open:
    ```
    Circuit breaker is half-open, allowing test requests
    ```

---

## Additional Resources

- [Polly Documentation](https://github.com/App-vNext/Polly)
- [.NET HTTP Client Factory](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
- [Polly.Contrib.WaitAndRetry](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry)

---

## License
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

This project is licensed under the MIT License, which allows you to freely use, modify, and distribute the code. See the [`LICENSE`](LICENSE) file for full details.