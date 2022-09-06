using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SynoAI.Notifiers.Webhook;

namespace SynoAI.Notifiers.Mqtt
{
    public class MqttFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            using (logger.BeginScope(nameof(MqttFactory)))
            {
                logger.LogInformation("Processing MQTT config");

                string host = section.GetValue<string>("Host", null);
                int? port = section.GetValue<int?>("Port", null);
                string username = section.GetValue<string>("Username", null);
                string password = section.GetValue<string>("Password", null);
                string baseTopic = section.GetValue<string>("BaseTopic", "synoai");
                bool sendImage = section.GetValue<bool>("SendImage", false);

                Mqtt mqtt = new Mqtt()
                {
                    Host = host,
                    Port = port,
                    Username = username,
                    Password = password,
                    BaseTopic = baseTopic,
                    SendImage = sendImage
                };

                return mqtt;
            }
        }
    }
}
