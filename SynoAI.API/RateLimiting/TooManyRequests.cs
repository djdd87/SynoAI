namespace SynoAI.API.RateLimiting;

public class TooManyRequests : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return Task.CompletedTask;
    }
}