// <snippet_1>
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WebRateLimitAuth.Models;

namespace WebRateLimitAuth;

// <snippet>
public class SampleRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private Func<OnRejectedContext, CancellationToken, ValueTask>? _onRejected;
    private readonly MyRateLimitOptions _options;

    public SampleRateLimiterPolicy(ILogger<SampleRateLimiterPolicy> logger,
                                   IOptions<MyRateLimitOptions> options)
    {
        _onRejected = (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            logger.LogWarning($"Request rejected by {nameof(SampleRateLimiterPolicy)}");
            return ValueTask.CompletedTask;
        };
        _options = options.Value;
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? 
                                                     OnRejected { get => _onRejected; }
    // </snippet>

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartition.GetSlidingWindowLimiter<string>(string.Empty, 
            key => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = _options.permitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _options.queueLimit,
                Window = TimeSpan.FromSeconds(_options.window),
                SegmentsPerWindow = _options.segmentsPerWindow
            });
    
    }
}
// </snippet_1>
