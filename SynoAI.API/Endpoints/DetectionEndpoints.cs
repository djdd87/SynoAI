namespace SynoAI.API.EndPoints;

public static class DetectionEndpoints
{
    public static void MapDetectionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/detection", HandleAsync)
        .WithName("TriggerDetection")
        .WithOpenApi();
    }

    public static async Task<IResult> HandleAsync()
    {
        throw new NotImplementedException();
    }
}