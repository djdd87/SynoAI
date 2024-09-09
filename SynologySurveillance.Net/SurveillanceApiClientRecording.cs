public partial class SurveillanceApiClient
{
    // Recording methods
    public async Task<byte[]> DownloadRecordingAsync(string camId, string startTime, string endTime, string fileName = null, bool concate = true)
    {
        var data = new { camId, startTime, endTime, fileName, concate };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/Recording/Download/v1", HttpMethod.Get, data);
    }

    public async Task<byte[]> DownloadRecordingSnapshotsAsync(string startTime, string endTime, string camId, double interval)
    {
        var data = new { startTime, endTime, camId, interval };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/Recording/DownloadSnapshot/v1", HttpMethod.Get, data);
    }

    // Snapshot methods
    public async Task<byte[]> DownloadSnapshotsAsync(string startTime, string endTime, string camName = null, string dsId = "0")
    {
        var data = new { startTime, endTime, camName, dsId };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/SnapShot/Download/v1", HttpMethod.Get, data);
    }

    public async Task<SnapshotInfo> TakeSnapshotAsync(string camId, int profileType = 0, bool download = true, bool save = true, string time = null)
    {
        var data = new { camId, profileType, download, save, time };
        return await SendRequestAsync<SnapshotInfo>("/webapi/SurveillanceStation/ThirdParty/SnapShot/Take/v1", HttpMethod.Get, data);
    }
}

public class SnapshotInfo
{
    public int DsId { get; set; }
    public int SnapshotId { get; set; }
    public string CamName { get; set; }
}