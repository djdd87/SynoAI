using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace SynoAI.Notifiers.Pushover
{
    public class PushoverFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            string apiKey = section.GetValue<string>("ApiKey");
            string userKey = section.GetValue<string>("UserKey");
            List<string> devices = section.GetValue<List<string>>("Devices");
            string sound = section.GetValue<string>("Sound");   // https://pushover.net/api#sounds
            int priority = section.GetValue<int>("Priority");   // https://pushover.net/api#priority
            string title = section.GetValue<string>("Title");   

            logger.LogInformation("Processing Pushover Config", apiKey);

            return new Pushover()
            {
                ApiKey = apiKey,
                UserKey = userKey
            };
        }
    }
}
