using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SynoAI.Notifiers.Email;
using SynoAI.Notifiers.Pushbullet;
using SynoAI.Notifiers.Pushover;
using SynoAI.Notifiers.SynologyChat;
using SynoAI.Notifiers.Telegram;
using SynoAI.Notifiers.Webhook;
using SynoAI.Notifiers.Discord;
using SynoAI.Notifiers.Mqtt;
using System;
using System.Collections.Generic;

namespace SynoAI.Notifiers
{
    /// <summary>
    /// Handles the construction of the notifiers.
    /// </summary>
    public abstract class NotifierFactory
    {
        public abstract INotifier Create(ILogger logger, IConfigurationSection section);

        public static INotifier Create(NotifierType type, ILogger logger, IConfigurationSection section)
        {
            NotifierFactory factory;
            switch (type)
            {
                case NotifierType.Email:
                    factory = new EmailFactory();
                    break;
                case NotifierType.Pushbullet:
                    factory = new PushbulletFactory();
                    break;
                case NotifierType.Pushover:
                    factory = new PushoverFactory();
                    break;
                case NotifierType.SynologyChat:
                    factory = new SynologyChatFactory();
                    break;
                case NotifierType.Telegram:
                    factory = new TelegramFactory();
                    break;
                case NotifierType.Webhook:
                    factory = new WebhookFactory();
                    break;
                case NotifierType.Discord:
                    factory = new DiscordFactory();
                    break;
                case NotifierType.MQTT:
                    factory = new MqttFactory();
                    break;
                default:
                    throw new NotImplementedException(type.ToString());
            }

            INotifier notifier = factory.Create(logger, section);
            notifier.Cameras = section.GetSection("Cameras").Get<List<string>>();
            notifier.Types = section.GetSection("Types").Get<List<string>>();

            return notifier;
        }
    }
}
