using SynoAI.Core.Interfaces;

namespace SynoAI.API.EndPoints;

public static class DetectionEndpoints
{
    public static void MapDetectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/detection")
            .WithTags("Detection")
            .WithOpenApi();

        group.MapGet("/{cameraName}", async (string cameraName, IDetectionService detectionService) =>
        {
            await detectionService.RunAsync(cameraName);
            return TypedResults.Ok();
        })
        .WithName("TriggerDetection")
        .WithDescription("Triggers a detection operation for the specified camera by it's name.");
    }
}