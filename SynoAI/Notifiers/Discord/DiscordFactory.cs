using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynoAI.Notifiers.Discord
{
    public class DiscordFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            string discordWebhookUrl = section.GetValue<string>("DiscordWebhookUrl");
            logger.LogInformation("Processing Discord Config", discordWebhookUrl);

            return new Discord()
            {
                DiscordWebhookUrl = discordWebhookUrl
            };
        }
    }
}