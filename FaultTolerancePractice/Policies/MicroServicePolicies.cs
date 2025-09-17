using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Text;
using System.Text.Json;

namespace FaultTolerancePractice.Policies
{
    public class MicroServicePolicies:IMicroServicePolicies
    {
        private readonly ILogger<MicroServicePolicies> _logger;

        public MicroServicePolicies(ILogger<MicroServicePolicies> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
                    });

            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .CircuitBreakerAsync(
             handledEventsAllowedBeforeBreaking: 3,
             durationOfBreak: TimeSpan.FromMinutes(2), // Waiting time to be in "Half Open" state
             onBreak: (outcome, timespan) =>
             {
                 _logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
             },
             onReset: () => {
                 _logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed.");
             });

            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));

            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
        {
            AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
              maxParallelization: 2, //Allows up to 2 concurrent requests
              maxQueuingActions: 40, //Queue up to 40 additional requests
              onBulkheadRejectedAsync: (context) => {
                  _logger.LogWarning("BulkheadIsolation triggered. Can't send any more requests since the queue is full");

                  throw new BulkheadRejectedException("Bulkhead queue is full");
              }
              );

            return policy;
        }


        public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
        {
            AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
              .FallbackAsync(async (context) =>
              {
                  _logger.LogWarning("Fallback triggered: The request failed, returning dummy data");

                  var fakeResponse = new
                  {
                      message = "Temporarily Unavailable (fallback)"
                  };

                  var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                  {
                      Content = new StringContent(JsonSerializer.Serialize(fakeResponse), Encoding.UTF8, "application/json")
                  };

                  return response;
              });

            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = GetRetryPolicy();
            var circuitBreakerPolicy = GetCircuitBreakerPolicy();
            var timeoutPolicy = GetTimeoutPolicy();
            var fallbackPolicy = GetFallbackPolicy();
            var bulkPolicy = GetBulkheadIsolationPolicy();

            AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
            return wrappedPolicy;
        }
    }
}
