using System.Collections.Concurrent;

namespace SynoAI.API.RateLimiting;

public class CameraRateLimiter
{
    private readonly ILogger<CameraRateLimiter>? _logger;
    private readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

    public CameraRateLimiter(ILogger<CameraRateLimiter> logger)
    {
        _logger = logger;
    }

    public bool CanMakeRequest(string cameraName, int delay)
    {
        var now = DateTime.UtcNow;
        var lastRequestTime = _lastRequestTimes.GetOrAdd(cameraName, now);

        if (now - lastRequestTime < new TimeSpan(0, 0, 0, 0, delay))
        {
            _logger?.LogInformation("Request for camera {cameraName} rejected: too frequent", cameraName);
            return false;
        }

        _lastRequestTimes[cameraName] = now;
        return true;
    }
}