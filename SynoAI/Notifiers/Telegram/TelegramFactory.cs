using System;
using System.Net.Http;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynoAI.Notifiers.Telegram
{
    public class TelegramFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            using (logger.BeginScope(nameof(TelegramFactory)))
            {
                logger.LogInformation("Processing Telegram Config");
                
                string token = section.GetValue<string>("Token");
                string chatId = section.GetValue<string>("ChatID");
                string photoBaseURL = section.GetValue<string>("PhotoBaseURL");

                return new Telegram()
                {
                    ChatID = chatId,
                    Token = token,
                    PhotoBaseURL = photoBaseURL
                };
            }
        }
    }
}
