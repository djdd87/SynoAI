namespace SynoAI.Notifiers.Discord
{
    internal class DiscordFactory : NotifierFactory
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