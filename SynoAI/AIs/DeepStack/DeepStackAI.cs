using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SynoAI.AIs.DeepStack
{
    public class DeepStackAI : AI
    {
        public async override Task<IEnumerable<AIPrediction>> Process(ILogger logger, Camera camera, byte[] image)
        {
            using (HttpClient client = new HttpClient())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                decimal minConfidence = camera.Threshold / 100m;
                string requestJson = JsonConvert.SerializeObject(new DeepStackRequest()
                {
                    MinConfidence = minConfidence
                });

                MultipartFormDataContent multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new StreamContent(new MemoryStream(image)), "image", "image");
                multipartContent.Add(new StringContent(requestJson, null, "application/json")); 

                client.BaseAddress = new Uri(Config.AIUrl);

                logger.LogDebug($"{camera.Name}: DeepStackAI: Sending image with minimum confidence of {minConfidence} ({camera.Threshold}%).");

                HttpResponseMessage response = await client.PostAsync(Config.AIPath, multipartContent);
                if (response.IsSuccessStatusCode)
                {
                    DeepStackResponse deepStackResponse = await GetResponse(logger, camera, response);
                    if (deepStackResponse.Success)
                    {
                        IEnumerable<AIPrediction> predictions = deepStackResponse.Predictions.Where(x=> x.Confidence >= minConfidence).Select(x => new AIPrediction()  
                        {
                            Confidence = x.Confidence * 100,
                            Label = x.Label,
                            MaxX = x.MaxX,
                            MaxY = x.MaxY,
                            MinX = x.MinX,
                            MinY = x.MinY
                        }).ToList();
                        
                        stopwatch.Stop();
                        logger.LogInformation($"{camera.Name}: DeepStackAI: Processed successfully ({stopwatch.ElapsedMilliseconds}ms).");
                        return predictions;
                    }
                    else
                    {
                        logger.LogWarning($"{camera.Name}: DeepStackAI: Failed with unknown error.");
                    }
                }
                else
                {
                    logger.LogWarning($"{camera.Name}: DeepStackAI: Failed to call API with HTTP status code '{response.StatusCode}'.");
                }

                return null;
            }
        }

        /// <summary>
        /// Fetches the response content and parses it a DeepStack object.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        /// <returns>A usable object.</returns>
        private async Task<DeepStackResponse> GetResponse(ILogger logger, Camera camera, HttpResponseMessage message)
        {
            string content = await message.Content.ReadAsStringAsync();                
            logger.LogDebug($"{camera.Name}: DeepStackAI: Responded with {content}.");

            return JsonConvert.DeserializeObject<DeepStackResponse>(content);
        }
    }
}
