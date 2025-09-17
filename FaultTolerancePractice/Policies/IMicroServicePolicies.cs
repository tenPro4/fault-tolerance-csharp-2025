using Polly;

namespace FaultTolerancePractice.Policies
{
    public interface IMicroServicePolicies
    {
        IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
        IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
    }
}
