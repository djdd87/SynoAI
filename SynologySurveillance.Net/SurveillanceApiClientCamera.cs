public partial class SurveillanceApiClient
{
    // Camera methods
    public async Task<CameraListResponse> ListCamerasAsync(string keyword = null)
    {
        var data = new { keyword };
        return await SendRequestAsync<CameraListResponse>("/webapi/SurveillanceStation/ThirdParty/Camera/List/v1", HttpMethod.Get, data);
    }

    public async Task<CameraInfo> EditCameraAsync(string camId, string newName = null, string recordPrefix = null, string recordSchedule = null, int? rotationByDay = null, int? rotationBySpace = null)
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

public class CameraListResponse
{
    public int Total { get; set; }
    public CameraInfo[] Cameras { get; set; }
}

public class CameraEditResponse
{
    public CameraInfo Camera { get; set; }
}

public class CameraGetResponse
{
    public CameraInfo[] Cameras { get; set; }
}

public class CameraInfo
{
    public int StatusId { get; set; }
    public string Status { get; set; }
    public string DsName { get; set; }
    public string Model { get; set; }
    public string Vendor { get; set; }
    public int DsId { get; set; }
    public string Name { get; set; }
    public string Mac { get; set; }
    public string Ip { get; set; }
    public int CamId { get; set; }
    public StreamInfo StreamHigh { get; set; }
    public StreamInfo StreamMedium { get; set; }
    public StreamInfo StreamLow { get; set; }
    public int RecStatus { get; set; }
    public string UserName { get; set; }
    public string AudioCodec { get; set; }
    public bool AudioOut { get; set; }
    public string VideoCodec { get; set; }
    public string RecordPrefix { get; set; }
    public string RecordSchedule { get; set; }
    public string RotationBySpace { get; set; }
    public int RotationByDay { get; set; }
    public int Port { get; set; }
    public string PtzCapability { get; set; }
    public int DINumber { get; set; }
    public DIDOItem[] DIList { get; set; }
    public int DONumber { get; set; }
    public DIDOItem[] DOList { get; set; }
}

public class StreamInfo
{
    public string Resolution { get; set; }
    public string Quality { get; set; }
    public int Fps { get; set; }
    public string ConstantBitrate { get; set; }
    public int BitrateCtrl { get; set; }
}

public class DIDOItem
{
    public int Index { get; set; }
    public string Name { get; set; }
    public int NormalState { get; set; }
    public bool TriggerState { get; set; }
}

public class DigitalOutputInfo
{
    public int DOIndex { get; set; }
    public string Name { get; set; }
    public int NormalState { get; set; }
    public bool TriggerState { get; set; }
}