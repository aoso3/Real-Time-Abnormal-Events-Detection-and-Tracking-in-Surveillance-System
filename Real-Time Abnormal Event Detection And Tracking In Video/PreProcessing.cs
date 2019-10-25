using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    class PreProcessing
    {

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
