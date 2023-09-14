using SynoAI.Notifiers.Email;
using SynoAI.Notifiers.Pushbullet;
using SynoAI.Notifiers.Pushover;
using SynoAI.Notifiers.SynologyChat;
using SynoAI.Notifiers.Telegram;
using SynoAI.Notifiers.Webhook;
using SynoAI.Notifiers.Discord;
using SynoAI.Notifiers.Mqtt;

namespace SynoAI.Notifiers
{
    /// <summary>
    /// Handles the construction of the notifiers.
    /// </summary>
    internal abstract class NotifierFactory
    {
        public abstract INotifier Create(ILogger logger, IConfigurationSection section);

        public static INotifier Create(NotifierType type, ILogger logger, IConfigurationSection section)
        {
            NotifierFactory factory = type switch
            {
                NotifierType.Email => new EmailFactory(),
                NotifierType.Pushbullet => new PushbulletFactory(),
                NotifierType.Pushover => new PushoverFactory(),
                NotifierType.SynologyChat => new SynologyChatFactory(),
                NotifierType.Telegram => new TelegramFactory(),
                NotifierType.Webhook => new WebhookFactory(),
                NotifierType.Discord => new DiscordFactory(),
                NotifierType.MQTT => new MqttFactory(),
                _ => throw new NotImplementedException(type.ToString()),
            };
            INotifier notifier = factory.Create(logger, section);
            notifier.Cameras = section.GetSection("Cameras").Get<List<string>>();
            notifier.Types = section.GetSection("Types").Get<List<string>>();

            return notifier;
        }
    }
}
