using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SynoAI.AIs;
using SynoAI.Models;
using SynoAI.Notifiers;
using SynoAI.Notifiers.Pushbullet;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI
{
    /// <summary>
    /// Represents the system configuration.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// The URL to the synology API.
        /// </summary>
        public static string Url { get; private set; }
        /// <summary>
        /// The username to login to the API with.
        /// </summary>
        public static string Username { get; private set; }
        /// <summary>
        /// The password to login to the API with.
        /// </summary>
        public static string Password { get; private set; }

        /// <summary>
        /// The artificial intelligence system to process the images with.
        /// </summary>
        public static IAI AI { get; private set; }

        /// <summary>
        /// The list of cameras.
        /// </summary>
        public static IEnumerable<Camera> Cameras { get; private set; }

        /// <summary>
        /// The list of possible notifiers.
        /// </summary>
        public static INotifier Notifier { get; private set; }

        /// <summary>
        /// Generates the configuration from the provided IConfiguration.
        /// </summary>
        /// <param name="configuration">The configuration from which to pull the values.</param>
        public static void Generate(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing config.");
            Url = configuration.GetValue<string>("Url");
            Username = configuration.GetValue<string>("User");
            Password = configuration.GetValue<string>("Password");

            AI = GenerateAI(logger, configuration);
            Cameras = GenerateCameras(logger, configuration);
            Notifier = GenerateNotifier(logger, configuration);
        }

        private static IAI GenerateAI(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing AI config.");

            IConfigurationSection section = configuration.GetSection("AI");
            string type = section.GetValue<string>("Type");

            if (!Enum.TryParse(type, out AIType ai))
            {
                logger.LogError($"AI Type '{ type }' is not supported.");
                throw new NotImplementedException(type);
            }

            return AIFactory.Create(ai, logger, section);
        }

        private static IEnumerable<Camera> GenerateCameras(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing camera config.");

            IConfigurationSection section = configuration.GetSection("Cameras");
            return section.Get<List<Camera>>();
        }

        private static INotifier GenerateNotifier(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing notifier config.");

            IConfigurationSection section = configuration.GetSection("Notifier");
            string type = section.GetValue<string>("Type");

            if (!Enum.TryParse(type, out NotifierType notifier))
            {
                logger.LogError($"Notifier Type '{ type }' is not supported.");
                throw new NotImplementedException(type);
            }

            return NotifierFactory.Create(notifier, logger, section);
        }
    }
}
