using Polly;
using Polly.Wrap;
using System;

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

        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = GetRetryPolicy();

            AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy);
            return wrappedPolicy;
        }
    }
}
