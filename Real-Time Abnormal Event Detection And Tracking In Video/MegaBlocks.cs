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
    class MegaBlocks
    {

        public double[][][][] createMegaBlocks(List<double[][][]> motionInfoOfFrames, int xBlockSize, int yBlockSize, int noOfRows, int noOfCols, int total_frames)
        {
            int n = 2;
            int frameCounter = 0;
            //double[][][][] megaBlockMotInfVal = new double[xBlockSize / n + 1][yBlockSize / n + 1][motionInfoOfFrames.Count][8];
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
                        //double[] left = megaBlockMotInfVal[(int)i / n][(int)j / n][frameCounter];
                        //var products = left.Zip(right, (m, n) => m * n);
                        //double[] sum = new double[8];
                        for (int k = 0; k < 8; k++)
                        {
                            megaBlockMotInfVal[(int)i / n][(int)j / n][frameCounter][k] += frame[i][j][k];

                            //Console.WriteLine("K = {0} || {1}", k, megaBlockMotInfVal[(int)i / n][(int)j / n][frameCounter][k]);
                        }
                        //megaBlockMotInfVal[(int)i / n, (int)j / n, frameCounter] = sum;                                            
                    }

                frameCounter++;
            }
            return megaBlockMotInfVal;

        }

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

                    //count = 0;
                    //for (int q = 0; q < 5; q++)
                    //{
                    //    //Console.WriteLine("Cluster: {0}", count++);
                    //    //for (int w = 0; w < 8; w++)
                    //    Console.WriteLine(centriods[q][0]);
                    //}

                    codewords[i][j] = centriods;
                         
                }

            return codewords;

        }


    }

}