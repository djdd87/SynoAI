using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.API.Models.Data;

namespace SynoAI.Api.Endpoints;

public static class ZoneEndpoints
{
    public static void MapZoneEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/cameras/{cameraId:guid}/zones", async (Guid cameraId, ICameraService cameraService) =>
        {
            var zones = await cameraService.GetZonesForCameraAsync(cameraId);
            return TypedResults.Ok(zones);
        })
        .WithName("GetZonesForCamera")
        .WithOpenApi();

        app.MapPost("/cameras/{cameraId:guid}/zones", async (Guid cameraId, Zone zone, ICameraService cameraService) =>
        {
            await cameraService.AddZoneToCameraAsync(cameraId, zone);
            return TypedResults.Created($"/cameras/{cameraId}/zones/{zone.Id}", zone);
        })
        .WithName("AddZoneToCamera")
        .WithOpenApi();

        app.MapGet("/zones/{zoneId:guid}", async Task<Results<Ok<Zone>, NotFound>> (Guid zoneId, ICameraService cameraService) =>
        {
            var zone = await cameraService.GetZoneByIdAsync(zoneId);
            if (zone == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(zone);
        })
        .WithName("GetZoneById")
        .WithOpenApi();

        app.MapDelete("/zones/{zoneId:guid}", async Task<Results<NoContent, NotFound>> (Guid zoneId, ICameraService cameraService) =>
        {
            var zone = await cameraService.GetZoneByIdAsync(zoneId);
            if (zone == null)
            {
                return TypedResults.NotFound();
            }
            await cameraService.DeleteZoneAsync(zoneId);
            return TypedResults.NoContent();
        })
        .WithName("DeleteZone")
        .WithOpenApi();
    }
}