using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;

namespace SynoAI.API.EndPoints;

public static class CameraEndpoints
{
    public static void MapCameraEndpoints(this IEndpointRouteBuilder app)
    {
        // Map the group
        var group = app.MapGroup("/cameras")
            .WithTags("Cameras")
            .WithOpenApi();

        // Fetch all cameras
        group.MapGet("/", async (ICameraService cameraService) =>
        {
            var results = await cameraService.GetListAsync();
            return TypedResults.Ok(results);
        })
        .Produces<IEnumerable<Camera>>(StatusCodes.Status200OK)
        .WithName("GetCameras")
        .WithDescription("Returns a list of all cameras added to the application.");

        // Fetch a specific camera by ID
        group.MapGet("/{id:guid}", async Task<Results<Ok<Camera>, NotFound>> (Guid id, ICameraService cameraService) =>
        {
            var camera = await cameraService.GetAsync(id);
            if (camera == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(camera);
        })
        .Produces<IEnumerable<Camera>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetCameraById")
        .WithDescription("Returns a camera by the specified ID.");

        // Fetch a specific camera by it's name
        group.MapGet("/by-name/{name}", async Task<Results<Ok<Camera>, NotFound>> (string name, ICameraService cameraService) =>
        {
            var camera = await cameraService.GetAsync(name);
            if (camera == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(camera);
        })
        .Produces<IEnumerable<Camera>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetCameraByName")
        .WithDescription("Returns a camera with the specified name.");

        // Create a group for the camera zones
        var zoneGroup = group.MapGroup("/{cameraId:guid}/zones");

        // Fetches all zones for the specified camera
        zoneGroup.MapGet("/", async (Guid cameraId, ICameraService cameraService) =>
        {
            var zones = await cameraService.GetZonesForCameraAsync(cameraId);
            return TypedResults.Ok(zones);
        })
        .WithName("GetZonesForCamera");

        // Adds a zone to the specified camera
        zoneGroup.MapPost("/", async (Guid cameraId, Zone zone, ICameraService cameraService) =>
        {
            await cameraService.AddZoneToCameraAsync(cameraId, zone);
            return TypedResults.Created($"/cameras/{cameraId}/zones", zone);
        })
        .WithName("AddZoneToCamera");
    }
}