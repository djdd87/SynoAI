public partial class SurveillanceApiClient
{
    // Bookmark methods
    public async Task<BookmarkInfo> CreateBookmarkAsync(string camId, string name, string startTime, string endTime = null, string comment = null)
    {
        var data = new { camId, name, startTime, endTime, comment };
        var result = await SendRequestAsync<BookmarkCreateResponse>("/webapi/SurveillanceStation/ThirdParty/Bookmark/Create/v1", HttpMethod.Get, data);
        return result.Bookmark[0];
    }

    public async Task<BookmarkListResponse> ListBookmarksAsync(string camIds, string keyword = null, string startTime = null, string endTime = null)
    {
        var data = new { camIds, keyword, startTime, endTime };
        return await SendRequestAsync<BookmarkListResponse>("/webapi/SurveillanceStation/ThirdParty/Bookmark/List/v1", HttpMethod.Get, data);
    }

    public async Task<byte[]> DownloadBookmarkRecordingAsync(int bookmarkId, string dsId = "0")
    {
        var data = new { bookmarkId, dsId };
        return await SendRequestAsync<byte[]>("/webapi/SurveillanceStation/ThirdParty/Bookmark/DownloadRecording/v1", HttpMethod.Get, data);
    }

    public async Task<BookmarkInfo> EditBookmarkAsync(int bookmarkId, string name = null, string comment = null, string startTime = null, string endTime = null, string dsId = "0")
    {
        var data = new { bookmarkId, name, comment, startTime, endTime, dsId };
        var result = await SendRequestAsync<BookmarkEditResponse>("/webapi/SurveillanceStation/ThirdParty/Bookmark/Edit/v1", HttpMethod.Get, data);
        return result.Bookmark[0];
    }

    public async Task DeleteBookmarksAsync(string bookmarkIds, string dsId = "0")
    {
        var data = new { bookmarkIds, dsId };
        await SendRequestAsync<object>("/webapi/SurveillanceStation/ThirdParty/Bookmark/Delete/v1", HttpMethod.Get, data);
    }
}

public class BookmarkCreateResponse
{
    public BookmarkInfo[] Bookmark { get; set; }
}

public class BookmarkListResponse
{
    public BookmarkInfo[] Bookmarks { get; set; }
}

public class BookmarkEditResponse
{
    public BookmarkInfo[] Bookmark { get; set; }
}

public class BookmarkInfo
{
    public string CamName { get; set; }
    public int CamId { get; set; }
    public int DsId { get; set; }
    public string EndTime { get; set; }
    public string StartTime { get; set; }
    public int BookmarkId { get; set; }
    public string Comment { get; set; }
    public string Name { get; set; }
}