using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SynoAI.Services
{
    public class SynologyService : ISynologyService
    {
        /// <summary>
        /// The current cookie with valid authentication.
        /// </summary>
        private static Cookie Cookie { get; set; }
        /// <summary>
        /// A list of all cameras mapped from the config friendly name to the Synology Camera ID.
        /// </summary>
        protected static Dictionary<string, int> Cameras { get; private set; }

        private const string API_LOGIN = "SYNO.API.Auth";
        private const string API_CAMERA = "SYNO.SurveillanceStation.Camera";

        private const string URI_INFO = "webapi/query.cgi?api=SYNO.API.Info&version=1&method=query";
        private const string URI_LOGIN = "webapi/{0}?api=SYNO.API.Auth&method=Login&version={1}&account={2}&passwd={3}&session=SurveillanceStation";
        private const string URI_CAMERA_INFO = "webapi/{0}?api=SYNO.SurveillanceStation.Camera&method=List&version={1}";
        private const string URI_CAMERA_SNAPSHOT = "webapi/{0}?version={1}&id={2}&api=\"SYNO.SurveillanceStation.Camera\"&method=GetSnapshot";

        /// <summary>
        /// Holds the entry point to the SYNO.API.Auth API entry point.
        /// </summary>
        private static string _loginPath { get; set; }
        /// <summary>
        /// Holds the entry point to the SYNO.SurveillanceStation.Camera API entry point.
        /// </summary>
        private static string _cameraPath { get; set; }

        private IHostApplicationLifetime _applicationLifetime;
        private ILogger<SynologyService> _logger;

        public SynologyService(IHostApplicationLifetime applicationLifetime, ILogger<SynologyService> logger)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }


        /// <summary>
        /// Fetches all the end points, because they're dynamic between DSM versions.
        /// </summary>
        public async Task<bool> GetEndPointsAsync()
        {
            _logger.LogInformation("API: Querying end points");

            using (HttpClient httpClient = GetHttpClient(Config.Url))
            {
                HttpResponseMessage result = await httpClient.GetAsync(URI_INFO);
                if (result.IsSuccessStatusCode)
                {
                    SynologyResponse<SynologyApiInfoResponse> response = await GetResponse<SynologyApiInfoResponse>(result);
                    if (response.Success)
                    {
                        // Find the Authentication entry point
                        if (response.Data.TryGetValue(API_LOGIN, out SynologyApiInfo loginInfo))
                        {
                            _logger.LogDebug($"API: Found path '{loginInfo.Path}' for {API_LOGIN}");

                            if (loginInfo.MaxVersion < Config.ApiVersionAuth)
                            {
                                _logger.LogError($"API: {API_CAMERA} only supports a max version of {loginInfo.MaxVersion}, but the system is set to use version {Config.ApiVersionAuth}.");
                            }
                        }
                        else
                        {
                            _logger.LogError($"API: Failed to find {API_LOGIN}.");
                        }

                        // Find the Camera entry point
                        if (response.Data.TryGetValue(API_CAMERA, out SynologyApiInfo cameraInfo))
                        {
                            _logger.LogDebug($"API: Found path '{cameraInfo.Path}' for {API_CAMERA}");

                            if (cameraInfo.MaxVersion < Config.ApiVersionCamera)
                            {
                                _logger.LogError($"API: {API_CAMERA} only supports a max version of {cameraInfo.MaxVersion}, but the system is set to use version {Config.ApiVersionCamera}.");
                            }
                        }
                        else
                        {
                            _logger.LogError($"API: Failed to find {API_CAMERA}.");
                        }

                        _loginPath = loginInfo.Path;
                        _cameraPath = cameraInfo.Path;

                        _logger.LogInformation("API: Successfully mapped all end points");
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"API: Failed due to error code '{response.Error.Code}'");
                    }
                }
                else
                {
                    _logger.LogError($"API: Failed due to HTTP status code '{result.StatusCode}'");
                }
            }
            return false;
        }

        /// <summary>
        /// Generates a login cookie for the username and password in the config.
        /// </summary>
        /// <returns>A cookie, or null on failure.</returns>
        public async Task<Cookie> LoginAsync()
        {
            _logger.LogInformation("Login: Authenticating");

            string loginUri = string.Format(URI_LOGIN, _loginPath, Config.ApiVersionAuth, Config.Username, Config.Password);
            _logger.LogDebug($"Login: Logging in ({loginUri})");

            CookieContainer cookieContainer = new CookieContainer();
            using (HttpClient httpClient = GetHttpClient(Config.Url, cookieContainer))
            {
                HttpResponseMessage result = await httpClient.GetAsync(loginUri);
                if (result.IsSuccessStatusCode)
                {
                    SynologyResponse<SynologyLogin> response = await GetResponse<SynologyLogin>(result);
                    if (response.Success)
                    {
                        _logger.LogInformation("Login: Successful");

                        IEnumerable<Cookie> cookies = cookieContainer.GetCookies(httpClient.BaseAddress).Cast<Cookie>().ToList();
                        Cookie cookie = cookies.FirstOrDefault(x => x.Name == "id");
                        if (cookie == null)
                        {
                            _applicationLifetime.StopApplication();
                        }

                        return cookie;
                    }
                    else
                    {
                        _logger.LogError($"Login: Failed due to Synology API error code '{response.Error.Code}'");
                    }
                }
                else
                {
                    _logger.LogError($"Login: Failed due to HTTP status code '{result.StatusCode}'");
                }
            }
            return null;
        }

        /// <summary>
        /// Fetches all of the required camera information from the API.
        /// </summary>
        /// <returns>A list of all cameras.</returns>
        public async Task<IEnumerable<SynologyCamera>> GetCamerasAsync()
        {
            _logger.LogInformation("GetCameras: Fetching Cameras");

            CookieContainer cookieContainer = new CookieContainer();
            using (HttpClient client = GetHttpClient(Config.Url, cookieContainer))
            {
                cookieContainer.Add(client.BaseAddress, new Cookie("id", Cookie.Value));

                string cameraInfoUri = string.Format(URI_CAMERA_INFO, _cameraPath, Config.ApiVersionCamera);
                HttpResponseMessage result = await client.GetAsync(cameraInfoUri);

                SynologyResponse<SynologyCameras> response = await GetResponse<SynologyCameras>(result);
                if (response.Success)
                {
                    _logger.LogInformation($"GetCameras: Successful. Found {response.Data.Cameras.Count()} cameras.");
                    return response.Data.Cameras;
                }
                else
                {
                    _logger.LogError($"GetCameras: Failed due to error code '{response.Error.Code}'");
                }
            }

            return null;
        }

        /// <summary>
        /// Takes a snapshot of the specified camera.
        /// </summary>
        /// <returns>A string to the file path.</returns>
        public async Task<byte[]> TakeSnapshotAsync(string cameraName)
        {
            CookieContainer cookieContainer = new CookieContainer();
            using (HttpClient client = GetHttpClient(Config.Url, cookieContainer))
            {
                cookieContainer.Add(client.BaseAddress, new Cookie("id", Cookie.Value));

                if (Cameras.TryGetValue(cameraName, out int id))
                {
                    _logger.LogDebug($"{cameraName}: Found with Synology ID '{id}'.");

                    string resource = string.Format(URI_CAMERA_SNAPSHOT + $"&profileType={(int)Config.Quality}", _cameraPath, Config.ApiVersionCamera, id);
                    _logger.LogDebug($"{cameraName}: Taking snapshot from '{resource}'.");

                    _logger.LogInformation($"{cameraName}: Taking snapshot");
                    HttpResponseMessage response = await client.GetAsync(resource);
                    response.EnsureSuccessStatusCode();

                    if (response.Content.Headers.ContentType.MediaType == "image/jpeg")
                    {
                        // Only return the bytes when we have a valid image back
                        _logger.LogDebug($"{cameraName}: Reading snapshot");
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        // We didn't get an image type back, so this must have errored
                        SynologyResponse errorResponse = await GetErrorResponse(response);
                        if (errorResponse.Success)
                        {
                            // This should never happen, but let's add logging just in case
                            _logger.LogError($"{cameraName}: Failed to get snapshot, but the API reported success.");
                        }
                        else
                        {
                            _logger.LogError($"{cameraName}: Failed to get snapshot with error code '{errorResponse.Error.Code}'");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"The camera with the name '{cameraName}' was not found in the Synology camera list.");
                }

                return null;
            }
        }

        /// <summary>
        /// Fetches the response content and parses it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the return 'data'.</typeparam>
        /// <param name="message">The message to parse.</param>
        /// <returns>A Synology response object.</returns>
        private async Task<SynologyResponse<T>> GetResponse<T>(HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SynologyResponse<T>>(content);
        }

        /// <summary>
        /// Fetches the error response content and parses it to the specified type.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>A Synology response object.</returns>
        private async Task<SynologyResponse> GetErrorResponse(HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SynologyResponse>(content);
        }

        public async Task InitialiseAsync()
        {
            _logger.LogInformation("Initialising");

            // Get the actual end points, because they're not guaranteed to be the same on all installations and DSM versions
            try
            {
                bool retrievedEndPoints = await GetEndPointsAsync();
                if (!retrievedEndPoints) 
                {
                    _applicationLifetime.StopApplication();
                }

                // Perform a login first as all actions need a valid cookie
                Cookie = await LoginAsync();
                if (Cookie == null)
                {
                    // The login failed, so kill the application
                    _applicationLifetime.StopApplication();
                    return;
                }

                // Fetch all the cameras and store a Name to ID dictionary for quick lookup
                IEnumerable<SynologyCamera> synologyCameras = await GetCamerasAsync();
                if (synologyCameras == null)
                {
                    // We failed to fetch the cameras, so kill the application
                    _applicationLifetime.StopApplication();
                    return;
                }
                else
                {
                    Cameras = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (Camera camera in Config.Cameras)
                    {
                        SynologyCamera match = synologyCameras.FirstOrDefault(x => x.GetName().Equals(camera.Name, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            _logger.LogWarning($"GetCameras: The camera with the name '{camera.Name}' was not found in the Surveillance Station camera list.");
                        }
                        else
                        {
                            Cameras.Add(camera.Name, match.Id);
                        }
                    }
                }

                _logger.LogInformation("Initialisation successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred initialising SynoAI. Exiting...");
                _applicationLifetime.StopApplication();
            }
        }

        /// <summary>
        /// Generates an HttpClient object.
        /// </summary>
        /// <param name="baseAddress">The base URI.</param>
        /// <param name="cookieContainer">The container for the cookies.</param>
        /// <returns>An HttpClient.</returns>
        private HttpClient GetHttpClient(string baseAddress, CookieContainer cookieContainer = null)
        {           
            Uri uri = new Uri(baseAddress);
            return GetHttpClient(uri, cookieContainer);
        }

        /// <summary>
        /// Generates an HttpClient object.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="cookieContainer">The container for the cookies.</param>
        /// <returns>An HttpClient.</returns>
        private HttpClient GetHttpClient(Uri baseUri, CookieContainer cookieContainer = null)
        {           
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            if (cookieContainer != null)
            {
                httpClientHandler.CookieContainer = cookieContainer;
            }

            if (Config.AllowInsecureUrl)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
            }
            
            return new HttpClient(httpClientHandler)
            {
                BaseAddress = baseUri
            };
        }
    }
}
