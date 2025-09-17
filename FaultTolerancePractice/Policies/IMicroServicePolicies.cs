using Polly;

namespace FaultTolerancePractice.Policies
{
    public interface IMicroServicePolicies
    {
        IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
        IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
        IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy();
        IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
        IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy();
        IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
    }
}
