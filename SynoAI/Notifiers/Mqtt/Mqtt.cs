using Microsoft.Extensions.Logging;
using SynoAI.Models;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet;
using System.Threading;
using System;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace SynoAI.Notifiers.Mqtt
{
    /// <summary>
    /// Sends a message over MQTT.
    /// </summary>
    public sealed class Mqtt : NotifierBase
    {
        /// <summary>
        /// The username when using Basic authentication.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The username when using Basic authentication.
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// The username when using Basic authentication.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The password to use when using Basic authentication.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Notifications will be sent to "{BaseTopic}/{CameraName}/notification".
        /// </summary>
        public string BaseTopic { get; set; }
        /// <summary>
        /// Whether the image should be sent as part of the message payload.
        /// </summary>
        public bool SendImage { get; set; }

        private const double _connectionTimeoutSeconds = 10.0;
        private IMqttClient _client;

        public Mqtt() : base()
        {
            _client = (new MQTTnet.MqttFactory()).CreateMqttClient();
        }

        public override Task InitializeAsync(ILogger logger)
        {
            return ConnectAsync(logger);
        }

        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            logger.LogInformation("MQTT: sending notification.");

            if (BaseTopic == null)
            {
                logger.LogError("MQTT: send aborted because base topic is not configured");
                return;
            }

            MqttApplicationMessageBuilder messageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic($"{BaseTopic}/{camera.Name}/notification")
                .WithPayload(GeneratePayload(camera, notification));
            
            await _client.PublishAsync(messageBuilder.Build());
        }

        /// <summary>
        /// Connects to the MQTT broker
        /// </summary>
        private async Task ConnectAsync(ILogger logger)
        {
            if (_client.IsConnected)
            {
                logger.LogError("MQTT: connection aborted because client is already connected.");
                return;
            }

            if (Host == null)
            {
                logger.LogError("MQTT: connection failed because no host specified.");
                return;
            }

            logger.LogInformation("MQTT: connecting to broker.");

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(Host, Port);
            
            if (Username != null)
            {
                mqttClientOptionsBuilder.WithCredentials(Username, Password ?? "");
            }

            try
            {
                using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(_connectionTimeoutSeconds)))
                {
                    await _client.ConnectAsync(mqttClientOptionsBuilder.Build(), timeoutToken.Token);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("MQTT: connection failed.");
                logger.LogError(ex, "An exception occurred");
            }
        }

        /// <summary>
        /// Disconnects from the MQTT broker
        /// </summary>
        private async Task DisconnectAsync(ILogger logger)
        {
            try
            {
                if (_client != null && _client.IsConnected)
                {
                    logger.LogInformation("MQTT: disconnecting from broker.");
                    await _client.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("MQTT: disconnect failed.");
                logger.LogError(ex, "An exception occurred");
            }
        }

        /// <summary>
        /// Generates the message payload as a JSON string encoded as UTF-8
        /// </summary>
        private byte[] GeneratePayload(Camera camera, Notification notification)
        {
            var request = new
            {
                camera = camera.Name,
                foundTypes = notification.FoundTypes,
                predictions = notification.ValidPredictions,
                message = GetMessage(camera, notification.FoundTypes),
                image = SendImage ? ToBase64String(notification.ProcessedImage.GetReadonlyStream()) : null
            };

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// Returns FileStream data as a base64-encoded string
        /// </summary>
        private string ToBase64String(FileStream fileStream)
        {
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, (int)fileStream.Length);
                
            return Convert.ToBase64String(buffer);
        }

        public override Task CleanupAsync(ILogger logger)
        {
            return DisconnectAsync(logger);
        }
    }
}
