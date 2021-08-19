using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SynoAI.Models;
using SynoAI.Notifiers;
using SynoAI.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using SynoAI.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace SynoAI.Controllers
{

    public class HomeController : Controller
    {
        public DateTime date = DateTime.Now;
        public static List<GraphData> graphData = new List<GraphData>();
        public static int yMax = 0;
        public static int cameraHours = 0;
        public static long cameraStorage = 0;
        public static int cameraFiles = 0;
        static readonly string[] byteSizes = { "bytes", "Kb", "Mb", "Gb", "Tb" };

        [Route("")]
        // [Route("Home")]
        // [Route("Home/Index")]
        // [Route("Home/Index/{id?}")]



        /// <summary>
        /// Called by the user from web browser
        /// </summary>
        public IActionResult Index()
        {
            ViewData["Message"] = $"Valid detections per hour for {date:yyyy_MM_dd}";
            return View();
        }



        public static void GetGraphData(string cameraName) 
        {   
            graphData.Clear();
            yMax = 0;
            cameraHours = 0;
            cameraFiles = 0;
            cameraStorage = 0;

            string directory = $"Captures";
            directory = Path.Combine(directory, cameraName);

            if (Directory.Exists(directory))
            {
                var dir = new DirectoryInfo(directory);
                FileInfo[] files = dir.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();

                cameraFiles = files.Count();

                int index = -1;
                int objects = 0;

                string name = string.Empty;

                int objectsHour = 0;
                int validPredictionsHour = 0;
                int oldHour = -1;
                
                foreach (FileInfo item in files) 
                {   
                    // Add file size to storage counter
                    cameraStorage += item.Length;

                    // Check if current image file corresponds to a New hour                
                    if (oldHour != item.CreationTimeUtc.Hour) 
                    {
                        // This image corresponds to a new hour (and a complete hour has already been processed):
                        if (cameraHours <= 24 && oldHour != -1)
                        {
                            // Store past hour values
                            graphData.Add( new GraphData() { Hour = oldHour, Objects = objectsHour, Predictions = validPredictionsHour });

                            //Adjust Max value for Y axis
                            if ( objectsHour  > yMax)
                            {
                               yMax = objectsHour ;
                            } else if ( validPredictionsHour  > yMax)
                            {
                               yMax = validPredictionsHour ;
                            } 


                            // Reset counters for next hour calculations
                            validPredictionsHour = 0;
                            objectsHour = 0;
                        }
                        
                        oldHour = item.CreationTime.Hour;
                        cameraHours++;
                    }

                    // Inside first 24 hours, feed counters for hourly graph points
                    if (cameraHours < 25)
                    {
                        name = Path.GetFileNameWithoutExtension(item.Name);
                        index = name.IndexOf("-");
                        if (index != -1) 
                        {
                            if (!int.TryParse(name.Substring(index +1), out objects))
                                objects = 0;
                        }
                        else
                        {
                            objects = 0; //Could not grab
                        }

                        //Update counters for this ongoing hour
                        validPredictionsHour++;
                        objectsHour += objects;
                    }
                }
            }
        }

        

public static string NiceByteSize()
{
    if (cameraStorage > 0) {
        int i = 0;
        decimal dValue = (decimal)cameraStorage;
        while (Math.Round(dValue, 1) >= 1000)
        {
            dValue /= 1024;
            i++;
        }
        return string.Format("{0:n1} {1}", dValue, byteSizes[i]);
    }
    return "---";
}       

        public static int GraphBar(int value, int height) 
        {
            double maxValue = yMax;
            double currentValue = value;
            double availHeight = height;
            double result = (availHeight / maxValue) * currentValue;

            return Convert.ToInt16(result);
        }

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