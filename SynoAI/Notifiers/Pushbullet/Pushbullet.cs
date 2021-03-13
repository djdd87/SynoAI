using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using SynoAI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Pushbullet
{
    /// <summary>
    /// Configuration for sending a Pushbullet notification.
    /// https://docs.pushbullet.com/
    /// </summary>
    public class Pushbullet : NotifierBase
    {
        //private const int MAX_FILE_SIZE = 26214400;

        private const string URI_UPLOAD_REQUEST = "https://api.pushbullet.com/v2/upload-request";
        private const string URI_PUSHES = "https://api.pushbullet.com/v2/pushes";

        /// <summary>
        /// The API Key for sending the notification.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Sends a message and an image using the Pushbullet API.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="snapshotManager">A thread safe object for fetching the processed image.</param>
        /// <param name="foundTypes">The list of types that were found.</param>
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, ISnapshotManager snapshotManager, IEnumerable<string> foundTypes, ILogger logger)
        {
            // Pushbullet file uploads are a two part process. First we need to request to upload a file
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Access-Token", ApiKey);

                // POST the request to upload a file so we know where we're supposed to send it
                ProcessedImage processedImage = snapshotManager.GetImage(camera); 

                string fileName = processedImage.FileName;
                string requestJson = JsonConvert.SerializeObject(new PushbulletUploadRequest()
                {
                    FileName = fileName,
                    FileType = "image/jpeg"
                });

                HttpResponseMessage requestResponse = await client.PostAsync(URI_UPLOAD_REQUEST, new StringContent(requestJson, null, "application/json"));
                if (requestResponse.IsSuccessStatusCode)
                {
                    // The upload request was successful
                    PushbulletUploadRequestResponse uploadRequestResult = await GetResponse<PushbulletUploadRequestResponse>(requestResponse);

                    // POST the file to the requested URL
                    using (FileStream fileStream = processedImage.GetReadonlyStream())
                    {
                        HttpResponseMessage uploadFileResponse;
                        uploadFileResponse = await client.PostAsync(uploadRequestResult.UploadUrl, new MultipartFormDataContent
                        {
                            { new StreamContent(fileStream), "file", fileName }
                        });

                        if (uploadFileResponse.IsSuccessStatusCode)
                        {
                            // The file was uploaded successfully, so we can now send the message
                            string pushJson = JsonConvert.SerializeObject(new PushbulletPush()
                            {
                                Type = "file",
                                Title = $"{camera.Name}: Movement Detected",
                                Body = GetMessage(camera, foundTypes),
                                FileName = uploadRequestResult.FileName,
                                FileUrl = uploadRequestResult.FileUrl,
                                FileType = uploadRequestResult.FileType
                            });

                            HttpResponseMessage pushResponse = await client.PostAsync(URI_PUSHES, new StringContent(pushJson, null, "application/json"));
                            if (pushResponse.IsSuccessStatusCode)
                            {
                                logger.LogInformation($"{camera.Name}: Pushbullet notification sent successfully");
                            }
                            else
                            {
                                PushbulletErrorResponse error = await GetResponse<PushbulletErrorResponse>(pushResponse);
                                logger.LogError($"{camera.Name}: Pushbullet error sending push ({error.Error})");
                            }
                        }
                        else
                        {
                            PushbulletErrorResponse error = await GetResponse<PushbulletErrorResponse>(uploadFileResponse);
                            logger.LogError($"{camera.Name}: Pushbullet error uploading file ({error.Error})");
                        }
                    }
                }
                else
                {
                    PushbulletErrorResponse error = await GetResponse<PushbulletErrorResponse>(requestResponse);
                    logger.LogError($"{camera.Name}: Pushbullet error requesting upload ({error.Error})");
                }
            }
        }

        /// <summary>
        /// Fetches the response content and parses it a DeepStack object.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>A usable object.</returns>
        private async Task<T> GetResponse<T>(HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
