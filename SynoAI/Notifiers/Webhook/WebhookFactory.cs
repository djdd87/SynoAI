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
                string imageField = section.GetValue<string>("ImageField", "image");
                string method = section.GetValue<string>("Method", "POST");
                bool sendImage = section.GetValue<bool>("SendImage", true);
                bool sendTypes = section.GetValue<bool>("SendTypes", false);
                bool allowInsecureUrl = section.GetValue("AllowInsecureUrl", false);

                Webhook webhook = new()
                {
                    Url = url,
                    Authentication = authentication,
                    Username = username,
                    Password = password,
                    Token = token,
                    SendImage = sendImage,
                    AllowInsecureUrl = allowInsecureUrl
                };

                if (!string.IsNullOrWhiteSpace(imageField))
                {
                    webhook.ImageField = imageField.Trim();
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
