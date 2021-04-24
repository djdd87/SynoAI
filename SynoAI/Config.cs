using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;
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
        /// Allow insecure URL Access to the Synology API.
        /// </summary>
        public static bool AllowInsecureUrl {get;private set;}

        /// <summary>
        /// The version of the SYNO.API.Auth API to use.
        /// </summary>
        public static int ApiVersionAuth {get;private set;}
        /// <summary>
        /// The version of the SYNO.SurveillanceStation.Camera API to use.
        /// </summary>
        public static int ApiVersionCamera {get;private set;}
        
        /// <summary>
        /// Sets the profile type, aka Quality, of the image taken by the camera. 
        /// 0 = High Quality
        /// 1 = Balanced 
        /// 2 = Low bandwidth
        /// </summary>
        public static CameraQuality Quality { get; private set; }

        /// <summary>
        /// The amount of time that needs to have passed between the last call to check the camera and the current call.
        /// </summary>
        public static int Delay { get; private set; }

        /// <summary>
        /// The hex code of the colour to use for the boxing around image matches.
        /// </summary>
        public static string BoxColor { get; private set; }

        /// <summary>
        /// The font to use on the image labels.
        /// </summary>
        public static string Font { get; private set; }
        /// <summary>
        /// The font size to use on the image labels.
        /// </summary>
        public static int FontSize { get; private set; }
        /// <summary>
        /// The hex code of the colour to use on the image labels.
        /// </summary>
        public static string FontColor { get; private set; }
        /// <summary>
        /// The offset from the left of the image boundary box.
        /// </summary>
        public static int TextOffsetX { get; private set; }
        /// <summary>
        /// The offset from the top of the image boundary box.
        /// </summary>
        public static int TextOffsetY { get; private set; }
        /// <summary>
        /// Whether this original snapshot generated from the API should be saved to the file system.
        /// </summary>
        public static bool SaveOriginalSnapshot { get; private set; }

        /// <summary>
        /// The artificial intelligence system to process the images with.
        /// </summary>
        public static AIType AI { get; private set; }
        public static string AIUrl { get; private set; }
        public static int MinSizeX { get; private set; }
        public static int MinSizeY { get; private set; }

        /// <summary>
        /// The list of cameras.
        /// </summary>
        public static IEnumerable<Camera> Cameras { get; private set; }

        /// <summary>
        /// Whether the captured image should:
        ///  - Draw all predictions over the min size (All)
        ///  - Only draw around predictions that exist in the camera's types list
        ///  - Or draw nothing.
        /// </summary>
        public static DrawMode DrawMode {get; private set; }

        /// <summary>
        /// The list of possible notifiers.
        /// </summary>
        public static IEnumerable<INotifier> Notifiers { get; private set; }

        /// <summary>
        /// Generates the configuration from the provided IConfiguration.
        /// </summary>
        /// <param name="configuration">The configuration from which to pull the values.</param>
        public static void Generate(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing config.");

            Url = configuration.GetValue<string>("Url");
            
            Username = configuration.GetValue<string>("User");  // "Username" returns the local system account when debugging, which isn't ideal. Need to resolve this.
            Password = configuration.GetValue<string>("Password");
            AllowInsecureUrl = configuration.GetValue<bool>("AllowInsecureUrl", false);

            ApiVersionAuth = configuration.GetValue<int>("ApiVersionInfo", 6);      // DSM 6.0 beta2
            ApiVersionCamera = configuration.GetValue<int>("ApiVersionCamera", 9);  // Surveillance Station 8.0

            Quality = configuration.GetValue<CameraQuality>("Quality", CameraQuality.Balanced);

            Delay = configuration.GetValue<int>("Delay", 5000);
            DrawMode = configuration.GetValue<DrawMode>("DrawMode", DrawMode.Matches);

            BoxColor = configuration.GetValue<string>("BoxColor", SKColors.Red.ToString());

            Font = configuration.GetValue<string>("Font", "Tahoma");
            FontColor = configuration.GetValue<string>("FontColor", SKColors.Red.ToString());
            FontSize = configuration.GetValue<int>("FontSize", 12);
            
            TextOffsetX = configuration.GetValue<int>("TextOffsetX", 4);
            TextOffsetY = configuration.GetValue<int>("TextOffsetY", 2);

            MinSizeX = configuration.GetValue<int>("MinSizeX", 50);
            MinSizeY = configuration.GetValue<int>("MinSizeY", 50);

            SaveOriginalSnapshot = configuration.GetValue<bool>("SaveOriginalSnapshot", false);

            IConfigurationSection aiSection = configuration.GetSection("AI");
            AI = aiSection.GetValue<AIType>("Type", AIType.DeepStack);
            AIUrl = aiSection.GetValue<string>("Url");

            Cameras = GenerateCameras(logger, configuration);
            Notifiers = GenerateNotifiers(logger, configuration);
        }

        private static IEnumerable<Camera> GenerateCameras(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing camera config.");

            IConfigurationSection section = configuration.GetSection("Cameras");
            return section.Get<List<Camera>>();
        }

        private static IEnumerable<INotifier> GenerateNotifiers(ILogger logger, IConfiguration configuration)
        {
            logger.LogInformation("Processing notifier config.");

            List<INotifier> notifiers = new List<INotifier>();

            IConfigurationSection section = configuration.GetSection("Notifiers");
            foreach (IConfigurationSection child in section.GetChildren())
            {
                string type = child.GetValue<string>("Type");

                if (!Enum.TryParse(type, out NotifierType notifier))
                {
                    logger.LogError($"Notifier Type '{ type }' is not supported.");
                    throw new NotImplementedException(type);
                }

                notifiers.Add(NotifierFactory.Create(notifier, logger, child));
            }

            return notifiers;
        }
    }
}
