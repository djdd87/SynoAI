
using SynologySurveillance.Net.Models;

public interface ISurveillanceApiClient
{
    Task<DigitalOutputInfo> ControlDigitalOutputAsync(string camId, int DOIndex, bool triggerState);
    Task<BookmarkInfo> CreateBookmarkAsync(string camId, string name, string startTime, string? endTime = null, string? comment = null);
    Task DeleteBookmarksAsync(string bookmarkIds, string dsId = "0");
    Task<byte[]> DownloadBookmarkRecordingAsync(int bookmarkId, string dsId = "0");
    Task<byte[]> DownloadRecordingAsync(string camId, string startTime, string endTime, string? fileName = null, bool concate = true);
    Task<byte[]> DownloadRecordingSnapshotsAsync(string startTime, string endTime, string camId, double interval);
    Task<byte[]> DownloadSnapshotsAsync(string startTime, string endTime, string? camName = null, string dsId = "0");
    Task<BookmarkInfo> EditBookmarkAsync(int bookmarkId, string? name = null, string? comment = null, string? startTime = null, string? endTime = null, string dsId = "0");
    Task<CameraInfo> EditCameraAsync(string camId, string? newName = null, string? recordPrefix = null, string? recordSchedule = null, int? rotationByDay = null, int? rotationBySpace = null);
    Task<CameraInfo[]> GetCameraInfoAsync(string camIds, bool DIDOs = false);
    Task<BookmarkListResponse> ListBookmarksAsync(string camIds, string? keyword = null, string? startTime = null, string? endTime = null);
    Task<CameraListResponse> ListCamerasAsync(string? keyword = null);
    Task<string> LoginAsync(string account, string password);
    Task LogoutAsync();
    Task PerformPtzOperationAsync(string camId, string action);
    Task<SnapshotInfo> TakeSnapshotAsync(string camId, int profileType = 0, bool download = true, bool save = true, string? time = null);
}