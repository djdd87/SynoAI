using Microsoft.AspNetCore.Mvc;
using System.IO;
using SynoAI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Linq;

namespace SynoAI.Controllers
{

    public class HomeController : Controller
    {
        static readonly string[] byteSizes = { "bytes", "Kb", "Mb", "Gb", "Tb" };

        /// <summary>
        /// Called by the user from web browser - General view (up to 24 latest hours)
        /// </summary>
        [Route("")]
        public IActionResult Index()
        {
            // Not ready for prime-time just yet
            //return View();
            return new EmptyResult();
        }

        /// <summary>
        /// Called by the user from web browser - View snapshots in realtime for a camera
        /// </summary>
        [Route("{cameraname}/RT")]
        public IActionResult Realtime(string cameraname)
        {
            ViewData["camera"] = cameraname;
            return View();
        }


        /// <summary>
        /// Called by the user from web browser - Zoom into one hour of snapshots
        /// </summary>
        [Route("{cameraname}/{year}/{month}/{day}/{hour}")]
        public IActionResult Hour(string cameraname, string year, string month, string day, string hour)
        {
            ViewData["camera"] = cameraname;
            ViewData["year"] = year;
            ViewData["month"] = month;
            ViewData["day"] = day;
            ViewData["hour"] = hour;
            return View();
        }


        /// <summary>
        /// Called by the user from web browser - Zoom into a minute of snapshots and show it's snapshots
        /// </summary>
        [Route("{cameraname}/{year}/{month}/{day}/{hour}/{minute}")]
        public IActionResult Minute(string cameraname, string year, string month, string day, string hour,string minute)
        {
            ViewData["camera"] = cameraname;
  
            try 
            {
                ViewData["date"] = new DateTime(Int16.Parse(year),Int16.Parse(month),Int16.Parse(day),Int16.Parse(hour),Int16.Parse(minute),59,999);;
            }
            catch (Exception) 
            {
                ViewData["date"] = DateTime.Now;
            }
            return View();
        }


        /// <summary>
        /// Return snapshot image as JPEG, either in original size or a scaled down version, if asked.
        //// In order to use System.Drawing.Common
        //// In Terminal, issue: dotnet add SynoAI package System.Drawing.Common
        /// </summary>
        [Route("{cameraName}/{filename}/{width}")]
        [Route("{cameraName}/{filename}")]
        public ActionResult Snapshot(string cameraName, string filename, int width = 0)
        {   
            // Reconstruct the path to the actual snapshot inside the NAS 
            string path = Path.Combine(Constants.DIRECTORY_CAPTURES, cameraName);
            path = Path.Combine(path, filename);

            // Grab the original Snapshot
            byte[] originalSnapshot = System.IO.File.ReadAllBytes(path);

            if (width != 0) 
            {
                // New (reduced) width specified: Scale down the original snapshot

                // First retrieve the original Snapshot
                using var memoryStream = new MemoryStream(originalSnapshot);

                // Second, convert it into a bitmap for resizing
                using var originalImage = new Bitmap(memoryStream);

                //Get image ratio from original bitmap width and Height: 
                double ratio = (double)originalImage.Width / (double)originalImage.Height;   

                // Calculate new height based on that ratio and the new reduced width.
                ratio = width / ratio;
                int height = (int)Math.Floor(ratio);

                //Create a resized bitmap image
                var resized = new Bitmap(width, height);
                using var graphics = Graphics.FromImage(resized);
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;

                graphics.DrawImage(originalImage, 0, 0, width, height);

                //Convert resized image into jpeg and return
                using var stream = new MemoryStream(); 
                resized.Save(stream, ImageFormat.Jpeg);
                return File(stream.ToArray(), "image/jpeg");
            }
            else 
            {
                // Width 0 means full-size, just return the original fetched image.
                return File(originalSnapshot, "image/jpeg");
            }
        }


        /// <summary>
        /// Analyzes snapshots saved into a given camera folder, either for 24 hours, or for a specific hour
        ///  (used for graphing in Index.cshtml and Camera.cshtml)
        /// </summary>
        public static GraphData GetData(string cameraName, DateTime date, bool GraphHour = false) 
        {  
            GraphData data = new GraphData();
            string directory = Path.Combine(Constants.DIRECTORY_CAPTURES, cameraName);

            if (Directory.Exists(directory))
            {
                int objectsCounter = 0;
                int predictionsCounter = 0;
                DateTime oldDate = DateTime.MinValue;

                //Retrieve Snapshots and order it in descending Creation DateTime (most up-to-date first)
                var dir = new DirectoryInfo(directory);
                FileInfo[] snapshots = dir.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

                //User is asking for global 24 hours report, so I also meter the general storage / snapshots / hours counters.
                if (!GraphHour)
                    data.Snapshots = snapshots.Count();

                foreach (FileInfo snapshot in snapshots) 
                {
                    if (!GraphHour)                         
                        data.Storage += snapshot.Length;   // Add file size to global storage usage counter

                    //Start working from given date
                    if (snapshot.CreationTime <= date) 
                    {
                        // Graphing minutes inside an hour
                        if (GraphHour)
                        {
                            // New minute!
                            if (oldDate.Minute != snapshot.CreationTime.Minute)
                            {
                                //Store the old minute's data as graphpoint and reset counters for this new minute
                                if (oldDate != DateTime.MinValue)
                                {
                                    data = AddGraphPoint(data, oldDate, objectsCounter, predictionsCounter);
                                    objectsCounter = 0;
                                    predictionsCounter = 0;
                                }

                                if ((oldDate.Hour != snapshot.CreationTime.Hour && oldDate != DateTime.MinValue)) break; // Only graphing inside designated hour: work is done.

                                data.MinutesCounter++;
                            }
                            // Update counters: Add filesize to storage usage and increment snapshot counters, etc.
                            data.Storage += snapshot.Length;
                            data.Snapshots++;
                            predictionsCounter++;
                            objectsCounter += GetObjects(snapshot.Name);
                        }
                        else
                        {
                            //Graphing a 24 hours timespan, new hour:                      
                            if ((oldDate.Hour != snapshot.CreationTime.Hour || oldDate == DateTime.MinValue)) 
                            {
                                //An hour of snapshots just passed thru: Add it to the graph!
                                if (data.HoursCounter <= 24  && oldDate != DateTime.MinValue) {
                                    data = AddGraphPoint(data,oldDate, objectsCounter, predictionsCounter);
                                    objectsCounter = 0;
                                    predictionsCounter = 0;
                                }
                                data.HoursCounter++;
                            }

                            // If inside 24 hours graph window, update counters
                            if (data.HoursCounter <=24) 
                            {
                                predictionsCounter++;
                                objectsCounter += GetObjects(snapshot.Name);
                            }
                        }                    

                        //Move forward into next snapshot...
                        oldDate = snapshot.CreationTime;
                    }
                }

                // Are there any remaining predictions left, either inside the last "remaining" hour or minute  ?
                if (predictionsCounter > 0)
                    data = AddGraphPoint(data,oldDate, objectsCounter, predictionsCounter);       
            }
            return data;
        }


        /// <summary>
        /// Analyzes snapshots saved into a given camera folder, for a specific minute
        /// (used for grabbing the snapshots in Gallery.cshtml)
        /// </summary>
        public static List<String> GetSnapshots(string cameraName, DateTime date) 
        {
            List<String> files = new List<String>();
            string directory = Path.Combine(Constants.DIRECTORY_CAPTURES, cameraName);

            if (Directory.Exists(directory))
            {
                //Retrieve Snapshots and order it in descending Creation DateTime (most up-to-date first)
                var dir = new DirectoryInfo(directory);
                FileInfo[] snapshots = dir.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

                foreach (FileInfo snapshot in snapshots) 
                {
                    //Start working from given date
                    if (snapshot.CreationTime <= date) 
                    {
                        if (snapshot.CreationTime.Minute != date.Minute) break; //We are not inside the current minute, any more.
                        files.Add(snapshot.Name); //Store the snapshot filename.
                    }
                }
            }
            return files;
        }



        /// <summary>
        /// Given the Snapshot filename, extract the number of objects detected, if available
        /// </summary>
        private static int GetObjects(string filename)
        {
            int objects = 0;
            string name = Path.GetFileNameWithoutExtension(filename);
            int index = name.IndexOf("-");
            if (index != -1)
            {
                //try to extract the number of valid objects predicted inside this snapshot
                if (!int.TryParse(name.Substring(index + 1), out objects))
                    objects = 0;
            }
            else
            {
                objects = 0; //Could not grab
            }
            return objects;
        }


        /// <summary>
        /// Adds a Graph Point value into the Graph data and updates max y-axis value
        /// </summary>
        private static GraphData AddGraphPoint(GraphData data, DateTime date, int objects, int predictions) 
        {
            // Store past hour values
            data.GraphPoints.Add( new GraphPoint() { Date = date, Objects = objects, Predictions = predictions });

            //Adjust Max value for Y axis
            if ( objects  > data.yMax)
            {
               data.yMax = objects ;
            } 
            else if ( predictions  > data.yMax)
            {
                data.yMax = predictions;
            } 
            return data;
        }


        /// <summary>
        /// Create a nice string showing the filesize formatted into Kb, Mb, Gb, etc. 
        /// </summary>
        public static string NiceByteSize(long numberOfBytes)
        {
            if (numberOfBytes > 0) {
                int i = 0;
                decimal dValue = (decimal)numberOfBytes;
                while (Math.Round(dValue, 1) >= 1000)
                {
                    dValue /= 1024;
                    i++;
                }
                return string.Format("{0:n1} {1}", dValue, byteSizes[i]);
            }
            return "---";
        }       


        /// <summary>
        /// Returns string including all objects types configured for valid detection inside the given camera
        /// </summary>
        public static string GetTypes(Camera camera) 
        {
            if (camera.Types.Count() == 0) 
            {
                 return "Any";
            }
            else
            {
                string items = string.Empty;
                foreach (string item in camera.Types)
                {
                    items+= item + " ";
                    
                }
                return items;
            }
        }
    }
}