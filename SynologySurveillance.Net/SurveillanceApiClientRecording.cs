using SynologySurveillance.Net.Models;

public partial class SurveillanceApiClient
{
    public async Task<byte[]> DownloadRecordingAsync(string camId, string startTime, string endTime, string? fileName = null, bool concate = true)
    {
        var data = new { camId, startTime, endTime, fileName, concate };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/Recording/Download/v1", HttpMethod.Get, data);
    }

    public async Task<byte[]> DownloadRecordingSnapshotsAsync(string startTime, string endTime, string camId, double interval)
    {
        var data = new { startTime, endTime, camId, interval };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/Recording/DownloadSnapshot/v1", HttpMethod.Get, data);
    }
}