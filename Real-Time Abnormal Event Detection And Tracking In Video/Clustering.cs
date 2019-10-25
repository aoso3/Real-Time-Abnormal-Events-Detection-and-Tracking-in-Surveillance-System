using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    class Clustering
    {
        int xBlockSize, yBlockSize, noOfRowInBlock, noOfColInBlock;
        int total_frames;
        public void cluster_video(String Source_video_path,String Codeword_path)
        {

            MotionInfluenceMap MIG = new MotionInfluenceMap();
            List<double[][][]> MotionInfOfFrames = MIG.get_motion_influence_map(Source_video_path, out xBlockSize, out yBlockSize, out noOfRowInBlock, out noOfColInBlock, out total_frames,true);
            MegaBlocks MB = new MegaBlocks();
            double[][][][] MegaBlock = MB.createMegaBlocks(MotionInfOfFrames, xBlockSize, yBlockSize, noOfRowInBlock, noOfColInBlock, (int)total_frames);
            double[][][][] codewords = MB.kmeans(MegaBlock, xBlockSize, yBlockSize, (int)total_frames);

            int n = 2;
            int row = xBlockSize / n + 1;
            int col = yBlockSize / n + 1;
            using (StreamWriter st = new StreamWriter(Codeword_path))
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < col; j++)
                        for (int q = 0; q < 5; q++)
                        {
                            for (int w = 0; w < 8; w++)
                            {
                                if (w != 7) 
                                    st.Write(codewords[i][j][q][w] + " ");
                                else
                                    st.Write(codewords[i][j][q][w]);

                            }
                            st.WriteLine();
                        }
                    
        }


    }
}
