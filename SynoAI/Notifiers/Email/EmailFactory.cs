using System;
using System.Net.Http;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynoAI.Notifiers.Email
{
    public class EmailFactory : NotifierFactory
    {
        public override INotifier Create(ILogger logger, IConfigurationSection section)
        {
            using (logger.BeginScope(nameof(EmailFactory)))
            {
                logger.LogInformation("Processing Email Config");
                
                SecureSocketOptions socketOptions = GetSecureSocketOptions(logger, section);
                string sender = section.GetValue<string>("Sender");
                string destination = section.GetValue<string>("Destination");
                string host = section.GetValue<string>("Host");
                int port = section.GetValue<int>("Port", 25);
                string username = section.GetValue<string>("Username");
                string password = section.GetValue<string>("Password");

                return new Email()
                {
                    Sender = sender,
                    Destination = destination,
                    Username = username,
                    Password = password,
                    Host = host,
                    SocketOptions = socketOptions,
                    Port = port
                };
            }
        }

        private SecureSocketOptions GetSecureSocketOptions(ILogger logger, IConfigurationSection section)
        {
            string options = section.GetValue<string>("Encryption", "None").ToUpper();

            if (string.IsNullOrWhiteSpace(options) || options.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return SecureSocketOptions.None;
            }

            switch (options.ToUpper())
            {
                case "AUTO":
                    return SecureSocketOptions.Auto;
                case "SSL":
                    return SecureSocketOptions.SslOnConnect;
                case "STARTTLS":
                    return SecureSocketOptions.StartTls;
                case "STARTTLSWHENAVAILABLE":
                    return SecureSocketOptions.StartTlsWhenAvailable;
                default:
                    logger.LogError($"The email encryption type '{options}' is not supported.", options);
                    throw new NotSupportedException($"The email SecureSocketOptions type '{options}' is not supported.");
            }
        }
    }
}
