using SynologySurveillance.Net.Models;

public partial class SurveillanceApiClient
{
    // Camera methods
    public async Task<CameraListResponse> ListCamerasAsync(string? keyword = null)
    {
        var data = new { keyword };
        return await SendRequestAsync<CameraListResponse>("/webapi/SurveillanceStation/ThirdParty/Camera/List/v1", HttpMethod.Get, data);
    }

    public async Task<CameraInfo> EditCameraAsync(string camId, string? newName = null, string? recordPrefix = null, string? recordSchedule = null, int? rotationByDay = null, int? rotationBySpace = null)
    {
        var data = new { camId, newName, recordPrefix, recordSchedule, rotationByDay, rotationBySpace };
        var result = await SendRequestAsync<CameraEditResponse>("/webapi/SurveillanceStation/ThirdParty/Camera/Edit/v1", HttpMethod.Get, data);
        return result.Camera;
    }

    public async Task<CameraInfo[]> GetCameraInfoAsync(string camIds, bool DIDOs = false)
    {
        var data = new { camIds, DIDOs };
        var result = await SendRequestAsync<CameraGetResponse>("/webapi/SurveillanceStation/ThirdParty/Camera/Get/v1", HttpMethod.Get, data);
        return result.Cameras;
    }

    public async Task PerformPtzOperationAsync(string camId, string action)
    {
        var data = new { camId, action };
        await SendRequestAsync<object>("/webapi/SurveillanceStation/ThirdParty/Camera/PTZControl/v1", HttpMethod.Get, data);
    }

    public async Task<DigitalOutputInfo> ControlDigitalOutputAsync(string camId, int DOIndex, bool triggerState)
    {
        var data = new { camId, DOIndex, triggerState };
        return await SendRequestAsync<DigitalOutputInfo>("/webapi/SurveillanceStation/ThirdParty/Camera/DOControl/v1", HttpMethod.Get, data);
    }
}