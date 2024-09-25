using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using SynoAI.API.Models;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Models;
using SynoAI.Core.Models.Contracts;

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

        group.MapGet("/", GetListAsync)
            .WithName("GetZoneList")
            .WithDescription("Returns all zones.");

        group.MapPut("/{zoneId:guid}", UpdateAsync)
            .WithName("UpdateZone")
            .WithDescription("Updates a zone with the specified ID.");

        group.MapDelete("/{zoneId:guid}", DeleteAsync)
            .WithName("DeleteZone")
            .WithDescription("Deletes a zone by it's unique ID.");
    }


    private static async Task<Ok<IEnumerable<ZoneResponse>>> GetListAsync(IZoneService zoneService, IMapper mapper)
    {
        var results = await zoneService.GetListAsync();

        var zoneDtos = mapper.Map<IEnumerable<ZoneResponse>>(results);
        return TypedResults.Ok(zoneDtos);
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

    private static async Task<Results<Ok, BadRequest<string>>> UpdateAsync(
        Guid zoneId, 
        UpdateZoneRequest data, 
        IZoneService zoneService, 
        IMapper mapper)
    {
        var contract = mapper.Map<UpdateZone>(data);

        var result = await zoneService.UpdateAsync(zoneId, contract);
        if (result.IsSuccess)
        {
            return TypedResults.Ok();
        }
        else
        {
            return TypedResults.BadRequest(result.Error);
        }
    }

    private static async Task<Results<Ok, NotFound>> DeleteAsync(
        Guid cameraId,
        ICameraService cameraService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger();
        logger.LogInformation("Calling API to delete camera '{id}'.", cameraId);

        var deleted = await cameraService.DeleteAsync(cameraId);
        if (deleted)
        {
            logger.LogInformation("Camera deleted with ID {id}.", cameraId);
            return TypedResults.Ok();
        }
        else
        {
            logger.LogWarning("Zone deletion failed: Zone not found.");
            return TypedResults.NotFound();
        }
    }
}
