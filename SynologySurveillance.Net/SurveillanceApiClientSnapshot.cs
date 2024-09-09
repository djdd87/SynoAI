using SynologySurveillance.Net.Models;

public partial class SurveillanceApiClient
{
    public async Task<byte[]> DownloadSnapshotsAsync(string startTime, string endTime, string ?camName = null, string dsId = "0")
    {
        var data = new { startTime, endTime, camName, dsId };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/SnapShot/Download/v1", HttpMethod.Get, data);
    }

    public async Task<SnapshotInfo> TakeSnapshotAsync(string camId, int profileType = 0, bool download = true, bool save = true, string? time = null)
    {
        var data = new { camId, profileType, download, save, time };
        return await SendRequestAsync<SnapshotInfo>("/webapi/SurveillanceStation/ThirdParty/SnapShot/Take/v1", HttpMethod.Get, data);
    }
}