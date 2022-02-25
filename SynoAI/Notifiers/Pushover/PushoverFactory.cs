using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace SynoAI.Notifiers.Pushover
{
    public class PushoverFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            logger.LogInformation("Processing Pushover Config");

            string apiKey = section.GetValue<string>("ApiKey");
            string userKey = section.GetValue<string>("UserKey");
            List<string> devices = section.GetValue<List<string>>("Devices");
            string sound = section.GetValue<string>("Sound");   // https://pushover.net/api#sounds
            PushoverPriority priority = section.GetValue("Priority", PushoverPriority.Normal); 
            int retry = section.GetValue<int>("Retry");
            int expire = section.GetValue<int>("Expire");

            return new Pushover()
            {
                ApiKey = apiKey,
                UserKey = userKey,
                Devices = devices, 
                Sound = sound,
                Priority = priority,
                Retry = retry,
                Expire = expire
            };
        }
    }
}
