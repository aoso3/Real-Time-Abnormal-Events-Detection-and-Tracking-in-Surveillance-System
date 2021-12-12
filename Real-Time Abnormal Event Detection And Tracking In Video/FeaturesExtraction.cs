using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.CV.VideoStab;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Controls;
using System.Reflection;
using Emgu.CV.Features2D;
using Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video;


namespace real_time_abnormal_event_detection_and_tracking_in_video
{
    /// <summary>
    /// This class is used to extract features from the video.
    /// </summary>
    class FeaturesExtraction
    {

        Mat frame, prev_frame, mag, ang;
        VideoCapture cap = null;
        double[][] featuers_list;
        double[] feat_list;
        int total_frames;
        List<int> F_L;

        /// <summary>
        /// takes the video and process it two frames at a time to calculate
        /// optical flow features and save them on the disk.
        /// </summary>
        /// <param name="vid">Path of the video on the disk.</param>
        /// <param name="save_path">Path to save the features on the disk.</param>
        /// <returns></returns>
        public void Extract_Featurers2(String vid, String save_path)
        {

            int mm = 0;
            try
            {
                mag = new Mat();
                ang = new Mat();
                frame = new Mat();
                prev_frame = new Mat();
                cap = new VideoCapture(vid);
                total_frames = Convert.ToInt32(cap.GetCaptureProperty(CapProp.FrameCount));
                F_L = new List<int>();


                frame = cap.QueryFrame();
                prev_frame = frame;

                Console.WriteLine(total_frames);

            }
            catch (NullReferenceException except)
            {
                Console.WriteLine(except.Message);
            }
            //17900
            while (mm < total_frames - 2)
            {

                try
                {
                    prev_frame = frame;
                    frame = cap.QueryFrame();

                    Bitmap image = new Bitmap(frame.Bitmap);

                    // Create a new FAST Corners Detector
                    FastCornersDetector fast = new FastCornersDetector()
                    {
                        Suppress = true, // suppress non-maximum points
                        Threshold = 70   // less leads to more corners
                    };

                    // Process the image looking for corners
                    List<IntPoint> points = fast.ProcessImage(image);

                    // Create a filter to mark the corners
                    PointsMarker marker = new PointsMarker(points);

                    // Apply the corner-marking filter
                    Bitmap markers = marker.Apply(image);

                    // Show on the screen
                    //Accord.Controls.ImageBox.Show(markers);

                    // Use it to extract interest points from the Lena image:
                    List<IntPoint> descriptors = fast.ProcessImage(image);
                    PointF[] features = new PointF[descriptors.Count];

                    int c = 0;
                    foreach (IntPoint p in descriptors)
                    {
                        features[c] = new PointF(p.X, p.Y);
                        c++;
                    }

                    ImageViewer viewer = new ImageViewer();

                    Image<Gray, Byte> prev_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
                    Image<Gray, Byte> curr_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
                    curr_grey_img = frame.ToImage<Gray, byte>();
                    prev_grey_img = prev_frame.ToImage<Gray, Byte>();

                    PointF[] shiftedFeatures;
                    Byte[] status;
                    float[] trackErrors;

                    CvInvoke.CalcOpticalFlowPyrLK(prev_grey_img, curr_grey_img, features, new Size(9, 9), 3, new MCvTermCriteria(20, 0.05),
                       out shiftedFeatures, out status, out trackErrors);

                    //Image<Gray, Byte> displayImage = cap.QueryFrame().ToImage<Gray, Byte>();
                    //for (int i = 0; i < features.Length; i++)
                    //    displayImage.Draw(new LineSegment2DF(features[i], shiftedFeatures[i]), new Gray(), 2);


                    for (int i = 0; i < features.Length; i++)
                    {
                        CvInvoke.Circle(frame, System.Drawing.Point.Round(shiftedFeatures[i]), 4, new MCvScalar(0, 255, 255), 2);
                    }

                    int mean_X = 0;
                    int mean_Y = 0;

                    foreach (PointF p in shiftedFeatures)
                    {
                        mean_X += (int)p.X;
                        mean_Y += (int)p.Y;
                    }

                    mean_X /= shiftedFeatures.Length;
                    mean_Y /= shiftedFeatures.Length;

                    F_L.Add(mean_X);
                    F_L.Add(mean_Y);


                    //double[] inner = new double[] { mean_X, mean_Y };
                    //featuers_list[mm] = inner;

                    //viewer.Image = frame;
                    //viewer.ShowDialog();
                    //prev_frame = frame;

                    //Console.WriteLine("frame:{0} " + mm);
                    Console.WriteLine("frame:{0} " + mm + "  X:{1} " + mean_X + "   Y:{2} " + mean_Y);

                    mm++;
                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }

            }
            //int go = 0;
            //foreach (double[] arr in featuers_list)
            //{
            //    Console.Write("frame:{0} ", go++);
            //    foreach (double d in arr)
            //        Console.Write(d + "    ");

            //    Console.WriteLine();
            //}
            Serialize.SerializeObject(F_L, save_path);

        }


        /// <summary>
        /// takes the video and calculate motion influance flow features
        /// for only the next two frames and return the featues.
        /// </summary>
        /// <param name="vid">Path of the video on the disk.</param>
        /// <param name="current_frame">The frame that we are now at while playing the video.</param>
        /// <returns>Optical flow featues features List</returns>
        public double[] extract(String vid, int current_frame)
        {

            int mm = 0;
            try
            {
                mag = new Mat();
                ang = new Mat();
                frame = new Mat();
                prev_frame = new Mat();
                cap = new VideoCapture(vid);
                total_frames = Convert.ToInt32(cap.GetCaptureProperty(CapProp.FrameCount));

                cap.SetCaptureProperty(CapProp.PosFrames, current_frame);
                frame = cap.QueryFrame();
                prev_frame = frame;

                //Console.WriteLine(total_frames);

            }
            catch (NullReferenceException except)
            {
                Console.WriteLine(except.Message);
            }

            for (int r = current_frame; r < current_frame + 2; r++)
            {

                try
                {
                    prev_frame = frame;
                    frame = cap.QueryFrame();

                    Bitmap image = new Bitmap(frame.Bitmap);

                    // Create a new FAST Corners Detector
                    FastCornersDetector fast = new FastCornersDetector()
                    {
                        Suppress = true, // suppress non-maximum points
                        Threshold = 70   // less leads to more corners
                    };

                    // Process the image looking for corners
                    List<IntPoint> points = fast.ProcessImage(image);

                    // Create a filter to mark the corners
                    PointsMarker marker = new PointsMarker(points);

                    // Apply the corner-marking filter
                    Bitmap markers = marker.Apply(image);

                    // Show on the screen
                    //Accord.Controls.ImageBox.Show(markers);

                    // Use it to extract interest points from the Lena image:
                    List<IntPoint> descriptors = fast.ProcessImage(image);
                    PointF[] features = new PointF[descriptors.Count];

                    int c = 0;
                    foreach (IntPoint p in descriptors)
                    {
                        features[c] = new PointF(p.X, p.Y);
                        c++;
                    }

                    ImageViewer viewer = new ImageViewer();

                    Image<Gray, Byte> prev_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
                    Image<Gray, Byte> curr_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
                    curr_grey_img = frame.ToImage<Gray, byte>();
                    prev_grey_img = prev_frame.ToImage<Gray, Byte>();

                    PointF[] shiftedFeatures;
                    Byte[] status;
                    float[] trackErrors;

                    CvInvoke.CalcOpticalFlowPyrLK(prev_grey_img, curr_grey_img, features, new Size(9, 9), 3, new MCvTermCriteria(20, 0.05),
                       out shiftedFeatures, out status, out trackErrors);

                    //Image<Gray, Byte> displayImage = cap.QueryFrame().ToImage<Gray, Byte>();
                    //for (int i = 0; i < features.Length; i++)
                    //    displayImage.Draw(new LineSegment2DF(features[i], shiftedFeatures[i]), new Gray(), 2);

                    for (int i = 0; i < features.Length; i++)
                    {
                        CvInvoke.Circle(frame, System.Drawing.Point.Round(shiftedFeatures[i]), 4, new MCvScalar(0, 255, 255), 2);
                    }

                    double mean_X = 0;
                    double mean_Y = 0;

                    foreach (PointF p in shiftedFeatures)
                    {
                        mean_X += p.X;
                        mean_Y += p.Y;
                    }

                    mean_X /= shiftedFeatures.Length;
                    mean_Y /= shiftedFeatures.Length;

                    feat_list = new double[] { mean_X, mean_Y };

                    //viewer.Image = frame;
                    //viewer.ShowDialog();
                    //prev_frame = frame;

                    //Console.WriteLine("frame:{0} " + mm);
                    //Console.WriteLine("frame:{0} " + mm + "  X:{1} " + mean_X + "   Y:{2} " + mean_Y);

                    mm++;
                    return feat_list;
                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }

            }

            return null;

        }

        // Older virsion of Extract_Featurers

        //public void Extract_Featurers(String vid, String save_path)
        //{

        //    int mm = 0;
        //    try
        //    {
        //        mag = new Mat();
        //        ang = new Mat();
        //        frame = new Mat();
        //        prev_frame = new Mat();
        //        cap = new VideoCapture(vid);
        //        total_frames = Convert.ToInt32(cap.GetCaptureProperty(CapProp.FrameCount));
        //        featuers_list = new double[total_frames - 2][];


        //        frame = cap.QueryFrame();
        //        prev_frame = frame;

        //        Console.WriteLine(total_frames);

        //    }
        //    catch (NullReferenceException except)
        //    {
        //        Console.WriteLine(except.Message);
        //    }
        //    //17900
        //    while (mm < total_frames - 2)
        //    {

        //        try
        //        {
        //            prev_frame = frame;
        //            frame = cap.QueryFrame();

        //            Bitmap image = new Bitmap(frame.Bitmap);

        //            // Create a new FAST Corners Detector
        //            FastCornersDetector fast = new FastCornersDetector()
        //            {
        //                Suppress = true, // suppress non-maximum points
        //                Threshold = 70   // less leads to more corners
        //            };

        //            // Process the image looking for corners
        //            List<IntPoint> points = fast.ProcessImage(image);

        //            // Create a filter to mark the corners
        //            PointsMarker marker = new PointsMarker(points);

        //            // Apply the corner-marking filter
        //            Bitmap markers = marker.Apply(image);

        //            // Show on the screen
        //            //Accord.Controls.ImageBox.Show(markers);

        //            // Use it to extract interest points from the Lena image:
        //            List<IntPoint> descriptors = fast.ProcessImage(image);
        //            PointF[] features = new PointF[descriptors.Count];

        //            int c = 0;
        //            foreach (IntPoint p in descriptors)
        //            {
        //                features[c] = new PointF(p.X, p.Y);
        //                c++;
        //            }

        //            ImageViewer viewer = new ImageViewer();

        //            Image<Gray, Byte> prev_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
        //            Image<Gray, Byte> curr_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
        //            curr_grey_img = frame.ToImage<Gray, byte>();
        //            prev_grey_img = prev_frame.ToImage<Gray, Byte>();

        //            PointF[] shiftedFeatures;
        //            Byte[] status;
        //            float[] trackErrors;

        //            CvInvoke.CalcOpticalFlowPyrLK(prev_grey_img, curr_grey_img, features, new Size(9, 9), 3, new MCvTermCriteria(20, 0.05),
        //               out shiftedFeatures, out status, out trackErrors);



        //            //Image<Gray, Byte> displayImage = cap.QueryFrame().ToImage<Gray, Byte>();
        //            //for (int i = 0; i < features.Length; i++)
        //            //    displayImage.Draw(new LineSegment2DF(features[i], shiftedFeatures[i]), new Gray(), 2);


        //            for (int i = 0; i < features.Length; i++)
        //            {
        //                CvInvoke.Circle(frame, System.Drawing.Point.Round(shiftedFeatures[i]), 4, new MCvScalar(0, 255, 255), 2);
        //            }

        //            double mean_X = 0;
        //            double mean_Y = 0;

        //            foreach (PointF p in shiftedFeatures)
        //            {
        //                mean_X += p.X;
        //                mean_Y += p.Y;
        //            }

        //            mean_X /= shiftedFeatures.Length;
        //            mean_Y /= shiftedFeatures.Length;



        //            double[] inner = new double[] { mean_X, mean_Y };
        //            featuers_list[mm] = inner;




        //            //viewer.Image = frame;
        //            //viewer.ShowDialog();
        //            //prev_frame = frame;

        //            //Console.WriteLine("frame:{0} " + mm);
        //            Console.WriteLine("frame:{0} " + mm + "  X:{1} " + mean_X + "   Y:{2} " + mean_Y);

        //            mm++;
        //        }
        //        catch (Exception e)
        //        { Console.WriteLine(e.Message); }

        //    }
        //    //int go = 0;
        //    //foreach (double[] arr in featuers_list)
        //    //{
        //    //    Console.Write("frame:{0} ", go++);
        //    //    foreach (double d in arr)
        //    //        Console.Write(d + "    ");

        //    //    Console.WriteLine();
        //    //}
        //    Serialize.SerializeObject(featuers_list, save_path);

        //}



    }
}
