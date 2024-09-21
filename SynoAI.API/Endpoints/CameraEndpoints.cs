using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models.Requests;
using SynoAI.Core.Processors;

namespace SynoAI.API.EndPoints;

/// <summary>
/// Represents the object required to update a camera.
/// </summary>
/// <param name="Name">The name of the camera.</param>
/// <param name="QualityProfile">The quality profile to use when snapshotting the camera.</param>
public record CreateCameraRequest(string Name, QualityProfile QualityProfile);

/// <summary>
/// Represents the object required to create a zone.
/// </summary>
/// <param name="Name">The name of the zone.</param>
/// <param name="ProcessorType">The processor type to use for the zone.</param>
public record CreateZoneRequest(string Name, ProcessorType ProcessorType);

public static class CameraEndpoints
{
    public static void MapCameraEndpoints(this IEndpointRouteBuilder app)
    {
        // Map the group
        var group = app.MapGroup("/cameras")
            .WithTags("Cameras")
            .WithOpenApi();

        // Fetch all cameras
        group.MapGet("/", async (ICameraService cameraService, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger();
            logger.LogInformation("Fetching camera list");

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

        // Create a new camera
        group.MapPost("/", CreateCamera)
            .Produces<Guid>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateCamera")
            .WithDescription("Returns a list of all cameras added to the application.");

        // Deletes a camera
        group.MapDelete("/", DeleteCamera)
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("DeleteCamera")
            .WithDescription("Deletes a camera by the specified ID.");
    }

    private static async Task<IResult> CreateCamera(
        CreateCameraRequest request,
        ICameraService cameraService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to create camera.");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            logger.LogWarning("No camera name was provided.");
            return Results.BadRequest("Camera name is required.");
        }

        var createCamera = new CreateCamera
        {
            Name = request.Name,
            QualityProfile = request.QualityProfile
        };

        var result = await cameraService.CreateAsync(createCamera);
        if (result.IsSuccess)
        {
            Guid cameraId = result.Result!.Id;

            logger.LogInformation("Camera created with ID {id}.", cameraId);
            return Results.Created($"/{cameraId}", cameraId);
        }
        else
        {
            logger.LogWarning("Camera creation failed: {error}", result.Error);
            return Results.BadRequest(result.Error);
        }
    }

    private static async Task<IResult> DeleteCamera(
        Guid cameraId,
        ICameraService cameraService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to delete camera '{cameraId}'.", cameraId);

        var result = await cameraService.DeleteAsync(cameraId);
        if (result.IsSuccess)
        {
            logger.LogInformation("Camera deleted with ID {cameraId}.", cameraId);
            return Results.Created($"/{cameraId}", cameraId);
        }
        else
        {
            logger.LogWarning("Camera deletion failed: {error}", result.Error);
            return Results.BadRequest(result.Error);
        }
    }
}