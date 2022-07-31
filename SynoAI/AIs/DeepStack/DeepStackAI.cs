using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SynoAI.App;
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
            Stopwatch stopwatch = Stopwatch.StartNew();

            decimal minConfidence = camera.Threshold / 100m;

            MultipartFormDataContent multipartContent = new()
            {
                { new StreamContent(new MemoryStream(image)), "image", "image" },
                { new StringContent(minConfidence.ToString()), "min_confidence" } // From face detection example - using JSON with MinConfidence didn't always work
            };

            logger.LogDebug($"{camera.Name}: DeepStackAI: POSTing image with minimum confidence of {minConfidence} ({camera.Threshold}%) to {string.Join("/", Config.AIUrl, Config.AIPath)}.");

            Uri uri = GetUri(Config.AIUrl, Config.AIPath);

            try
            {
                HttpResponseMessage response = await Shared.HttpClient.PostAsync(uri, multipartContent);
                if (response.IsSuccessStatusCode)
                {
                    DeepStackResponse deepStackResponse = await GetResponse(logger, camera, response);
                    if (deepStackResponse.Success)
                    {
                        IEnumerable<AIPrediction> predictions = deepStackResponse.Predictions.Where(x => x.Confidence >= minConfidence).Select(x => new AIPrediction()
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
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"{camera.Name}: DeepStackAI: Failed to call API error '{ex}'.");
            }

            return null;
        }

        /// <summary>
        /// Builds a <see cref="Uri"/> from the provided base and resource.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="resourcePath"></param>
        /// <returns>A <see cref="Uri"/> for the combined base and resource.</returns>
        protected Uri GetUri(string basePath, string resourcePath)
        {
            Uri baseUri = new(basePath);
            return new Uri(baseUri, resourcePath);
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
