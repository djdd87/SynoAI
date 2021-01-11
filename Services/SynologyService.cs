using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Exceptions;
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
    public class SynologyService
    {
        /// <summary>
        /// The current cookie with valid authentication.
        /// </summary>
        private static Cookie Cookie { get; set; }
        /// <summary>
        /// A list of all cameras mapped from the config friendly name to the Synology Camera ID.
        /// </summary>
        protected static Dictionary<string, int> Cameras { get; private set; } = new Dictionary<string, int>();

        private const string URI_LOGIN = "webapi/auth.cgi?api=SYNO.API.Auth&method=Login&version=1&account={0}&passwd={1}&session=SurveillanceStation";
        private const string URI_CAMERA_INFO = "webapi/entry.cgi?api=SYNO.SurveillanceStation.Camera&method=List&version=3";
        private const string URI_CAMERA_SNAPSHOT = "webapi/entry.cgi?camStm=1&version=3&cameraId={0}&api=\"SYNO.SurveillanceStation.Camera\"&method=GetSnapshot";

        public async Task LoginAsync(IHostApplicationLifetime applicationLifetime, ILogger logger)
        {
            logger.LogInformation("Login: Creating client");

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
                            logger.LogInformation("Login: Successful");

                            IEnumerable<Cookie> cookies = cookieContainer.GetCookies(httpClient.BaseAddress).Cast<Cookie>().ToList();
                            Cookie = cookies.FirstOrDefault(x => x.Name == "id");
                            if (Cookie == null)
                            {
                                applicationLifetime.StopApplication();                            
                            }
                        }
                        else
                        {
                            logger.LogError($"Login: Failed due to error code '{response.Error.Code}'");
                            applicationLifetime.StopApplication();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fetches all of the required camera information from the API.
        /// </summary>
        /// <returns>A list of all cameras.</returns>
        public async Task GetCamerasAsync(IHostApplicationLifetime applicationLifetime, ILogger logger)
        {
            logger.LogInformation("GetCameras: Fetching Cameras");

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
                    logger.LogInformation("GetCameras: Successful. Checking Configuration.");

                    // Ensure that all of the cameras in the config exist
                    IEnumerable<SynologyCamera> synologyCameras = response.Data.Cameras;
                    foreach (Camera camera in Config.Cameras)
                    {
                        SynologyCamera match = synologyCameras.FirstOrDefault(x=> x.Name.Equals(camera.Name, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            logger.LogWarning($"GetCameras: The camera with the name '{camera.Name}' was not found in the Surveillance Station camera list.");
                        }
                        else
                        {
                            Cameras.Add(camera.Name, match.Id);
                        }
                    }
                }
                else
                {
                    logger.LogError($"GetCameras: Failed due to error code '{response.Error.Code}'");
                    applicationLifetime.StopApplication();
                }
            }
        }

        /// <summary>
        /// Fetches the response content and parses it to the specified type..
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
        /// Generates the URI for the provided resource.
        /// </summary>
        /// <param name="resource">The resource to get the path for.</param>
        /// <returns>A string URI.</returns>
        private Uri GetURI(string resource)
        {
            string uri = Config.Url;
            if (!uri.EndsWith("/"))
            {
                uri += "/";
            }
            return new Uri(uri + resource);
        }

        public static async Task InitialiseAsync(IHostApplicationLifetime applicationLifetime, ILogger logger)
        {
            logger.LogInformation("Synology Service: Initialising");

            SynologyService service = new SynologyService();
            await service.LoginAsync(applicationLifetime, logger);
            await service.GetCamerasAsync(applicationLifetime, logger);

            logger.LogInformation("Synology Service: Initialised");
        }
    }
}
