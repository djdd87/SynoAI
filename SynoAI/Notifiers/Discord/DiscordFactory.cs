using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynoAI.Notifiers.Discord
{
    public class DiscordFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            string url = section.GetValue<string>("Url");
            logger.LogInformation("Processing Discord Config", url);

            return new Discord()
            {
                Url = url
            };
        }
    }
}