using AutoMapper;
using SynoAI.Core;
using SynoAI.Core.Models;

namespace SynoAI.API.Models;

/// <summary>
/// Mapping profile.
/// </summary>
public class CameraMappingProfile : Profile
{
    public CameraMappingProfile()
    {
        CreateMap<Camera, CameraResponse>();
        CreateMap<Zone, CameraZoneResponse>();
    }
}

/// <summary>
/// Represents a camera.
/// </summary>
/// <param name="Id">The ID of the camera.</param>
/// <param name="Name">The name of the camera.</param>
/// <param name="Zones">A list of zones associated with the camera.</param>
public record CameraResponse(
    Guid Id,
    string Name,
    List<CameraZoneResponse>? Zones
);

/// <summary>
/// Represents a camera zone.
/// </summary>
/// <param name="Id">The ID of the camera.</param>
/// <param name="Name">The name of the camera.</param>
/// <param name="ZoneType">The type of zone.</param>
public record CameraZoneResponse(
    Guid Id,
    string Name,
    ZoneType ZoneType
);

/// <summary>
/// Represents the object required to create a camera.
/// </summary>
/// <param name="Name">The name of the camera.</param>
/// <param name="QualityProfile">The quality profile to use when snapshotting the camera.</param>
public record CreateCameraRequest(
    string Name,
    QualityProfile? QualityProfile
);