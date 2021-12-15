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
                AuthorizationMethod authentication = section.GetValue<AuthorizationMethod>("Authorization", AuthorizationMethod.None);
                string username = section.GetValue<string>("Username", null);
                string password = section.GetValue<string>("Password", null);
                string token = section.GetValue<string>("Token", null);
                string field = section.GetValue<string>("Field", "image");
                string method = section.GetValue<string>("Method", "POST");
                bool sendImage = section.GetValue<bool>("SendImage", true);
                bool sendTypes = section.GetValue<bool>("SendTypes", false);

                Webhook webhook = new Webhook()
                {
                    Url = url,
                    Authentication = authentication,
                    Username = username,
                    Password = password,
                    Token = token,
                    SendImage = sendImage,
                    SendTypes = sendTypes
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
