using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using SynoAI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Webhook
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    public class Webhook : NotifierBase
    {
        /// <summary>
        /// The URL to send the request to.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The HTTP method (POST/PUT/etc).
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The type of authentication.
        /// </summary>
        public AuthorizationMethod Authentication { get; set; }
        /// <summary>
        /// The username when using Basic authentication.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password to use when using Basic authentication.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The token to use when using Bearer authentication.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The field name when posting the image.
        /// </summary>
        public string ImageField { get; set; }
        /// <summary>
        /// Whether the image should be sent in POST/PUT/PATCH requests. When this property is true, the request will made using 
        /// content-type of multipart/form-data.
        /// </summary>
        public bool SendImage { get; set; }
        /// <summary>
        /// Allow insecure URL Access to the API.
        /// </summary>
        public bool AllowInsecureUrl { get; set; }

        /// <summary>
        /// Sends a notification to the Webhook.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="notification">The notification data to process.</param>
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            logger.LogInformation($"{camera.Name}: Webhook: Processing");
            using (HttpClient client = GetHttpClient())
            {
                FileStream fileStream = null;
                client.DefaultRequestHeaders.Authorization = GetAuthenticationHeader();

                IEnumerable<string> foundTypes = notification.FoundTypes;
                string message = GetMessage(camera, foundTypes);

                HttpContent content;
                if (SendImage)
                {
                    // If we're sending the image, then we need to send the data as multipart/form-data.
                    string typesJson = JsonConvert.SerializeObject(foundTypes);
                    string validPredictionsJson = JsonConvert.SerializeObject(notification.ValidPredictions);

                    MultipartFormDataContent form = new()
                    {
                        { new StringContent(camera.Name), "\"camera\"" },
                        { new StringContent(typesJson), "\"foundTypes\"" },
                        { new StringContent(validPredictionsJson), "\"predictions\"" },
                        { new StringContent(message), "\"message\"" }
                    };

                    string imageUrl = GetImageUrl(camera, notification);
                    if (imageUrl != null)
                    {
                        form.Add(new StringContent(imageUrl), "\"imageUrl\"");
                    }

                    switch (Method)
                    {
                        case "PATCH":
                        case "POST":
                        case "PUT":
                            ProcessedImage processedImage = notification.ProcessedImage;
                            fileStream = processedImage.GetReadonlyStream();
                            form.Add(new StreamContent(fileStream), ImageField, processedImage.FileName);
                            break;
                    }

                    content = form;
                }
                else
                {
                    // Otherwise we can just use a simple JSON object
                    content = new StringContent(GenerateJSON(camera, notification, false), null, "application/json");
                }

                logger.LogInformation($"{camera.Name}: Webhook: Calling {Method}.");

                HttpResponseMessage response;
                try
                {
                    switch (Method)
                    {
                        case "DELETE":
                            response = await client.DeleteAsync(Url);
                            break;
                        case "GET":
                            response = await client.GetAsync(Url);
                            break;
                        case "PATCH":
                            response = await client.PatchAsync(Url, content);
                            break;
                        case "POST":
                            response = await client.PostAsync(Url, content);
                            break;
                        case "PUT":
                            response = await client.PutAsync(Url, content);
                            break;
                        default:
                            logger.LogError($"{camera.Name}: Webhook: The method type '{Method}' is not supported.");
                            return;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"{camera.Name}: Webhook: Unhandled Exception occurred '{ex.Message}'.");
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation($"{camera.Name}: Webhook: Success.");
                    logger.LogDebug($"{camera.Name}: Webhook: Success with HTTP status code '{response.StatusCode}'.");
                }
                else
                {
                    logger.LogWarning($"{camera.Name}: Webhook: The end point responded with HTTP status code '{response.StatusCode}'.");
                }

                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="HttpClient"/> object for the Webhook request.
        /// </summary>
        /// <returns>A <see cref="HttpClient"/>.</returns>
        private HttpClient GetHttpClient()
        {
            if (!Config.AllowInsecureUrl)
            {
                return new();
            }

            HttpClientHandler httpClientHandler = new()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            return new(httpClientHandler);
        }

        /// <summary>
        /// Generates an authentication header for the client.
        /// </summary>
        /// <returns>An authentication header.</returns>
        private AuthenticationHeaderValue GetAuthenticationHeader()
        {
            string parameter;
            switch (Authentication)
            {
                case AuthorizationMethod.Basic:
                    byte[] bytes = Encoding.ASCII.GetBytes($"{Username}:{Password}");
                    parameter = Convert.ToBase64String(bytes);
                    break;
                case AuthorizationMethod.Bearer:
                    parameter = Token;
                    break;
                default:
                    return null;
            }

            return new AuthenticationHeaderValue(Authentication.ToString(), parameter);
        }
    }
}
