using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.API.RateLimiting;
using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;

namespace SynoAI.API.EndPoints;

public static class DetectionEndpoints
{
    public static void MapDetectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/detection")
            .WithTags("Detection")
            .WithOpenApi();

        group.MapGet("/{cameraName}", DetectAsync)
        .WithName("TriggerDetection")
        .WithDescription("Triggers a detection operation for the specified camera by it's name.");
    }

    private static async Task<Results<Ok, NotFound, TooManyRequests>> DetectAsync(string cameraName, ICameraService cameraService, IDetectionService detectionService, CameraRateLimiter rateLimiter)
    {
        Camera? camera = await cameraService.GetAsync(cameraName);
        if (camera is null)
        {
            return TypedResults.NotFound();
        }

        if (!rateLimiter.CanMakeRequest(cameraName, camera.Delay))
        {
            return new TooManyRequests();
        }

        await detectionService.RunAsync(cameraName);
        return TypedResults.Ok();
    }
}