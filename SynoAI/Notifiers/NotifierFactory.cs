using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SynoAI.Notifiers.Email;
using SynoAI.Notifiers.Pushbullet;
using SynoAI.Notifiers.Telegram;
using SynoAI.Notifiers.Webhook;
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
                case NotifierType.Pushbullet:
                    factory = new PushbulletFactory();
                    break;
                case NotifierType.Webhook:
                    factory = new WebhookFactory();
                    break;
                case NotifierType.Email:
                    factory = new EmailFactory();
                    break;
                case NotifierType.Telegram:
                    factory = new TelegramFactory();
                    break;
                default:
                    throw new NotImplementedException(type.ToString());
            }

            INotifier notifier = factory.Create(logger, section);
            notifier.Cameras = section.GetSection("Cameras").Get<List<string>>();

            return notifier;
        }
    }
}
