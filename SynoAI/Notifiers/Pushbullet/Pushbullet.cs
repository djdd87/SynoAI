using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.App;
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
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            // Pushbullet file uploads are a two part process. First we need to request to upload a file
            ProcessedImage processedImage = notification.ProcessedImage;

            string fileName = processedImage.FileName;
            string requestJson = JsonConvert.SerializeObject(new PushbulletUploadRequest()
            {
                FileName = fileName,
                FileType = "image/jpeg"
            });

            StringContent uploadRequest = new StringContent(requestJson, null, "application/json");
            AddHeaders(uploadRequest);

            HttpResponseMessage requestResponse = await Shared.HttpClient.PostAsync(URI_UPLOAD_REQUEST, uploadRequest);
            if (requestResponse.IsSuccessStatusCode)
            {
                // The upload request was successful
                PushbulletUploadRequestResponse uploadRequestResult = await GetResponse<PushbulletUploadRequestResponse>(requestResponse);

                // POST the file to the requested URL
                using (FileStream fileStream = processedImage.GetReadonlyStream())
                {
                    MultipartFormDataContent upload = new MultipartFormDataContent
                    {
                        {
                            new StreamContent(fileStream), "file", fileName
                        }
                    };

                    AddHeaders(upload);

                    HttpResponseMessage uploadFileResponse = await Shared.HttpClient.PostAsync(uploadRequestResult.UploadUrl, upload);

                    string uploadError = null;
                    bool uploadSuccess = uploadFileResponse.IsSuccessStatusCode;
                    if (!uploadFileResponse.IsSuccessStatusCode)
                    {
                        PushbulletErrorResponse error = await GetResponse<PushbulletErrorResponse>(uploadFileResponse);
                        uploadError = $"Pushbullet error uploading file ({error.Error})";
                        logger.LogError($"{camera.Name}: {uploadError}");
                    }

                    // The file was uploaded successfully, so we can now send the message
                    string pushJson = JsonConvert.SerializeObject(new PushbulletPush()
                    {
                        Type = uploadSuccess ? "file" : "note",
                        Title = $"{camera.Name}: Movement Detected",
                        Body = GetMessage(camera, notification.FoundTypes, errorMessage: uploadError),
                        FileName = uploadSuccess ? uploadRequestResult.FileName : null,
                        FileUrl = uploadSuccess ? uploadRequestResult.FileUrl : null,
                        FileType = uploadSuccess ? uploadRequestResult.FileType : null
                    });

                    StringContent push = new StringContent(pushJson, null, "application/json");
                    AddHeaders(push);

                    HttpResponseMessage pushResponse = await Shared.HttpClient.PostAsync(new Uri(URI_PUSHES), push);
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
            }
            else
            {
                PushbulletErrorResponse error = await GetResponse<PushbulletErrorResponse>(requestResponse);
                logger.LogError($"{camera.Name}: Pushbullet error requesting upload ({error.Error})");
            }
        }

        private void AddHeaders(HttpContent content)
        {
            content.Headers.Add("Access-Token", ApiKey);
        }
    }
}
