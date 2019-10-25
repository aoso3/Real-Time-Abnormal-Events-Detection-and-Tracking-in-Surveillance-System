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

            //int[] la = new int[21220];

            //for (int i = 0; i < 21220; i++)
            //{
            //    if (i < 11092)
            //        la[i] = 1;
            //    else
            //        la[i] = 0;

            //}
            //Serialize.SerializeObject(la, "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\target2.txt");

            //OptFlowOfBlocks o = new OptFlowOfBlocks();
            //o.calcOptFlowOfBlocks();

           
            //Clustering clus = new Clustering();
            //clus.cluster_video(@"D:\\2\\DataSet\\2\\Train\\field train_1.mpg", "D:\\codewords.txt");
            //clus.cluster_video(@"D:\\2\\DataSet\\2\\Train\\field train_2.mpg", "D:\\codewords.txt");
            //clus.cluster_video(@"D:\\2\\DataSet\\2\\Train\\field train_3.mpg", "D:\\codewords.txt");
            //clus.cluster_video(@"D:\\2\\DataSet\\2\\Train\\field train_4.mpg", "D:\\codewords.txt");
            //int frame_num = 0;
            //for (int i = 0; i < 20; i++)
            //{
            //    AbnormalDetection abnoraml = new AbnormalDetection(@"D:\1\videos\testing_videos\35.avi", "D:\\codewords.txt", frame_num);
            //    abnoraml.Detect();
            //    frame_num++;
            //}


            //Console.WriteLine("Finished");
            //Console.ReadKey();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ICU());

            //Classifiers classifiers = new Classifiers();

            ////classifiers.FeaturesExtraction("D:\\2\\DataSet\\UOR Data Set\\only abnormal 2\\8.mpg",
            ////    "D:\\2\\DataSet\\UOR Data Set\\8.txt");

            //String Root = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video";
            //String Abnormal_Data_Path = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\data2\\abnormal_data2.txt";
            //String Data_Path = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\data.txt";
            //String Labels_Path = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\target.txt";
            //double[][] train_data;
            //double[][] test_data;
            //int[] train_label;
            //int[] test_label;


            //classifiers.Data(out train_data, out test_data, out train_label, out test_label, Data_Path, Labels_Path);
            //classifiers.RandomForestLearning(train_data, test_data, train_label, test_label, Root, "RF7.bin", 20);
            //classifiers.Knn(train_data, test_data, train_label, test_label, Root, "knn7.bin");
            //classifiers.LogisticRegression(train_data, test_data, train_label, test_label, Root, "LR7.bin");
            //classifiers.Naive_Bias(train_data, test_data, train_label, test_label, Root, "NB7.bin");
            //classifiers.HMM(Abnormal_Data_Path, Root, "HMM_seq.bin");
            //classifiers.SVM(train_data, test_data, train_label, test_label, Root, "SVM7.bin");


            //Console.WriteLine("Done");
            //Console.ReadKey();

        }
    }
}