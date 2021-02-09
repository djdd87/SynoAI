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
                string toEmail = section.GetValue<string>("ToEmail");
                string username = section.GetValue<string>("Username");
                string password = section.GetValue<string>("Password");
                string host = section.GetValue<string>("Host");
                SecureSocketOptions socketOptions = getSecureSocketOptions();
                int port = section.GetValue<int>("Port");

                Email email = new Email()
                {
                    ToEmail = toEmail,
                    Username = username,
                    Password = password,
                    Host = host,
                    SocketOptions = socketOptions,
                    Port = port
                };

                SecureSocketOptions getSecureSocketOptions()
                {
                    string options = section.GetValue<string>("Encryption").ToUpper();

                    if (!string.IsNullOrEmpty(options))
                    {
                        switch (options)
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
                    else
                    {
                        return SecureSocketOptions.None;
                    }
                }

                return email;
            }
        }
    }
}
