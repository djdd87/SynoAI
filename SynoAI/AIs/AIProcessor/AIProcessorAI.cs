using Newtonsoft.Json;
using SynoAI.App;
using SynoAI.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SynoAI.AIs.AIProcessor
{
    internal class AIProcessorAI : AI
    {
        public override AIType AIType => Config.AI;
        public override async Task<IEnumerable<AIPrediction>> Process(ILogger logger, Camera camera, byte[] image)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            decimal minConfidence = camera.Threshold / 100m;

            MultipartFormDataContent multipartContent = new()
            {
                { new StreamContent(new MemoryStream(image)), "image", "image" },
                { new StringContent(minConfidence.ToString()), "min_confidence" } // From face detection example - using JSON with MinConfidence didn't always work
            };

            logger.LogDebug("{CameraName}: {AIType}: POSTing image with minimum confidence of {MinConfidence} ({CameraThreshold}%) to {Url}.",
                camera.Name,
                this.AIType,
                minConfidence,
                camera.Threshold,
                string.Join("/", Config.AIUrl, Config.AIPath));

            Uri uri = GetUri(Config.AIUrl, Config.AIPath);

            try
            {
                HttpResponseMessage response = await Shared.HttpClient.PostAsync(uri, multipartContent);
                if (response.IsSuccessStatusCode)
                {
                    AIProcessorResponse deepStackResponse = await GetResponse(logger, camera, response, this.AIType);
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
                        logger.LogInformation("{CameraName}: {AIType}: Processed successfully ({ElapsedMilliseconds}ms).",
                            camera.Name,
                            this.AIType,
                            stopwatch.ElapsedMilliseconds);

                        return predictions;
                    }
                    else
                    {
                        logger.LogWarning("{cameraName}: {AIType}: Failed with unknown error.", 
                            camera.Name,
                            this.AIType);
                    }
                }
                else
                {
                    logger.LogWarning("{cameraName}: {AIType}: Failed to call API with HTTP status code '{responseStatusCode}'.",
                        camera.Name,
                        this.AIType,
                        response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                logger.LogError("{camera.Name}: {AIType}: Failed to call API error '{ex}'.",
                    camera.Name,
                    this.AIType,
                    ex
                    );
            }

            return null;
        }

        /// <summary>
        /// Builds a <see cref="Uri"/> from the provided base and resource.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="resourcePath"></param>
        /// <returns>A <see cref="Uri"/> for the combined base and resource.</returns>
        protected static Uri GetUri(string basePath, string resourcePath)
        {
            Uri baseUri = new(basePath);
            return new Uri(baseUri, resourcePath);
        }

        /// <summary>
        /// Fetches the response content and parses it a DeepStack object.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="message">The message to parse.</param>
        /// <param name="logger"></param>
        /// <param name="aiType"></param>
        /// <returns>A usable object.</returns>
        private static async Task<AIProcessorResponse> GetResponse(ILogger logger, Camera camera, HttpResponseMessage message, AIType aiType)
        {
            string content = await message.Content.ReadAsStringAsync();                
            logger.LogDebug("{cameraName}: {AIType}: Responded with {content}.",
                camera.Name,
                aiType,
                content);

            return JsonConvert.DeserializeObject<AIProcessorResponse>(content);
        }
    }
}
