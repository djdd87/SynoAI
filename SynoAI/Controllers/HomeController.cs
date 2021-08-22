using Microsoft.AspNetCore.Mvc;
using System.IO;
using SynoAI.Models;
using System;
using System.Collections.Generic;

using System.Linq;

namespace SynoAI.Controllers
{

    public class HomeController : Controller
    {
        // public DateTime date = DateTime.Now;
        // public static List<GraphData> graphData = new List<GraphData>();
        // public static int yMax = 0;
        // public static int cameraHours = 0;
        // public static long cameraStorage = 0;
        // public static int cameraFiles = 0;
        static readonly string[] byteSizes = { "bytes", "Kb", "Mb", "Gb", "Tb" };

        [Route("")]
                /// <summary>
        /// Called by the user from web browser
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        [Route("{cameraname}/{year}/{month}/{day}/{hour}")]

        public IActionResult Camera(string cameraname, string year, string month, string day, string hour)
        {
            ViewData["camera"] = cameraname;
            ViewData["year"] = year;
            ViewData["month"] = month;
            ViewData["day"] = day;
            ViewData["hour"] = hour;
            return View();
        }

        /// <summary>
        /// Analyzes snapshots saved into a given camera folder, either for 24 hours, or for a specific hour
        /// </summary>
        public static GraphData GetData(string cameraName, DateTime date, bool GraphHour = false ) 
        {  
            GraphData data = new GraphData();
            string directory = Path.Combine("Captures", cameraName);

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