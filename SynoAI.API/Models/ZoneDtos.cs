using AutoMapper;
using SynoAI.Core.Models;

namespace SynoAI.API.Models;

/// <summary>
/// Mapping profile.
/// </summary>
public class ZoneMappingProfile : Profile
{
    public ZoneMappingProfile()
    {
        CreateMap<Zone, ZoneResponse>();
        CreateMap<ZonePoint, ZonePointResponse>();
        CreateMap<ZonePoint, ZonePointResponse>();
        CreateMap<ZoneTarget, ZoneTargetResponse>();
    }
}

/// <summary>
/// Represents a Zone object.
/// </summary>
/// <param name="Id">The Id of the zone.</param>
/// <param name="Name">The friendly name of the zone.</param>
/// <param name="Points">The points that make up the detection or exclusion area.</param>
/// <param name="Targets">The targets that the zone will react to, e.g. "Person".</param>
/// <param name="TimeRanges">The time ranges that the zone will be active for.</param>
public record ZoneResponse(
    Guid Id,
    string Name,
    List<ZonePointResponse>? Points,
    List<ZoneTargetResponse>? Targets,
    List<ZoneTimeRangeResponse>? TimeRanges
);

/// <summary>
/// Represents the point in a <see cref="ZoneResponse"/> that defines the area of the area.
/// </summary>
/// <param name="Id">The ID of the point.</param>
/// <param name="X">The X co-ordinate.</param>
/// <param name="Y">The Y co-ordinate.</param>
/// <param name="Order">The order of the point in the area.</param>
public record ZonePointResponse(
    Guid Id, 
    int X, 
    int Y, 
    int Order
);

/// <summary>
/// Represents the defined target to look for in a <see cref="ZoneResponse"/>, e.g. a Person, Cat, Dog.
/// </summary>
/// <param name="Id">The ID of the target.</param>
/// <param name="TargetType">The type of target to look for.</param>
public record ZoneTargetResponse(
    Guid Id, 
    string TargetType
);

/// <summary>
/// Represents the time range a <see cref="ZoneResponse"/> is valid within.
/// </summary>
/// <param name="Id">The ID of the time range.</param>
/// <param name="StartTime">The start time for the Zone to be active.</param>
/// <param name="EndTime">The end time where the Zone becomes inactive.</param>
public record ZoneTimeRangeResponse(
    Guid Id, 
    TimeSpan StartTime, 
    TimeSpan EndTime
);