using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SynoAI.AIs;
using SynoAI.Models;
using SynoAI.Notifiers;
using System;
using System.Collections.Generic;

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
        /// The hex code of the colour to use for the boxing around image matches.
        /// </summary>
        public static string BoxColor { get; private set; }
        /// <summary>
        /// The hex code of the colour to use for the exclusion boxes.
        /// </summary>
        public static string ExclusionBoxColor { get; private set; }
        /// <summary>
        /// The hex code of the colour to use behind the text on the image outputs.
        /// </summary>
        public static string TextBoxColor { get; private set; }

        /// <summary>
        ///The stroke width of the Box drawn around the objects.
        /// </summary>
        public static int StrokeWidth { get; private set; }

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
        /// True will only place a reference number on each label image, later detailing object type and confidence percentage on the notification text
        /// </summary>
        public static bool AlternativeLabelling { get; private set; } 
        /// <summary>
        /// True will place each image label below the boundary box.
        /// </summary>
        public static bool LabelBelowBox { get; private set; } 
        /// <summary>
        /// Upon movement, the maximum number of snapshots sequentially retrieved from SSS until finding an object of interest (i.e. 4 snapshots)
        /// </summary>
        public static int MaxSnapshots { get; private set; } 
        /// <summary>
        /// Whether this original snapshot generated from the API should be saved to the file system.
        /// </summary>
        public static SaveSnapshotMode SaveOriginalSnapshot { get; private set; }

        /// <summary>
        /// The amount of days to keep captured images before automatically deleting them.
        /// </summary>
        public static int DaysToKeepCaptures { get; private set; }

        /// <summary>
        /// The artificial intelligence system to process the images with.
        /// </summary>
        public static AIType AI { get; private set; }
        /// <summary>
        /// The URL to access the AI.
        /// </summary>
        public static string AIUrl { get; private set; }
        /// <summary>
        /// Development use only. The internal path to call the AI. Potentially a better way to do this would be to support multiple AIs and have separate configs baked into each AI.
        /// </summary>
        public static string AIPath { get; private set; }

        /// <summary>
        /// The period of time in milliseconds (ms) that must occur between the last motion detection of camera and the next time it'll be processed.
        /// </summary>
        public static int Delay { get; private set; }
        /// <summary>
        /// The period of time in milliseconds (ms) that must occur between the last successful motion detection of camera and the next time it'll be processed.
        /// </summary>
        public static int? DelayAfterSuccess { get; private set; }

        /// <summary>
        /// The default minimum width that an object must be to be considered valid for reporting. Can be overridden on a camera by camera basis to account for different camera resolutions.
        /// </summary>
        public static int MinSizeX { get; private set; }
        /// <summary>
        /// The default minimum height that an object must be to be considered valid for reporting. Can be overridden on a camera by camera basis to account for different camera resolutions.
        /// </summary>
        public static int MinSizeY { get; private set; }

        /// <summary>
        /// The default maximum width that an object must be to be considered valid for reporting. Can be overridden on a camera by camera basis to account for different camera resolutions.
        /// </summary>
        public static int MaxSizeX { get; private set; }
        /// <summary>
        /// The default maximum height that an object must be to be considered valid for reporting. Can be overridden on a camera by camera basis to account for different camera resolutions.
        /// </summary>
        public static int MaxSizeY { get; private set; }

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
        /// Whether the draw the exclusion zone boxes on images. Useful for testing box locations.
        /// </summary>
        public static bool DrawExclusions {get; private set; }

        /// <summary>
        /// The list of possible notifiers.
        /// </summary>
        public static IEnumerable<INotifier> Notifiers { get; private set; }

        /// <summary>
        /// The URL to use for the SynoAI web frontend.
        /// </summary>
        public static string SynoAIUrL { get; private set; }

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
            
            DrawMode = configuration.GetValue<DrawMode>("DrawMode", DrawMode.Matches);
            DrawExclusions = configuration.GetValue<bool>("DrawExclusions", false);

            StrokeWidth = configuration.GetValue<int>("StrokeWidth", 2);

            BoxColor = configuration.GetValue<string>("BoxColor", SKColors.Green.ToString());
            FontColor = configuration.GetValue<string>("FontColor", SKColors.Green.ToString());
            ExclusionBoxColor = configuration.GetValue<string>("ExclusionBoxColor", SKColors.Red.ToString());
            TextBoxColor = configuration.GetValue<string>("TextBoxColor", SKColors.Transparent.ToString());

            Font = configuration.GetValue<string>("Font", "Tahoma");
            FontSize = configuration.GetValue<int>("FontSize", 12);
            
            TextOffsetX = configuration.GetValue<int>("TextOffsetX", 4);
            TextOffsetY = configuration.GetValue<int>("TextOffsetY", 2);

            MinSizeX = configuration.GetValue<int>("MinSizeX", 50);
            MinSizeY = configuration.GetValue<int>("MinSizeY", 50);

            Delay = configuration.GetValue<int>("Delay", 0);
            DelayAfterSuccess = configuration.GetValue<int>("DelayAfterSuccess", 0);

            // euquiq: A bit overkill to use int.MaxValue :)
            // TODO: Just make this use 0 or null and handle appropriately
            MaxSizeX = configuration.GetValue<int>("MaxSizeX", int.MaxValue);
            MaxSizeY = configuration.GetValue<int>("MaxSizeY", int.MaxValue);

            LabelBelowBox = configuration.GetValue<bool>("LabelBelowBox", false);
            AlternativeLabelling = configuration.GetValue<bool>("AlternativeLabelling", false);
            MaxSnapshots = configuration.GetValue<int>("MaxSnapshots", 1);

            SaveOriginalSnapshot = configuration.GetValue<SaveSnapshotMode>("SaveOriginalSnapshot", SaveSnapshotMode.Off);

            DaysToKeepCaptures = configuration.GetValue<int>("DaysToKeepCaptures", 0);

            IConfigurationSection aiSection = configuration.GetSection("AI");
            AI = aiSection.GetValue<AIType>("Type", AIType.DeepStack);
            AIUrl = aiSection.GetValue<string>("Url");
            AIPath = aiSection.GetValue<string>("Path","v1/vision/detection");

            SynoAIUrL = configuration.GetValue<string>("SynoAIUrl");

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
