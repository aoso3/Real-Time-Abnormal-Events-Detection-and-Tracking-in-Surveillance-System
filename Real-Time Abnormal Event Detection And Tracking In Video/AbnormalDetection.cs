using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    class AbnormalDetection
    {
        double[][][][] MegaBlock,codewords;
        List<double[][][]> MotionInfOfFrames;
        MotionInfluenceMap MIG;
        MegaBlocks MB;
        public int xBlockSize, yBlockSize, noOfRowInBlock, noOfColInBlock, n, row, col, cluster_n;
        int total_frames;
        public static double threshold = 380;//550
        //public static double threshold = 260; //best
        //public static double threshold = 0.854147293713963;
        //double threshold = 4.19471780437277;
        //double threshold = 7.96910765445956E-50;
        double[][][] minDistMatrix;
        public int fram_num;

       


        public AbnormalDetection(String video_path, String Codeword_path,int frame_number)
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

            //for (int a = 0; a < row; a++)
            //    for (int b = 0; b < col; b++)
            //        for (int q = 0; q < 5; q++)
            //            for (int w = 0; w < 8; w++)
            //                Console.WriteLine(codewords[a][b][q][w]);
                        

        }

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
