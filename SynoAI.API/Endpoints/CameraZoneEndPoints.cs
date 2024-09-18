using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;

namespace SynoAI.API.Endpoints;

public static class CameraZoneEndPoints
{
    public static void MapCameraZoneEndpoints(this IEndpointRouteBuilder app)
    {
        // Map the group
        var group = app.MapGroup("/cameras/{camera:guid}/zones")
            .WithTags("Camera Zones")
            .WithOpenApi();

        // Fetches all zones for the specified camera
        group.MapGet("/", async (Guid cameraId, ICameraService cameraService) =>
        {
            var zones = await cameraService.GetZonesForCameraAsync(cameraId);
            return TypedResults.Ok(zones);
        })
            .WithName("GetZonesForCamera");

        // Adds a zone to the specified camera
        group.MapPost("/", (Guid cameraId, Zone zone, ICameraService cameraService) =>
        {
            throw new NotImplementedException();
            //await cameraService.AddZoneToCameraAsync(cameraId, zone);
            //return TypedResults.Created($"/cameras/{cameraId}/zones", zone);
        })
            .WithName("AddZoneToCamera");
    }
}
