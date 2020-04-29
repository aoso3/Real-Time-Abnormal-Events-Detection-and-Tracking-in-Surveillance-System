using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    /// <summary>
    /// The main class that uses Motion Influence Map and Mega Blocks to
    /// detect the abnormal events in the video.
    /// </summary>
    class AbnormalDetection
    {
        double[][][][] MegaBlock,codewords;
        List<double[][][]> MotionInfOfFrames;
        MotionInfluenceMap MIG;
        MegaBlocks MB;
        public int xBlockSize, yBlockSize, noOfRowInBlock, noOfColInBlock, n, row, col, cluster_n;
        int total_frames;
        public static double threshold = 380;
        double[][][] minDistMatrix;
        public int fram_num;


        /// <summary>
        /// Takes tha data and create motion influance map and mega blocks and process them
        /// so we can create min distination matrix and create array of centers of normal frames.
        /// </summary>
        /// <param name="video_path">Path of the video we want to process.</param>
        /// <param name="Codeword_path">Path of the centers of normal events in the train videos.</param>
        /// <param name="frame_number">the number of frame we want to start the processing from.</param>
        /// <returns></returns>
        public AbnormalDetection(String video_path, String Codeword_path, int frame_number)
        {
            fram_num = frame_number;
            MIG = new MotionInfluenceMap();
            MotionInfOfFrames = MIG.get_motion_influence_map(video_path, out xBlockSize, out yBlockSize, out noOfRowInBlock, out noOfColInBlock, out total_frames, false, fram_num);
            MB = new MegaBlocks();
            MegaBlock = MB.createMegaBlocks(MotionInfOfFrames, xBlockSize, yBlockSize, noOfRowInBlock, noOfColInBlock, (int)total_frames);
            n = 2;
            row = xBlockSize / n + 1;
            col = yBlockSize / n + 1;
            cluster_n = 5;
            int i = 0, j = 0, k = 0;
            codewords = new double[row][][][];
 

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

            minDistMatrix = new double[(int)total_frames][][];

            for (int r = 0; r < total_frames; r++)
            {
                minDistMatrix[r] = new double[row][];
                for (int rr = 0; rr < row; rr++)
                    minDistMatrix[r][rr] = new double[col];
            }


            using (StreamReader reader = new StreamReader(Codeword_path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    double[] arr = Array.ConvertAll(line.Split(' '), Double.Parse) ;
                    codewords[i][j][k] = arr;
                    k++;
                    if (k == cluster_n)
                    {
                        k = 0;
                        j++;
                        if (j == col) 
                        {
                            j = 0;
                            if(i<row-1)
                            i++;
                        }
                    }

                    
                }

            }
        }

        /// <summary>
        /// after extracting the spatio-temporal feature vectors for all mega blocks,
        /// we construct a minimum distance matrix over the mega blocks in which the
        /// value of an element is defined by the minimum Euclidean distance between
        /// a feature vector of the current test frame and the codewords in the corresponding
        /// mega block.
        /// </summary>
        /// <returns></returns>
        public double[][][] Detect()
        {

            for (int i = 0; i < row; i++)
            {

                for (int j = 0; j < col; j++)
                {
                   List<double> eucledianDist = new List<double>();

                    for (int k = 0; k < total_frames; k++)
                    {
                        
                        for (int q = 0; q < cluster_n; q++)
                        {
                            double temp = 0;
                            for (int w = 0; w < 8; w++)
                                temp += MegaBlock[i][j][k][w] * MegaBlock[i][j][k][w] - codewords[i][j][q][w] * codewords[i][j][q][w];

                            eucledianDist.Add(Math.Sqrt(temp));

                        }

                        if (!Double.IsNaN(eucledianDist.Min()) && !Double.IsInfinity(eucledianDist.Min()))
                            minDistMatrix[k][i][j] = eucledianDist.Min();
                        else
                            minDistMatrix[k][i][j] = 0;

                        eucledianDist.Clear();

                    }

                }

             
            }

            return minDistMatrix;
            
        }


    }
}
