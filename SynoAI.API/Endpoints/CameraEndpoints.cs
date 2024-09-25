using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.API.Models;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models.Contracts;

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
        group.MapGet("/", GetListAsync)
            .WithName("GetCameras")
            .WithDescription("Returns a list of all cameras.");

        // Fetch a specific camera by ID
        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetCameraById")
            .WithDescription("Returns a camera by the specified ID.");

        // Fetch a specific camera by it's name
        group.MapGet("/by-name/{name}", GetByNameAsync)
            .WithName("GetCameraByName")
            .WithDescription("Returns a camera with the specified name.");

        // Runs a detection action
        group.MapGet("/{cameraName}/detect", DetectByNameAsync)
            .WithName("RunCameraDetectionByName")
            .WithDescription("Runs the detection action against the camera with the specified name.");

        // Runs a detection action
        group.MapGet("/{id:guid}/detect", DetectAsync)
            .WithName("RunCameraDetection")
            .WithDescription("Runs the detection action against the camera with the specified ID.");

        // Create a new camera
        group.MapPost("/", CreateCamera)
            .WithName("CreateCamera")
            .WithDescription("Returns a list of all cameras added to the application.");

        // Deletes a camera
        group.MapDelete("/{cameraId:guid}", DeleteCamera)
            .WithName("DeleteCamera")
            .WithDescription("Deletes a camera by the specified ID.");

        // Zones
        var zoneGroup = group.MapGroup("/{cameraId:guid}/zones")
            .WithTags("Cameras")
            .WithOpenApi();

        zoneGroup.MapPost("/", CreateZoneAsync)
        .WithName("CreateZone")
        .WithDescription("Updates a zone with the specified ID.");
    }

    private static async Task<Results<Ok<IEnumerable<CameraResponse>>, NotFound>> GetListAsync(ICameraService cameraService, ILoggerFactory loggerFactory, IMapper mapper)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Fetching camera list");

        var results = await cameraService.GetListAsync();

        var CameraResponses = mapper.Map<IEnumerable<CameraResponse>>(results);
        return TypedResults.Ok(CameraResponses);
    }

    private static async Task<Results<Ok<CameraResponse>, NotFound>> GetAsync(Guid id, ICameraService cameraService, IMapper mapper, bool includeZones = false)
    {
        var camera = await cameraService.GetAsync(id);
        if (camera == null)
        {
            return TypedResults.NotFound();
        }

        var CameraResponse = mapper.Map<CameraResponse>(camera);
        return TypedResults.Ok(CameraResponse);
    }

    private static async Task<Results<Ok<CameraResponse>, NotFound>> GetByNameAsync(string name, ICameraService cameraService, IMapper mapper)
    {
        var camera = await cameraService.GetAsync(name);
        if (camera == null)
        {
            return TypedResults.NotFound();
        }

        var CameraResponse = mapper.Map<CameraResponse>(camera);
        return TypedResults.Ok(CameraResponse);
    }

    private static async Task<Results<Ok, NotFound>> DetectAsync(Guid cameraId, ICameraService cameraService)
    {
        var camera = await cameraService.GetAsync(cameraId);
        if (camera == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> DetectByNameAsync(string cameraName, ICameraService cameraService)
    {
        var camera = await cameraService.GetAsync(cameraName);
        if (camera == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok();
    }

    private static async Task<Results<Created<Guid>, BadRequest<string>>> CreateCamera(
        CreateCameraRequest request,
        ICameraService cameraService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to create camera.");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            logger.LogWarning("No camera name was provided.");
            return TypedResults.BadRequest("Camera name is required.");
        }

        var createCamera = mapper.Map<CreateCamera>(request);

        var result = await cameraService.CreateAsync(createCamera);
        if (result.IsSuccess)
        {
            Guid cameraId = result.Result!.Id;

            logger.LogInformation("Camera created with ID {cameraId}.", cameraId);
            return TypedResults.Created($"/{cameraId}", cameraId);
        }
        else
        {
            logger.LogWarning("Camera creation failed: {error}", result.Error);
            return TypedResults.BadRequest(result.Error);
        }
    }

    private static async Task<Results<Ok, NotFound>> DeleteCamera(
        Guid cameraId,
        ICameraService cameraService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to delete camera '{cameraId}'.", cameraId);

        var deleted = await cameraService.DeleteAsync(cameraId);
        if (deleted)
        {
            logger.LogInformation("Camera deleted with ID {cameraId}.", cameraId);
            return TypedResults.Ok();
        }
        else
        {
            logger.LogWarning("Camera deletion failed: Camera not found");
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok, BadRequest>> CreateZoneAsync(
        Guid cameraId, 
        CreateZoneRequest data, 
        IZoneService zoneService,
        IMapper mapper)
    {
        var contract = mapper.Map<CreateZone>(data); // TODO - How to set CameraId? Can use opts, but might be better to just pass the CameraId as a param on the service.

        await zoneService.CreateAsync(contract);
        return TypedResults.Ok();
    }
}