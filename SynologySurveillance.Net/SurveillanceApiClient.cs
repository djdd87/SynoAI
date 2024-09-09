using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public partial class SurveillanceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string _sid;

    public SurveillanceApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    private async Task<T> SendRequestAsync<T>(string endpoint, HttpMethod method, object data = null)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");

        if (data != null)
        {
            request.Content = JsonContent.Create(data);
        }

        if (!string.IsNullOrEmpty(_sid))
        {
            request.RequestUri = new Uri($"{request.RequestUri}&_sid={_sid}");
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (!result.Success)
        {
            throw new ApiException(result.Error?.Code ?? 0, result.Error?.Message ?? "Unknown error");
        }

        return result.Data;
    }

    // Authentication methods
    public async Task<string> LoginAsync(string account, string password)
    {
        var data = new { account, passwd = password };
        var result = await SendRequestAsync<LoginResponse>("/webapi/SurveillanceStation/ThirdParty/Auth/Login/v1", HttpMethod.Get, data);
        _sid = result.Sid;
        return _sid;
    }

    public async Task LogoutAsync()
    {
        await SendRequestAsync<object>("/webapi/SurveillanceStation/ThirdParty/Auth/Logout/v1", HttpMethod.Get);
        _sid = null;
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public ErrorInfo Error { get; set; }
}

public class ErrorInfo
{
    public int Code { get; set; }
    public string Message { get; set; }
}

public class ApiException : Exception
{
    public int ErrorCode { get; }

    public ApiException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class LoginResponse
{
    public string Sid { get; set; }
}