using System;
using System.Collections.Generic;

namespace SynoAI.Models
{
    /// <summary>
    /// These classes take care of parameters and information for graph creation / data storage
    /// </summary>

    /// <summary>
    /// Class holding definition for one data point (extracted from filenames inside a Captures/camera folder) 
    /// </summary>
    public class GraphPoint
    {
        /// <summary>
        /// Snapshots date and time period's 
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The amount of valid predictions
        /// </summary>
        public int Predictions { get; set; }

        /// <summary>
        /// The amount of valid objects detected
        /// </summary>
        public int Objects { get; set; }
    }




    /// <summary>
    /// Class holding all graph data (consisting of GraphPoints and some global metrics)
    /// </summary>
    public class GraphData
    {
        /// <summary>
        /// Highest count of objects / snapshots, used to build the y-Axis labels and graph scaling
        /// </summary>
        public int yMax { get; set; }

        /// <summary>
        /// The amount of time in hours
        /// </summary>
        public int HoursCounter { get; set; }

        /// <summary>
        /// The amount of time in minutes
        /// </summary>
        public int MinutesCounter { get; set; }


        /// <summary>
        /// The amount of snapshots / files, depending on context, either globally or for a specific hour
        /// </summary>
        public int Snapshots { get; set; }

        /// <summary>
        /// The amount of valid storage used in disk for these snapshots, depending on context, either globally or for a specific hour
        /// </summary>
        public long Storage { get; set; }

        /// <summary>
        /// List of graph points (data to create the graph itself)
        /// </summary>
        public List<GraphPoint> GraphPoints { get; set; }

        public GraphData() {
            GraphPoints = new List<GraphPoint>();
            HoursCounter = 0;
            MinutesCounter = 0;
            Snapshots = 0;
            Storage =0;
            yMax = 0;
        }



    }




    /// <summary>
    /// Class Holding graph parameters for drawing purposes
    /// </summary>
    public class GraphDraw {

        /// <summary>
        /// The width in pixels available for graphicking
        /// </summary>
        public int GraphWidth { get; set; }

        /// <summary>
        /// The height in pixels available for graphicking
        /// </summary>
        public int GraphHeight { get; set; }

        /// <summary>
        /// The x-axis height, necessary for labelling the reference values
        /// </summary>
        public int GraphXAxisHeight { get; set; }

        /// <summary>
        /// The y-axis width, necessary for labelling the reference values
        /// </summary>
        public int GraphYAxisWidth { get; set; }

        /// <summary>
        /// The y-axis height is the full graph height minus the x axis height, below.
        /// </summary>
        public int GraphYAxisHeight()
        {
            return GraphHeight - GraphXAxisHeight;
        }

        /// <summary>
        /// The steps / number of labels for the Y-Axis 
        /// </summary>
        public int GraphYSteps { get; set; }

        /// <summary>
        /// The size for each step for labels in the Y-Axis (The avail height space divided the number of steps)
        /// </summary>
        public int GraphYStepSize() 
        {
            return GraphYAxisHeight() / GraphYSteps;
        }

        /// <summary>
        /// The amount of pixels available for each column in the graph
        /// </summary>
        public int GraphColsWidth(int graphpoints, bool half = true) 
        {   
            int width = GraphWidth - GraphYAxisWidth;        
            width = width / graphpoints;
            width = width / 2;
            if (!half) width = width * 2;
            return width;
        }

        /// <summary>
        /// Calculate graph bar length, given the available height and the actual value to graph
        /// </summary>
        public int GraphBarHeight(int yMax, int value) 
        {
            //Available height is the whole graph height minus the height space used by the X-Axis labels
            double height = GraphHeight - GraphXAxisHeight;

            double result = (height / (double)yMax) * (double)value;
            return Convert.ToInt16(result);
        }


        /// <summary>
        /// Since Y axis shows <NumberOfSteps> reference values, if there are less than GraphYSteps snapshots, we need to adjust way of displaying the y-axis ref
        /// </summary>
        public String yStepping(int yMax, int Step) 
        {
            double yValue = yMax;
            yValue = yValue / GraphYSteps;
            yValue = Math.Round(yValue); 

            //Y axis label results being a minimal reasonable number, or it is just the first step (top number, max value) so use it!
            if (yValue > 1 || Step == 1)
            {        
                yValue = yValue * (Step -1);
                yValue = yMax - yValue;
                return yValue.ToString();
            }
            else
            {
                //If Y axis label results being a small number, just fill this step with a space.
                return " ";
            }
        }
    }

}