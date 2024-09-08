using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;

namespace SynoAI.API.EndPoints;

public static class CameraEndpoints
{
    public static void MapCameraEndpoints(this IEndpointRouteBuilder app)
    {
        // Fetch all cameras
        app.MapGet("/cameras", async (ICameraService cameraService) =>
        {
            var results = await cameraService.GetListAsync();
            return TypedResults.Ok(results);
        })
        .Produces<IEnumerable<Camera>>(StatusCodes.Status200OK)
        .WithName("GetCameras")
        .WithTags("Cameras")
        .WithOpenApi()
        .WithDescription("Returns a list of all cameras added to the application.");

        // Fetch a specific camera by ID
        app.MapGet("/cameras/{id:guid}", async Task<Results<Ok<Camera>, NotFound>> (Guid id, ICameraService cameraService) =>
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
        .WithTags("Cameras")
        .WithOpenApi()
        .WithDescription("Returns a camera by the specified ID.");

        // Fetch a specific camera by it's name
        app.MapGet("/cameras/by-name/{name}", async Task<Results<Ok<Camera>, NotFound>> (string name, ICameraService cameraService) =>
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
        .WithTags("Cameras")
        .WithOpenApi()
        .WithDescription("Returns a camera with the specified name.");
    }
}