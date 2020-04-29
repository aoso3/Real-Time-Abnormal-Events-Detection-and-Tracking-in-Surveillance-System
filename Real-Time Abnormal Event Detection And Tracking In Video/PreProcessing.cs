using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    /// <summary>
    /// Divide the data into train/test.
    /// </summary>
    class PreProcessing
    {

        /// <summary>
        /// Takes the data as a whole and divide it into train/test.
        /// </summary>
        /// <param name="inputs">The data we want to spilt.</param>
        /// <param name="outputs">Labels of the data.</param>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <returns></returns>
        public static void train_test_split(double[][] inputs, int[] outputs, out double[][] train_data, out double[][] test_data, out int[] train_label, out int[] test_label)
        {
            train_data = new double[((int)Math.Floor(inputs.Length * 0.70))][];
            test_data = new double[((int)Math.Floor(inputs.Length * 0.20))][];
            train_label = new int[((int)Math.Floor(inputs.Length * 0.70))];
            test_label = new int[((int)Math.Floor(inputs.Length * 0.20))];


            int j = 0;
            for (int i = 0; i < inputs.Length; i++)
            {
                if (i < ((int)(inputs.Length * 0.70)))
                {
                    train_data[i] = inputs[i];
                    train_label[i] = outputs[i];
                }
                else
                {
                    if (j < test_data.Length)
                    {
                        test_data[j] = inputs[i];
                        test_label[j] = outputs[i];
                    }
                    j++;
                }
            }
        }

    }
}
