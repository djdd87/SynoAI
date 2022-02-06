using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using SynoAI.Models;
using SynoAI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SynoAI.Notifiers.Email
{
    /// <summary>
    /// Calls a third party API.
    /// </summary>
    public class Email : NotifierBase
    {
        /// <summary>
        /// The email address to send the notification from.
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// The email address to send the notification to.
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// The username to autheticate on the smtp server.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// The port of the smtp server.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The email provider host.
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The port of the email provider.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// The encryption method supported by the host.
        /// </summary>
        public SecureSocketOptions SocketOptions { get; internal set; }

        /// <summary>
        /// Sends a message and an image using the Pushbullet API.
        /// </summary>
        /// <param name="camera">The camera that triggered the notification.</param>
        /// <param name="notification">The notification data to process.</param>
        /// <param name="logger">A logger.</param>
        public override async Task SendAsync(Camera camera, Notification notification, ILogger logger)
        {
            using (logger.BeginScope($"Email '{Destination}'"))
            {
                // Assign camera name to variable for logger placeholder
                string cameraName = camera.Name;
                string filePath = notification.ProcessedImage.FilePath;

                try
                {
                    // Create the email message
                    string sender = string.IsNullOrWhiteSpace(Sender) ? Destination : Sender;

                    MimeMessage email = new MimeMessage();
                    email.From.Add(new MailboxAddress("SynoAI", sender));
                    email.To.Add(MailboxAddress.Parse(Destination));
                    email.Subject = $"SynoAI: Movement Detected ({camera.Name})";

                    BodyBuilder builder = new BodyBuilder();
                    builder.HtmlBody = $"<h2>Movement detected on camera: {camera.Name}</h2>";
                    builder.Attachments.Add(filePath);
                    email.Body = builder.ToMessageBody();

                    // Send email
                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Connect(Host, Port, SocketOptions);
                        if(!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                        {
                            smtp.Authenticate(Username, Password);
                        }
                        
                        await smtp.SendAsync(email);
                        smtp.Disconnect(true);
                    }

                    logger.LogInformation("{cameraName}: Email notification sent successfully", cameraName);
                }
                catch (Exception ex)
                {
                    logger.LogError("{cameraName}: Error occurred sending email", cameraName);
                    logger.LogError(ex, "An exception occurred");
                }
            }
        }
    }
}
