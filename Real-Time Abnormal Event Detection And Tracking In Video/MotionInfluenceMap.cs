using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Drawing;
using System.Runtime.InteropServices;



namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    class MotionInfluenceMap
    {

        Mat frame,prev_frame, mag, ang;
        VideoCapture cap = null;
        int rows, cols, noOfRowInBlock, noOfColInBlock, xBlockSize, yBlockSize, blockSize;
        double[][][] opFlowOfBlocks, centreOfBlocks, motionInfVal;

        public void motionInMapGenerator(double[][][] opFlowOfBlocks, int blockSize, double[][][] centreOfBlocks, int xBlockSize, int yBlockSize, out double[][][] motionInfVal)
        {
            motionInfVal = new double[xBlockSize][][];//yBlockSize, 8
            
            for (int r = 0; r < xBlockSize; r++)
            {
                motionInfVal[r] = new double[yBlockSize][];
                for (int rr = 0; rr < yBlockSize; rr++)
                    motionInfVal[r][rr] = new double[8];
               
            }

            double x1, y1, x2, y2, slope;

            for (int i = 0; i < xBlockSize; i++)
                for (int j = 0; j < yBlockSize; j++)
                {
                    double Td = getThresholdDistance(opFlowOfBlocks[i][j][0], blockSize) ;
                    double k = opFlowOfBlocks[i][j][1];
                    double posFi, negFi ;
                    double ang = (45 * k) * ( Math.PI / 180);
                    getThresholdAngle( ang,out posFi,out negFi);

                    for (int u = 0; u < xBlockSize; u++)
                        for (int p = 0; p < yBlockSize; p++)
                        {
                            getCentreOfBlock(i, j, u, p, centreOfBlocks,out x1,out y1, out x2, out y2, out slope);
                            double euclideanDist = calcEuclideanDist(x1, y1, x2, y2);

                             if (euclideanDist < Td){
                                double angWithXAxis = Math.Atan(slope);
                                double a1 = (45 * k) * (Math.PI / 180);
                                double angBtwTwoBlocks = angleBtw2Blocks(a1, angWithXAxis);

                                 if (negFi < angBtwTwoBlocks && angBtwTwoBlocks < posFi)
                                     motionInfVal[u][p][(int)opFlowOfBlocks[i][j][1]] += Math.Exp(-1 * (euclideanDist / opFlowOfBlocks[i][j][0]));
                             }

                        }          

                 }


        }


        public List<double[][][]> get_motion_influence_map(String vid, out int xBlockSize, out int yBlockSize, out int noOfRowInBlock, out int noOfColInBlock, out int total_frames, bool clustering, int frame_nr = 0)
        {
            List<double[][][]> ret = new List<double[][][]>();
            xBlockSize = 0;
            yBlockSize = 0;
            noOfRowInBlock = 0 ;
            noOfColInBlock = 0;
            total_frames = 0;

            try
            {
                mag = new Mat();
                ang = new Mat();
                frame = new Mat();
                prev_frame = new Mat();
                cap = new VideoCapture(vid);
                if (!clustering)
                    total_frames = 3;
                else
                    total_frames = Convert.ToInt32(cap.GetCaptureProperty(CapProp.FrameCount));
                cap.SetCaptureProperty(CapProp.PosFrames, frame_nr);
                frame = cap.QueryFrame();
                prev_frame = frame;
            }
            catch (NullReferenceException except)
            {
               Console.WriteLine(except.Message);
            }

            int mm = 0;
            Console.WriteLine("Total Frames : {0}", total_frames);
            
            while (mm < total_frames - 1)
            {

                Console.WriteLine("Frame : " + frame_nr);
                prev_frame = frame;
                frame_nr += 1;
                cap.SetCaptureProperty(CapProp.PosFrames, frame_nr);
                frame = cap.QueryFrame();

                //int txt_resize_factor = 2;

                //Size n_size = new Size(frame.Width / Convert.ToInt32(txt_resize_factor),
                //        frame.Height / Convert.ToInt32(txt_resize_factor));

                //CvInvoke.Resize(frame, frame, n_size);
                //CvInvoke.Resize(prev_frame, prev_frame, n_size);

                Image<Gray, Byte>  prev_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);
                Image<Gray, Byte>  curr_grey_img = new Image<Gray, byte>(frame.Width, frame.Height);

                Image<Gray, float> flow_x = new Image<Gray, float>(frame.Width, frame.Height);
                Image<Gray, float> flow_y = new Image<Gray, float>(frame.Width, frame.Height);

                curr_grey_img = frame.ToImage<Gray, byte>();
                prev_grey_img = prev_frame.ToImage<Gray, Byte>();


                CvInvoke.CalcOpticalFlowFarneback(prev_grey_img, curr_grey_img, flow_x, flow_y, 0.5, 3, 15, 3, 6, 1.3, 0);
                CvInvoke.CartToPolar(flow_x, flow_y, mag, ang);                

                OptFlowOfBlocks.calcOptFlowOfBlocks(mag, ang, curr_grey_img,out opFlowOfBlocks ,out centreOfBlocks,
                out rows, out cols, out noOfRowInBlock, out noOfColInBlock,out xBlockSize,out yBlockSize,out blockSize);
                motionInMapGenerator(opFlowOfBlocks, blockSize, centreOfBlocks, xBlockSize, yBlockSize, out motionInfVal);

                ret.Add(motionInfVal);

                //prev_grey_img.Dispose();
                //curr_grey_img.Dispose();
                //flow_x.Dispose();
                //flow_y.Dispose();
                mm++;

            }
            return ret;

        }




    private double getThresholdDistance(double mag, double blockSize){
        return mag * blockSize;
    }
    private void getThresholdAngle(double ang,out double pos,out double neg)
    {
        double tAngle = Math.PI / 2;
        pos = ang + tAngle;
        neg = ang - tAngle;
    }
  
    private void getCentreOfBlock(int index1_i, int index1_j,int index2_i,int index2_j , double[][][] centreOfBlocks,
        out double x1,out double y1,out double x2,out double y2,out double slope){
        x1 = centreOfBlocks[index1_i][index1_j][0];
        y1 = centreOfBlocks[index1_i][index1_j][1];
        x2 = centreOfBlocks[index2_i][index2_j][0];
        y2 = centreOfBlocks[index2_i][index2_j][1];
        slope = (x1 != x2) ? (y2 - y1) / (x2 - x1) : float.PositiveInfinity; 
        
    }

    private double calcEuclideanDist(double x1,double y1,double x2,double y2){
        return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    private double angleBtw2Blocks(double ang1,double ang2){
        if(ang1 - ang2 < 0){
            double ang1InDeg = ang1 * (180 / Math.PI);
            double ang2InDeg = ang2 * (180 / Math.PI);
            return (360 - (ang1InDeg - ang2InDeg)) * (Math.PI / 180) ;
        }

        return ang1 - ang2 ;
    }


    }
}
