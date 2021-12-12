using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Tracking;

using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video;
using System.Threading.Tasks;

namespace VideoSurveilance
{
    class Camera
    {
        private PictureBox camera_imageBox;
        private static Emgu.CV.Tracking.Tracker tracker;
        private string camera_video_path;
        private VideoCapture _cameraCapture;
        private double FrameRate;
        private Mat first_frame;
        private Rectangle temp_rec_trucked;
        private Rectangle last_seen_frame_rec;
        Rectangle to_be_trucked;
        public int right_margin;
        public int left_margin;
        public int up_margin;
        public int down_margin;
        private Camera upCamera;
        private Camera downCamera;
        private Camera rightCamera;
        private Camera leftCamera;
        private bool nextCameraStarted = false;
        Thread trackThread;
        public int frameNum = 0;

        public Camera(string camera_video_path, PictureBox camera_imageBox)
        {
            this.camera_video_path = camera_video_path;
            this.camera_imageBox = camera_imageBox;

            try
            {
                _cameraCapture = new VideoCapture(camera_video_path);
                first_frame = _cameraCapture.QueryFrame();
                this.FrameRate = _cameraCapture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                this.camera_imageBox.SizeMode = PictureBoxSizeMode.StretchImage;
                this.camera_imageBox.Image = first_frame.Bitmap;
                this.camera_imageBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imageBox1_MouseDown);
                this.camera_imageBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imageBox1_MouseUp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void setTruckedRec(Rectangle TruckedRec)
        {
            this.to_be_trucked = TruckedRec;
        }

        public Rectangle getLasSeenFrameRectangle()
        {
            return last_seen_frame_rec == null ? to_be_trucked : last_seen_frame_rec;
        }

        public void setDownCamera(Camera camera)
        {
            this.downCamera = camera;
        }

        public void setMargins(int up_margin, int down_margin, int left_margin, int right_margin)
        {
            this.up_margin = up_margin;
            this.down_margin = down_margin;
            this.left_margin = left_margin;
            this.right_margin = right_margin;
        }


        public void start_tracking()
        {
            if (to_be_trucked != null)
            {
                ProcessFrame();

            }
            else
            {
                MessageBox.Show("Please Select a person to track first!");
            }
        }

        void ProcessFrame()
        {
            //tracker = new TrackerKCF();
            //tracker.Init(first_frame, to_be_trucked);
            temp_rec_trucked = to_be_trucked;
            if (!_cameraCapture.IsOpened)
            {
                MessageBox.Show("error with openning the file");
                return;
            }
            Mat frame = _cameraCapture.QueryFrame();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int camera_speed = (int)(800.0 / FrameRate);
            while (frame != null)
            {
                try
                {
                    watch = System.Diagnostics.Stopwatch.StartNew();
                    bool ok = tracker.Update(frame.Clone(), out temp_rec_trucked);
                    if (ok)
                    {
                        //frame.CopyTo(last_seen_frame);
                        last_seen_frame_rec = temp_rec_trucked;
                        CvInvoke.Rectangle(frame, temp_rec_trucked, new MCvScalar(255.0, 0, 0), 1);
                        camera_imageBox.Image = frame.Bitmap;
                    }
                    else
                    {
                        CvInvoke.PutText(frame, "Tracking failure detected", new Point(100, 50), FontFace.HersheyPlain, 1.0, new MCvScalar(255.0, 255.0, 255.0));
                        camera_imageBox.Image = frame.Bitmap;

                        //if (!nextCameraStarted)
                        //    checkTheNextCamera();
                    }
                    frame = _cameraCapture.QueryFrame();
                    frameNum++;
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    if (camera_speed - elapsedMs > 0)
                    {
                        //Thread.Sleep(camera_speed - (int)elapsedMs); 
                        Thread.Sleep(800 / (int)FrameRate);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }

        }

        public void find_person(Mat Frame_from_another_camera, Rectangle person_rec)
        {

            _cameraCapture = new VideoCapture(camera_video_path);
            Rectangle search_area = find_template_position(Frame_from_another_camera, person_rec);
            search_area = new Rectangle(
                    0, // Left
                    0, // Top
                    Frame_from_another_camera.Width / 2, // Width
                    Frame_from_another_camera.Height); // Height
            Rectangle person = find_person(_cameraCapture, new Mat(first_frame, to_be_trucked), search_area);

            temp_rec_trucked = person;
            //tracker = new TrackerKCF();
            //tracker.Init(_cameraCapture.QueryFrame(), person);

            //temp_rec_trucked = to_be_trucked;
            Thread thread = new Thread(delegate ()
            {
                ProcessFrame();
            });
            thread.Start();
        }



        int _pressedLocationX;
        int _pressedLocationY;
        int _releasedLocationX;
        int _releasedLocationY;

        private void imageBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _pressedLocationX = e.X;
            _pressedLocationY = e.Y;
        }

        private void imageBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _releasedLocationX = e.X;
            _releasedLocationY = e.Y;

            double wFact = first_frame.Width / (double)camera_imageBox.Size.Width;
            double hFact = first_frame.Height / (double)camera_imageBox.Size.Height;

            int LeftStart = (int)(Math.Min(_pressedLocationX, _releasedLocationX) * wFact);
            int TopStart = (int)(Math.Min(_pressedLocationY, _releasedLocationY) * hFact);
            int Width = (int)Math.Abs((_releasedLocationX - _pressedLocationX) * wFact);
            int Height = (int)Math.Abs((_releasedLocationY - _pressedLocationY) * hFact);

            to_be_trucked = new Rectangle(LeftStart, TopStart, Width, Height);
            Mat newFrame = first_frame.Clone();
            CvInvoke.Rectangle(newFrame, to_be_trucked, new MCvScalar(255.0, 255.0, 255.0), 1);
            camera_imageBox.Image = newFrame.Bitmap;

            if (camera_is_working)
            {
                track = true;
                camera_is_working = false;
            }
        }

        bool camera_is_working = false;
        public bool track = false;

        public async void PlayVideo()
        {
            if (_cameraCapture == null || !_cameraCapture.IsOpened)
            {
                MessageBox.Show("error with openning the file");
                return;
            }
            camera_is_working = true;
            first_frame = _cameraCapture.QueryFrame();
            camera_imageBox.Image = first_frame.Bitmap;

            while (first_frame != null && camera_is_working)
            {
                try
                {
                    first_frame = _cameraCapture.QueryFrame();
                    camera_imageBox.Image = first_frame.Bitmap;
                    frameNum++;
                    //await Task.Delay(700 / (int)FrameRate);
                    Thread.Sleep((int)(800.0 / FrameRate)); //This may result in fast playback if the codec does not tell the truth
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        public void StopPlay()
        {
            camera_is_working = false;
        }


        Rectangle find_template_position(Mat MainFrame, Rectangle template)
        {
            Rectangle search_area;

            int centerX = template.Location.X + template.Width / 2;
            int centerY = template.Location.Y + template.Height / 2;

            int MainCenterX = MainFrame.Width / 2;
            int MainCenterY = MainFrame.Height / 2;

            if (centerX < MainCenterX) //Left
            {
                return search_area = new Rectangle(
                      0, // Left
                     0, // Top
                      MainCenterX, // Width
                      MainFrame.Height); // Height
            }
            else //Right
            {

                return search_area = new Rectangle(
                      MainCenterX, // Left
                     0, // Top
                      MainCenterX, // Width
                      MainFrame.Height); // Height

            }
        }

        Rectangle find_person(VideoCapture camera, Mat PersonImage, Rectangle search_area)
        {


            Rectangle result = new Rectangle();
            int fps = Convert.ToInt16(camera.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps));
            int FrameCount = Convert.ToInt16(camera.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount));

            for (int i = 0; i < FrameCount; i = i + fps)
            {
                long matchTime;
                Mat frame = camera.QueryFrame();

                //camera_imageBox.Image = new Mat(frame, search_area);
                result = DrawMatches.Draw(new Mat(frame, search_area), PersonImage, out matchTime);
                if (result != null)
                    break;
            }
            return result;
        }


    }
}