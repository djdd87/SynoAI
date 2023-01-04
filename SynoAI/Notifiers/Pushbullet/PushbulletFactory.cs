namespace SynoAI.Notifiers.Pushbullet
{
    internal class PushbulletFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            string apiKey = section.GetValue<string>("ApiKey");
            logger.LogInformation("Processing Pushbullet Config", apiKey);

            return new Pushbullet()
            {
                ApiKey = apiKey
            };
        }
    }
}
