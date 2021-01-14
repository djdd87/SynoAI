using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynoAI.Notifiers.Webhook
{
    public class WebhookFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            using (logger.BeginScope(nameof(WebhookFactory)))
            {
                logger.LogInformation("Processing Webhook Config");

                string url = section.GetValue<string>("Url");
                string field = section.GetValue<string>("Field");
                string method = section.GetValue<string>("Method");

                Webhook webhook = new Webhook()
                {
                    Url = url
                };

                if (!string.IsNullOrWhiteSpace(field))
                {
                    webhook.Field = field.Trim();
                }

                if (!string.IsNullOrWhiteSpace(method))
                {
                    webhook.Method = method.ToUpper().Trim();
                }

                return webhook;
            }
        }
    }
}
