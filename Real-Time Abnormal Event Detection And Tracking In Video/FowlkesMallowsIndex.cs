using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    /// <summary>
    /// Evaluation method that is used to determine the similarity between two clusterings
    /// using Fowlkes–Mallows index.
    /// </summary>
    class FowlkesMallowsIndex
    {
        int tp = 0;
        int fp = 0;
        int fn = 0;
        List<int> pridect = new List<int>();
        List<int> target = new List<int>();

        /// <summary>
        /// Evaluate the clustering using Fowlkes–Mallows index.
        /// </summary>
        /// <param name="Pridect_path">Path of the predicted clusters on the disk.</param>
        /// <param name="Target_path">Path of the target clusters on the disk.</param>
        /// <returns>The result value of Fowlkes–Mallows index.</returns>
        public double Evaluate(String Pridect_path, String Target_path)
        {

            using (StreamReader reader = new StreamReader(Pridect_path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    pridect.Add(Convert.ToInt16(line));
            }

            using (StreamReader reader = new StreamReader(Target_path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    target.Add(Convert.ToInt16(line));
            }


            for (int i = 0; i < target.Count; i++)
            {
                if (pridect[i] == target[i])
                    tp++;
                if ((pridect[i] != target[i]) && target[i] == 1)
                    fp++;
                if ((pridect[i] != target[i]) && pridect[i] == 1)
                    fn++;
            }

            double FMI = tp / Math.Sqrt((tp + fp) * (tp + fn));

            return FMI;
        }
    }
}
