using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Runtime.InteropServices;


namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    /// <summary>
    /// Calculate optical flow of the processed frames.
    /// </summary>
    class OptFlowOfBlocks
    {

        /// <summary>
        /// Calculating Optical-Flow of each block After dividing the frames into blocks,
        /// we compute optical-flow of each block by computing the average of optical-flows
        /// of all the pixels constituting a block
        /// </summary>
        /// <param name="mag">Magnitude of the block.</param>
        /// <param name="angle">Angle of the block.</param>
        /// <param name="grayImg">The frame we are processing.</param>
        /// <param name="opFlowOfBlocks">'Out' Optical flow of the blocks.</param>
        /// <param name="centreOfBlocks">'Out' Centers of the block.</param>
        /// <param name="rows">'Out' Height of the frame.</param>
        /// <param name="cols">'Out' Width of the frame.</param>
        /// <param name="noOfRowInBlock">'Out' Number of rows of the mega block.</param>
        /// <param name="noOfColInBlock">'Out' Number of columns of the mega gird.</param>
        /// <param name="xBlockSize">'Out' Horizontal size of the block of the block.</param>
        /// <param name="yBlockSize">'Out' Vertical size of the block of the block.</param>
        /// <param name="blockSize">'Out' Size of the created block.</param>
        /// <returns></returns>
        public static void calcOptFlowOfBlocks(Mat mag, Mat angle, Image<Gray, Byte> grayImg,out double[][][] opFlowOfBlocks,
                                               out double[][][] centreOfBlocks, out int rows, out int cols,out int noOfRowInBlock,
                                               out int noOfColInBlock, out int xBlockSize, out int yBlockSize,out int blockSize){
            
            double val = 0;
            double deg_threshold = 337.5;
            rows = grayImg.Height;
            cols = grayImg.Width;
            noOfRowInBlock = 8;
            noOfColInBlock = 8;
            xBlockSize = rows / noOfRowInBlock + 1;
            yBlockSize = cols / noOfColInBlock + 1;
            blockSize = noOfRowInBlock * noOfColInBlock;
            opFlowOfBlocks = new double[xBlockSize][][]; 
            centreOfBlocks = new double[xBlockSize][][]; 

            for (int r = 0; r < xBlockSize; r++)
            {
                opFlowOfBlocks[r] = new double[yBlockSize][];
                centreOfBlocks[r] = new double[yBlockSize][];

                for (int rr = 0; rr < yBlockSize; rr++)
                {
                    opFlowOfBlocks[r][rr] = new double[2];
                    centreOfBlocks[r][rr] = new double[2];
                }

            }

                for (int i = 0; i < mag.Height; i++)
                    for (int j = 0; j < mag.Width; j++)
                    {
                        double[] mag_value = new double[1];
                        Marshal.Copy(mag.DataPointer + (i * mag.Cols + j) * mag.ElementSize, mag_value, 0, 1);
                        opFlowOfBlocks[i / noOfRowInBlock][j / noOfColInBlock][0] += mag_value[0];
                        double[] angle_value = new double[1];
                        Marshal.Copy(angle.DataPointer + (i * angle.Cols + j) * angle.ElementSize, angle_value, 0, 1);
                        opFlowOfBlocks[i / noOfRowInBlock][j / noOfColInBlock][1] += angle_value[0];
                    }

            for (int i = 0; i < xBlockSize; i++)
                for (int j = 0; j < yBlockSize; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        opFlowOfBlocks[i][j][k] /= noOfRowInBlock * noOfColInBlock;
                        val = opFlowOfBlocks[i][j][k];

                        double opt = 0;
                        if( k == 1 )
                        {
                            double angInDeg = val * (180 / Math.PI);
                            if (angInDeg > deg_threshold)
                                opt = 0;
                            else
                            {
                                double q = Math.Floor(angInDeg / 22.5);
                                double a1 = q * 22.5;
                                double q1 = (int)(angInDeg - a1);
                                double a2 = (q + 2) * 22.5;
                                double q2 = (a2 - angInDeg);
                                
                                if (q1.CompareTo(q2) == -1)
                                    opt = Math.Round(a1 / 45);
                                else
                                    opt = Math.Round(a2 / 45);

                            }
                            opFlowOfBlocks[i][j][k] = opt;
                            double theta = val;
                        }
                        if (k == 1)
                        {
                            double r = val;
                            double x = ((i + 1) * noOfRowInBlock) - (noOfRowInBlock / 2);
                            double y = ((j + 1) * noOfColInBlock) - (noOfColInBlock / 2);
                            centreOfBlocks[i][j][0] = x;
                            centreOfBlocks[i][j][1] = y;
                        }


                    } 
        }

    }
}
