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

        private const string URI_LOGIN = "webapi/auth.cgi?api=SYNO.API.Auth&method=Login&version=3&account={0}&passwd={1}&session=SurveillanceStation";
        private const string URI_CAMERA_INFO = "webapi/entry.cgi?api=SYNO.SurveillanceStation.Camera&method=List&version=3";
        private const string URI_CAMERA_SNAPSHOT = "webapi/entry.cgi?camStm=1&version=3&cameraId={0}&api=\"SYNO.SurveillanceStation.Camera\"&method=GetSnapshot";

        private IHostApplicationLifetime _applicationLifetime;
        private ILogger<SynologyService> _logger;

        public SynologyService(IHostApplicationLifetime applicationLifetime, ILogger<SynologyService> logger)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }


        /// <summary>
        /// Generates a login cookie for the username and password in the config.
        /// </summary>
        /// <returns>A cookie, or null on failure.</returns>
        public async Task<Cookie> LoginAsync()
        {
            _logger.LogInformation("Login: Creating client");

            CookieContainer cookieContainer = new CookieContainer();
            using (HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            })
            {
                string loginUri = string.Format(URI_LOGIN, Config.Username, Config.Password);

                using (HttpClient httpClient = new HttpClient(httpClientHandler))
                {
                    httpClient.BaseAddress = new Uri(Config.Url);

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
                            _logger.LogError($"Login: Failed due to error code '{response.Error.Code}'");
                        }
                    }
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

            Uri baseAddress = new Uri(Config.Url);

            CookieContainer cookieContainer = new CookieContainer();
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (HttpClient client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                cookieContainer.Add(baseAddress, new Cookie("id", Cookie.Value));
                
                HttpResponseMessage result = await client.GetAsync(URI_CAMERA_INFO);

                SynologyResponse<SynologyCameras> response = await GetResponse<SynologyCameras>(result);
                if (response.Success)
                {
                    _logger.LogInformation($"GetCameras: Successful. Found {response.Data.Total} cameras.");
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
            Uri baseAddress = new Uri(Config.Url);

            CookieContainer cookieContainer = new CookieContainer();
            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (HttpClient client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                cookieContainer.Add(baseAddress, new Cookie("id", Cookie.Value));

                if (Cameras.TryGetValue(cameraName, out int id))
                {                    
                    _logger.LogInformation($"TakeSnapshot-{cameraName}: Found with Synology ID '{id}'.");
                    string resource = string.Format(URI_CAMERA_SNAPSHOT + $"&profileType={Config.Quality}", id);

                    _logger.LogInformation($"TakeSnapshot-{cameraName}: Taking snapshot");
                    HttpResponseMessage response = await client.GetAsync(resource);
                    response.EnsureSuccessStatusCode();

                    if (response.Content.Headers.ContentType.MediaType == "image/jpeg")
                    {
                        // Only return the bytes when we have a valid image back
                        _logger.LogInformation($"TakeSnapshot-{cameraName}: Reading snapshot");
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        // We didn't get an image type back, so this must have errored
                        SynologyResponse errorResponse = await GetErrorResponse(response);
                        if (errorResponse.Success)
                        {
                            // This should never happen, but let's add logging just in case
                            _logger.LogError($"TakeSnapshot-{cameraName}: Failed to get snapshot, but the API reported success.");
                        }
                        else
                        {
                            _logger.LogError($"TakeSnapshot-{cameraName}: Failed to get snapshot with error code '{errorResponse.Error.Code}'");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"TakeSnapshot: The camera with the name '{cameraName}' was not found in the Synology camera list.");
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
                    SynologyCamera match = synologyCameras.FirstOrDefault(x => x.Name.Equals(camera.Name, StringComparison.OrdinalIgnoreCase));
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
    }
}
