using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SynoAI.AIs.DeepStack
{
    public class DeepStackAI : AI
    {
        private const string URL_VISION_DETECTION = "v1/vision/detection";

        public async override Task<IEnumerable<AIPrediction>> Process(ILogger logger, byte[] image)
        {
            using (HttpClient client =new HttpClient())
            {
                MultipartFormDataContent multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new StreamContent(new MemoryStream(image)), "image", "image");

                client.BaseAddress = new Uri(Config.AIUrl);

                logger.LogInformation("DeepStackAI: Calling vision detection.");

                HttpResponseMessage response = await client.PostAsync(URL_VISION_DETECTION, multipartContent);
                if (response.IsSuccessStatusCode)
                {
                    DeepStackResponse deepStackResponse = await GetResponse(response);
                    if (deepStackResponse.Success)
                    {
                        logger.LogInformation("DeepStackAI: Success.");
                        return deepStackResponse.Predictions.Select(x => new AIPrediction()
                        {
                            Confidence = x.Confidence * 100,
                            Label = x.Label,
                            MaxX = x.MaxX,
                            MaxY = x.MaxY,
                            MinX = x.MinX,
                            MinY = x.MinY
                        }).ToList();
                    }
                    else
                    {
                        logger.LogWarning("DeepStackAI: Failed with unknown error.");
                    }
                }
                else
                {
                    logger.LogWarning($"DeepStackAI: Failed to call API with HTTP status code '{response.StatusCode}'.");
                }

                return null;
            }
        }

        /// <summary>
        /// Fetches the response content and parses it a DeepStack object.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>A usable object.</returns>
        private async Task<DeepStackResponse> GetResponse(HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DeepStackResponse>(content);
        }
    }
}
