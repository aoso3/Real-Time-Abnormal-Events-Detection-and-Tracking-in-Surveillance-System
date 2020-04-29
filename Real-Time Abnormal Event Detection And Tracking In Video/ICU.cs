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
using Emgu.CV.Cvb;
using Emgu.CV.UI;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.Cuda;
using real_time_abnormal_event_detection_and_tracking_in_video;
using Accord.MachineLearning;
using Accord.IO;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Models.Regression;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.Bayes;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov;
using System.Runtime.InteropServices;
using WMPLib;


namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    public partial class ICU : Telerik.WinControls.UI.RadForm
    {
        Emgu.CV.Structure.MCvTermCriteria _MCvTermCriteria = new Emgu.CV.Structure.MCvTermCriteria(30, 30);
        VideoSurveilance.Camera camera_1;
        VideoSurveilance.Camera camera_2;
        private VideoCapture _capture1 = null;
        private VideoCapture _capture2 = null;
        AbnormalDetection abnoraml1, abnoraml2;
        string vid1 = "";
        string vid2 = "";
        string train_vid;
        string codewards = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\codewords.txt";
        int fbs, current_frame_num1, current_frame_num2, total_frames1, total_frames2;
        Mat current_frame1, current_frame2;
        Thread t1, t2, p1, p2;
        bool play;
        FeaturesExtraction F_E;
        KNearestNeighbors knn;
        RandomForest RF;
        LogisticRegression LR;
        SupportVectorMachine<Gaussian> SVM;
        NaiveBayes NB;
        HiddenMarkovModel HMM;
        bool alarm = false, violence = true, cover_camera = true, chocking = true, lying = true, running = true, motion = false;
        WMPLib.WindowsMediaPlayer wplayer;
        bool player1 = false, player2 = false;
        String path = "H:\\Github Projects\\Real-Time-Abnormal-Event-Detection-And-Tracking-In-Video";
        int count = 0;
        int MIM_count = 0;
        int hmm_count = 0;
        double abnormal_vote = 0;
        double normal_vote = 0;
        double hmm_abnormal_vote = 0;
        double hmm_normal_vote = 0;

        public ICU()
        {
            InitializeComponent();
            current_frame_num1 = 0;
            current_frame_num2 = 0;
            F_E = new FeaturesExtraction();
            knn = Serializer.Load<KNearestNeighbors>(Path.Combine(path, "knn7.bin"));
            RF = Serializer.Load<RandomForest>(Path.Combine(path, "RF7.bin"));
            LR = Serializer.Load<LogisticRegression>(Path.Combine(path, "LR7.bin"));
            SVM = Serializer.Load<SupportVectorMachine<Gaussian>>(Path.Combine(path, "SVM7.bin"));
            NB = Serializer.Load<NaiveBayes>(Path.Combine(path, "NB7.bin"));
            HMM = Serializer.Load<HiddenMarkovModel>(Path.Combine(path, "HMM_seq7.bin"));
            dataGridView1.RowTemplate.Height = 120;
            ((DataGridViewImageColumn)dataGridView1.Columns[0]).ImageLayout = DataGridViewImageCellLayout.Stretch;
            dataGridView1.Columns[1].Visible = false;

        }

        private void ICU_Load(object sender, EventArgs e)
        {

        }
       

        private void Video1_Proccess1()
        {
            //if (_capture1 != null && _capture1.Ptr != IntPtr.Zero)
            //{

            int war_at_frame = 0;
            bool warning = false;
            while (camera_1.frameNum < total_frames1 - 10)
            {
                //Console.WriteLine(camera_1.frameNum);
                if (camera_1.frameNum % 20 == 0)
                    count = 0;

                abnormal_vote = 0;
                normal_vote = 0;
                try
                {

                    double[] fe = F_E.extract(vid1, camera_1.frameNum);
                    if (fe[0] == null || fe[1] == null)
                    {
                        fe[0] = 240;
                        fe[0] = 170;

                    }
                    int[] fff = new int[] { (int)fe[0], (int)fe[1] };

                    //int knn_answer = knn.Decide(fe);
                    int RF_answer = RF.Decide(fe);
                    bool LR_answer = LR.Decide(fe);
                    //bool SVM_answer = SVM.Decide(fe);
                    int NB_answer = NB.Decide(fff);
                    double fl1 = HMM.LogLikelihood(fff);

                    if (chocking || lying)
                    {
                        Console.WriteLine(fl1);
                        if (fl1.CompareTo(-8.3) == 1)
                            hmm_count++;

                    }


                    else if (violence)
                    {

                        if (RF_answer == 1)
                            abnormal_vote += 0.978546619845336;
                        else
                            normal_vote += 0.978546619845336;

                        if (LR_answer)
                            abnormal_vote += 0.8428031393318365;
                        else
                            normal_vote += 0.8428031393318365;

                        if (NB_answer == 1)
                            abnormal_vote += 0.8746569953754341;
                        else
                            normal_vote += 0.8746569953754341;

                        if (abnormal_vote.CompareTo(normal_vote) == 1)
                            count++;
                    }

                    if (hmm_count >= 2 || count >= 4)
                    {
                        if (count >= 4)
                            count = 0;
                        if (hmm_count >= 2)
                            hmm_count = 0;

                        this.pictureBox3.Invoke((MethodInvoker)delegate
                        {
                            // Running on the UI thread
                            pictureBox3.Image = Properties.Resources.warning;
                        });

                        if (alarm)
                        {
                            wplayer.URL = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\Alarm.mp3";
                            wplayer.controls.play();
                        }



                        //pictureBox3.Image = Properties.Resources.warning;
                        warning = true;
                        war_at_frame = camera_1.frameNum;

                        Media.Crop_video(vid1, (int)camera_1.frameNum / (fbs + 5), 30);
                        Media.thumbnail(vid1, (int)camera_1.frameNum / (fbs + 5));
                        Image image = Image.FromFile(@"D:\2\Real-Time Abnormal Event Detection And Tracking In Video\croped_videos\crop" + Media.num.ToString() + ".jpg");
                        dataGridView1.Rows.Add(image, @"D:\2\Real-Time Abnormal Event Detection And Tracking In Video\croped_videos\crop" + Media.num.ToString() + ".mpg");
                        Media.num++;

                    }

                    if (warning && camera_1.frameNum >= (war_at_frame + 10))
                    {
                        this.pictureBox3.Invoke((MethodInvoker)delegate
                        {
                            // Running on the UI thread
                            pictureBox3.Image = Properties.Resources._checked;
                        });
                        //pictureBox3.Image = Properties.Resources._checked;
                        warning = false;
                    }
            
                }
                catch (Exception e)
                {
                    Console.WriteLine("1--- ", e.Message);
                }


            }

        }


        private void Video1_Proccess2()
        {
            //if (_capture1 != null && _capture1.Ptr != IntPtr.Zero)
            //{

            int war_at_frame = 0;
            bool warning = false;
            while (camera_1.frameNum < total_frames1 - 10)
            {
                try
                {
                    if (cover_camera || running || motion)
                    {
                        abnoraml1 = new AbnormalDetection(vid1, codewards, camera_1.frameNum);
                        double[][][] minDistMatrix = abnoraml1.Detect();

                        for (int i = 0; i < 2; i++)
                        {

                            double max = -99;
                            for (int j = 0; j < abnoraml1.row; j++)
                                if (Math.Abs(minDistMatrix[i][j].Max()) > max)
                                    max = minDistMatrix[i][j].Max();


                            //Console.WriteLine("max {0} ",max);
                            if ((max.CompareTo(AbnormalDetection.threshold) == 1) || (motion && max > 0))
                            {
                                MIM_count++;
                                //Console.WriteLine(max);
                            }
                        }


                        if ((MIM_count >= 1))
                        {
                            MIM_count = 0;
                            this.pictureBox3.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                pictureBox3.Image = Properties.Resources.warning;
                            });
                            //pictureBox3.Image = Properties.Resources.warning;
                            warning = true;
                            war_at_frame = camera_1.frameNum;

                            if (alarm)
                            {
                                wplayer.URL = "D:\\2\\Real-Time Abnormal Event Detection And Tracking In Video\\Alarm.mp3";
                                wplayer.controls.play();
                            }

                            Media.Crop_video(vid1, (int)camera_1.frameNum / (fbs + 5), 30);
                            Media.thumbnail(vid1, (int)camera_1.frameNum / (fbs + 5));
                            Image image = Image.FromFile(@"D:\2\Real-Time Abnormal Event Detection And Tracking In Video\croped_videos\crop" + Media.num.ToString() + ".jpg");
                            dataGridView1.Rows.Add(image, @"D:\2\Real-Time Abnormal Event Detection And Tracking In Video\croped_videos\crop" + Media.num.ToString() + ".mpg");
                            Media.num++;
                        }

                        if (warning && camera_1.frameNum >= (war_at_frame + 10))
                        {
                            this.pictureBox3.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                pictureBox3.Image = Properties.Resources._checked;
                            });
                            //pictureBox3.Image = Properties.Resources._checked;
                            warning = false;
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("2--- ", e.Message);
                }


            }

        }






        private async void play_video1()
        {
            //if (_capture1 == null) 
            //    return;


            try
            {
                lock (camera_1)
                {
                    camera_1.PlayVideo();

                    if (camera_1.track == true)
                    {
                        camera_1.start_tracking();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public int vid2_frame = 0;

        private async void play_video2()
        {
            if (_capture2 == null)
                return;


            try
            {
                while (vid2_frame < total_frames2 && play)
                {
                    //mm++;
                    //imageBox1.Image = _capture.QueryFrame();

                    _capture2.SetCaptureProperty(CapProp.PosFrames, vid2_frame);
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    _capture2.Read(current_frame2);
                    pictureBox2.Image = current_frame2.Bitmap;
                    //current_frame_num2 += 1;
                    vid2_frame++;
                    await Task.Delay(700 / fbs);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void radButton1_Click(object sender, EventArgs e)
        {

            //CvInvoke.UseOpenCL = true;
            if (radCheckBox1.Checked)
                violence = true;
            else
                violence = false;
            if (radCheckBox2.Checked)
                cover_camera = true;
            else
                cover_camera = false;
            if (radCheckBox3.Checked)
                chocking = true;
            else
                chocking = false;
            if (radCheckBox4.Checked)
                lying = true;
            else
                lying = false;
            if (radCheckBox5.Checked)
                running = true;
            else
                running = false;
            if (radCheckBox6.Checked)
                motion = true;
            else
                motion = false;

            if (play)
            {
                t1.Abort();
                t2.Abort();
                p1.Abort();
                //p2.Abort();

                camera_1.frameNum = 0;
                vid2_frame = 0;
            }
            if (vid1 != "" && vid2 != "")
            {
                play = true;
                try
                {
                    if (vid1 != "" && vid1 != null && vid2 != "" && vid2 != null)
                    {
                        _capture1 = new VideoCapture(vid1);
                        //_capture2 = new VideoCapture(vid2);
                        total_frames1 = Convert.ToInt32(_capture1.GetCaptureProperty(CapProp.FrameCount));
                        //total_frames2 = Convert.ToInt32(_capture2.GetCaptureProperty(CapProp.FrameCount));
                        fbs = Convert.ToInt32(_capture1.GetCaptureProperty(CapProp.Fps));
                        current_frame1 = new Mat();
                        //current_frame2 = new Mat();
                        current_frame_num1 = 0;
                        //current_frame_num2 = 0;
                        //Application.Idle += ProcessFrame;
                        t1 = new Thread(Video1_Proccess1);
                        t2 = new Thread(Video1_Proccess2);
                        t1.Start();
                        t2.Start();
                        //proccess();
                        p1 = new Thread(play_video1);
                        //p2 = new Thread(play_video2);
                        p1.Start();

                        //p2.Start();
                    }
                }
                catch (Exception excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            else
            {
                MessageBox.Show("Please choose 2 videos!");
            }

        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            //reset();
            player1 = true;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                vid1 = openFileDialog1.FileName;
                camera_1 = new VideoSurveilance.Camera(openFileDialog1.FileName, pictureBox1);
            }

            pictureBox3.Image = Properties.Resources._checked;

        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            //reset();
            player1 = true;

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                vid2 = openFileDialog1.FileName;
                camera_2 = new VideoSurveilance.Camera(openFileDialog1.FileName, pictureBox2);
                camera_2.setMargins(120, 0, 150, 0);
                camera_1.setDownCamera(camera_2);
            }

            pictureBox4.Image = Properties.Resources._checked;

        }

        private void radButton4_Click(object sender, EventArgs e)
        {
            if (player1)
                camera_1.StopPlay();
            if (player2)
                camera_2.StopPlay();

            if (play)
            {
                t1.Abort();
                t2.Abort();
                p1.Abort();
                //p2.Abort();
            }
            try
            {
                Application.Exit();
            }
            catch (Exception eee)
            {
                Environment.Exit(Environment.ExitCode);

            }
        }
        private void reset()
        {
            play = false;
            t1.Abort();
            t2.Abort();
            p1.Abort();
            //p2.Abort();


            camera_1.frameNum = 0;
            vid2_frame = 0;
            pictureBox1.Image = null;
            pictureBox2.Image = null;
        }
        private void radButton5_Click(object sender, EventArgs e)
        {
            camera_1.StopPlay();
            camera_2.StopPlay();
            if (play)
                reset();

        }

        private void radButton6_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
                train_vid = openFileDialog1.FileName;

            if (train_vid != "" && train_vid != null)
            {
                Clustering clus = new Clustering();
                clus.cluster_video(train_vid, codewards);
            }

        }

        private void radButton6_Click_1(object sender, EventArgs e)
        {
            if (!alarm)
            {
                alarm = true;
                radButton6.Image = Properties.Resources.green_alarm2;
                radLabel2.Text = "Alarm On";
            }
            else
            {
                alarm = false;
                radButton6.Image = Properties.Resources.red_alarm2;
                radLabel2.Text = "Alarm Off";

            }

        }

        private void radCheckBox1_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            violence = (violence == true) ? false : true;

        }

        private void radCheckBox2_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            cover_camera = (cover_camera == true) ? false : true;

        }

        private void radCheckBox3_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            chocking = (chocking == true) ? false : true;

        }

        private void radCheckBox4_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            lying = (lying == true) ? false : true;
        }

        private void radCheckBox5_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            running = (running == true) ? false : true;

        }

        private void radCheckBox6_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            motion = (motion == true) ? false : true;

        }

        private void radGridView1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            camera_1.StopPlay();
            camera_2.StopPlay();
            var value = dataGridView1.Rows[e.RowIndex].Cells[1].Value;
            Player.play = true;
            Player form = new Player() { Owner = this };
            form.Show();
            this.Enabled = false;
            form.start(value.ToString());
        }


    }
}