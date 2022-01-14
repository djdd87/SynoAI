using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynoAI.Notifiers.Pushover
{
    public class PushoverFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            string apiKey = section.GetValue<string>("ApiKey");
            string userKey = section.GetValue<string>("UserKey");
            
            logger.LogInformation("Processing Pushover Config", apiKey);

            return new Pushover()
            {
                ApiKey = apiKey,
                UserKey = userKey
            };
        }
    }
}
