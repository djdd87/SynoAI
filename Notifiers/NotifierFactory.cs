using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SynoAI.Notifiers.Pushbullet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                default:
                    throw new NotImplementedException(type.ToString());
            }

            return factory.Create(logger, section);
        }
    }
}
