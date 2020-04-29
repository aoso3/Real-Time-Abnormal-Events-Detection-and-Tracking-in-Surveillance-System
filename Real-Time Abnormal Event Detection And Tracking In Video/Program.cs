using Accord.MachineLearning;
using Accord.IO;
using Accord.MachineLearning.Performance;
using Accord.Math.Distances;
using Accord.Statistics.Analysis;
using real_time_abnormal_event_detection_and_tracking_in_video;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math.Optimization.Losses;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ICU());
        }
    }
}