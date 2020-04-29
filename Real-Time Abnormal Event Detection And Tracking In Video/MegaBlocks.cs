using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;
using System.Threading;
using Accord.Math.Comparers;
using Accord.MachineLearning;
using System.IO;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    /// <summary>
    /// Create meaga blocks from the processed frames.
    /// </summary>
    class MegaBlocks
    {
        /// <summary>
        /// Creating Megablocks Frames are partitioned into non-overlapping mega blocks,
        /// each of which is a combination of multiple motion influence blocks.
        /// </summary>
        /// <param name="motionInfoOfFrames">The resulting motion influence map of the processed frames.</param>
        /// <param name="xBlockSize">Horizontal size of the block of the mega block.</param>
        /// <param name="yBlockSize">Vertical size of the block of the mega block.</param>
        /// <param name="noOfRows">Number of rows of the mega block grid.</param>
        /// <param name="noOfCols">Number of columns of the mega block gird.</param>
        /// <param name="total_frames">Total frames that we will process.</param>
        /// <returns>Mega blocks array of the frames.</returns>
        public double[][][][] createMegaBlocks(List<double[][][]> motionInfoOfFrames, int xBlockSize, int yBlockSize, int noOfRows, int noOfCols, int total_frames)
        {
            int n = 2;
            int frameCounter = 0;
            double[][][][] megaBlockMotInfVal = new double[xBlockSize / n + 1][][][];

            for (int r = 0; r < xBlockSize / n + 1; r++)
            {
                megaBlockMotInfVal[r] = new double[yBlockSize / n + 1][][];

                for (int rr = 0; rr < yBlockSize / n + 1; rr++)
                {
                    megaBlockMotInfVal[r][rr] = new double[total_frames][];
                    for (int rrr = 0; rrr < total_frames; rrr++)
                        megaBlockMotInfVal[r][rr][rrr] = new double[8];

                }
            }

            foreach (double[][][] frame in motionInfoOfFrames)
            {
                for (int i = 0; i < xBlockSize; i++)
                    for (int j = 0; j < yBlockSize; j++)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            megaBlockMotInfVal[(int)i / n][(int)j / n][frameCounter][k] += frame[i][j][k];
                        }
                    }

                frameCounter++;
            }
            return megaBlockMotInfVal;

        }

        /// <summary>
        /// Applying k-means clustering algorithm on the resulting mega blocks and return
        /// the centers of the normal clusters.
        /// </summary>
        /// <param name="megaBlockMotInfVal">The resulting mega blocks array of the frames of the processed frames.</param>
        /// <param name="xBlockSize">Horizontal size of the block of the mega block.</param>
        /// <param name="yBlockSize">Vertical size of the block of the mega block.</param>
        /// <param name="total_frames">Total frames that we will process.</param>
        /// <returns>Centers of the normal clusters.</returns>
        public double[][][][] kmeans(double[][][][] megaBlockMotInfVal, int xBlockSize, int yBlockSize, int total_frames)
        {
            int count = 0;
            int cluster_n = 5;
            int n = 2;
            int row = xBlockSize / n + 1;
            int col = yBlockSize / n + 1;
            double[][][][] codewords = new double[row][][][];
            double[][] centriods = new double[cluster_n][];

            for (int r = 0; r < row; r++)
            {
                codewords[r] = new double[col][][];

                for (int rr = 0; rr < col; rr++)
                {
                    codewords[r][rr] = new double[cluster_n][];
                    for (int rrr = 0; rrr < cluster_n; rrr++)
                        codewords[r][rr][rrr] = new double[8];

                }

            }

            for (int i = 0; i < row / n + 1; i++)
                for (int j = 0; j < col / n + 1; j++)
                {

                    Accord.Math.Random.Generator.Seed = 0;
                    double[][] observations = megaBlockMotInfVal[i][j];
                    KMeans kmeans = new KMeans(k: 5);
                    var clusters = kmeans.Learn(observations);
   
                    centriods = clusters.Centroids;

                    codewords[i][j] = centriods;
                         
                }

            return codewords;

        }


    }

}