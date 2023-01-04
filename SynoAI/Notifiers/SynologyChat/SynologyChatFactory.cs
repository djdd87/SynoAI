namespace SynoAI.Notifiers.SynologyChat
{
    internal class SynologyChatFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            using (logger.BeginScope(nameof(SynologyChatFactory)))
            {
                logger.LogInformation($"Processing {nameof(SynologyChat)} Config");

                string url = section.GetValue<string>("Url");

                SynologyChat webhook = new SynologyChat()
                {
                    Url = url
                };

                return webhook;
            }
        }
    }
}
