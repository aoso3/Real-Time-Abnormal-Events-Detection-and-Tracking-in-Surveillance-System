using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Threading;
using Telerik.WinControls;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Accord.MachineLearning;
using real_time_abnormal_event_detection_and_tracking_in_video;
using Accord.IO;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Models.Regression;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.Bayes;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov;


namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    public partial class Player : Telerik.WinControls.UI.RadForm
    {
        private VideoCapture _capture1 = null;
        public static string vid1 = "";
        int fbs, total_frames1;
        Mat current_frame1;
        int vid1_frame = 0;
        public static bool play = true;
        

        public Player()
        {
            InitializeComponent();
           
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }



        private void radButton4_Click(object sender, EventArgs e)
        {
            String Pridect_path = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\pridect.txt";
            String Target_path = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\target.txt";
            FowlkesMallowsIndex FMI = new FowlkesMallowsIndex();
            MessageBox.Show(FMI.Evaluate(Pridect_path, Target_path).ToString());

        }




        public async void start(String video) 
        {
          
            try
            {

                _capture1 = new VideoCapture(video);
                total_frames1 = Convert.ToInt32(_capture1.GetCaptureProperty(CapProp.FrameCount));
                fbs = Convert.ToInt32(_capture1.GetCaptureProperty(CapProp.Fps));
                current_frame1 = new Mat();

                if (_capture1 == null)
                    return;

                try
                {
                    while (vid1_frame < total_frames1 && play)
                    {
                        //mm++;
                        //imageBox1.Image = _capture.QueryFrame();

                        _capture1.SetCaptureProperty(CapProp.PosFrames, vid1_frame);
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        _capture1.Read(current_frame1);
                        pictureBox1.Image = current_frame1.Bitmap;
                        vid1_frame++;
                        //current_frame_num1 += 1;
                        await Task.Delay(1);


                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
          
        }

        private void radButton5_Click(object sender, EventArgs e)
        {
            vid1_frame = 0;
            play = false;
            pictureBox1.Image = null;
            Owner.Enabled = true;
            this.Close();
        }



    }
}
