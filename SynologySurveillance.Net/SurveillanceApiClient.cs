using SynologySurveillance.Net.Exceptions;
using SynologySurveillance.Net.Models;
using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public partial class SurveillanceApiClient : ISurveillanceApiClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _sid;

    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

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
        var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _serializerOptions);
        if (result == null)
        {
            throw new NotSupportedException($"Failed to deserialize content {content}");
        }
        else if (!result.Success)
        {
            throw new ApiException(result.Error?.Code ?? 0, result.Error?.Message ?? "Unknown error");
        }

        return result.Data!;
    }

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

    public void Dispose()
    {
        _httpClient?.Dispose();
        _sid = null;
    }
}