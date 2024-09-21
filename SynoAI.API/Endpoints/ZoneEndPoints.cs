using SynoAI.Core.Data;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models.Requests;

namespace SynoAI.API.Endpoints;

public record UpdateZoneRequest(string Name);

public static class ZoneEndPoints
{
    public static void MapZoneEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/zones")
            .WithTags("Zones")
            .WithOpenApi();

        group.MapGet("/{zoneId:guid}", GetAsync)
            .WithName("GetZone")
            .WithDescription("Gets a zone by it's unique ID.");

        group.MapGet("/", async (IZoneService zoneService) =>
        {
            await zoneService.GetListAsync();
            return TypedResults.Ok();
        })
        .WithName("GetZoneList")
        .WithDescription("Gets a zone by it's unique ID.");

        group.MapPut("/{zoneId:guid}", async (Guid zoneId, UpdateZoneRequest data, IZoneService zoneService) =>
        {
            UpdateZone zone = new UpdateZone(data.Name);

            await zoneService.UpdateAsync(zoneId, zone);
            return TypedResults.Ok();
        })
        .WithName("UpdateZone")
        .WithDescription("Updates a zone with the specified ID.");

        group.MapDelete("/{zoneId:guid}", async (Guid zoneId, IZoneService zoneService) =>
        {
            await zoneService.DeleteAsync(zoneId);
            return TypedResults.Ok();
        })
        .WithName("DeleteZone")
        .WithDescription("Deletes a zone by it's unique ID.");
    }


    private static async Task<IResult> GetAsync(
        Guid zoneId,
        IZoneService zoneService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to fetch zone '{zoneId}'.", zoneId);

        Zone? result = await zoneService.GetAsync(zoneId);
        if (result is null)
        {
            logger.LogWarning("Zone {zoneId} not found", zoneId);
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<IResult> DeleteZoneAsync(
        Guid cameraId,
        ICameraService cameraService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to delete camera '{id}'.", cameraId);

        var result = await cameraService.DeleteAsync(cameraId);
        if (result.IsSuccess)
        {
            logger.LogInformation("Camera deleted with ID {id}.", cameraId);
            return Results.Created($"/{cameraId}", cameraId);
        }
        else
        {
            logger.LogWarning("Camera deletion failed: {error}", result.Error);
            return Results.BadRequest(result.Error);
        }
    }
}
