using Accord.MachineLearning;
using Accord.IO;
using Accord.MachineLearning.Performance;
using Accord.Math.Distances;
using Accord.Statistics.Analysis;
using real_time_abnormal_event_detection_and_tracking_in_video;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Accord.MachineLearning.DecisionTrees;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Fitting;
using Accord.Statistics.Models.Regression;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.Bayes;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using Accord.Statistics.Models.Markov;


namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    /// <summary>
    /// Classifiers used to classify the frames in video to normal and abnormal
    /// </summary>
    class Classifiers
    {

        /// <summary>
        /// Extract the features of the video using the extraction algortithm implimented
        /// in FeaturesExtraction class.
        /// </summary>
        /// <param name="Video_Path">Path of the video on the disk.</param>
        /// <param name="Features_Path">Path of the features on the disk.</param>
        /// <returns></returns>
        public void FeaturesExtraction(String Video_Path, String Features_Path)
        {
            FeaturesExtraction t = new FeaturesExtraction();
            t.Extract_Featurers2(Video_Path, Features_Path);
        }

        /// <summary>
        /// serialize the data and labels and split them into train/test data
        /// </summary>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <param name="Data_Path">Path of the data on the disk.</param>
        /// <param name="Labels_Path">Path of the labels on the disk.</param>
        /// <returns></returns>
        public void Data(out double[][] train_data, out double[][] test_data, out int[] train_label, out int[] test_label, String Data_Path, String Labels_Path)
        {
            double[][] inputs = Serialize.DeSerializeObject<double[][]>(Data_Path);
            int[] outputs = Serialize.DeSerializeObject<int[]>(Labels_Path);

            PreProcessing.train_test_split(inputs, outputs, out train_data, out test_data, out train_label, out test_label);

        }

        /// <summary>
        /// Classify our data using random forest classifer and save the model.
        /// </summary>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <param name="Classifier_Path">Path where we want to save the classifer on the disk.</param>
        /// <param name="Classifier_Name">Name of the classifer we wnat to save.</param>
        /// <param name="NumOfTrees">Number of trees used in Random forest classifer</param>
        /// <returns></returns>
        public void RandomForestLearning(double[][] train_data, double[][] test_data, int[] train_label, int[] test_label, String Classifier_Path, String Classifier_Name, int NumOfTrees = 20)
        {
            var teacher = new RandomForestLearning()
            {
                NumberOfTrees = NumOfTrees,
            };

            var forest = teacher.Learn(train_data, train_label);

            int[] predicted = forest.Decide(test_data);

            double error = new ZeroOneLoss(test_label).Loss(predicted);

            Console.WriteLine(error);

            forest.Save(Path.Combine(Classifier_Path, Classifier_Name));

        }

        /// <summary>
        /// Classify our data using k-nearest neighbors classifer and save the model.
        /// </summary>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <param name="Classifier_Path">Path where we want to save the classifer on the disk.</param>
        /// <param name="Classifier_Name">Name of the classifer we wnat to save.</param>
        /// <returns></returns>
        public void Knn(double[][] train_data, double[][] test_data, int[] train_label, int[] test_label, String Classifier_Path, String Classifier_Name)
        {
            KNearestNeighbors knn = new KNearestNeighbors(k: 5);
            knn.Learn(train_data, train_label);

            int answer = knn.Decide(new double[] { 117.07004523277283, 119.9104585647583 });
            var cm = GeneralConfusionMatrix.Estimate(knn, test_data, test_label);
            double error = cm.Error;

            Console.WriteLine(error);

            knn.Save(Path.Combine(Classifier_Path, Classifier_Name));

        }

        /// <summary>
        /// Classify our data using Logistic Regression classifer and save the model.
        /// </summary>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <param name="Classifier_Path">Path where we want to save the classifer on the disk.</param>
        /// <param name="Classifier_Name">Name of the classifer we wnat to save.</param>
        /// <returns></returns>
        public void LogisticRegression(double[][] train_data, double[][] test_data, int[] train_label, int[] test_label, String Classifier_Path, String Classifier_Name)
        {

            var learner = new IterativeReweightedLeastSquares<LogisticRegression>()
            {
                Tolerance = 1e-4,
                MaxIterations = 100,
                Regularization = 0
            };

            LogisticRegression regression = learner.Learn(train_data, train_label);

            double ageOdds = regression.GetOddsRatio(0);
            double smokeOdds = regression.GetOddsRatio(1);

            double[] scores = regression.Probability(test_data);

            //bool[] pre = regression.Decide(test_data);

            var cm = GeneralConfusionMatrix.Estimate(regression, test_data, test_label);

            double error = cm.Error;

            Console.WriteLine(error);

            regression.Save(Path.Combine(Classifier_Path, Classifier_Name));

        }

        /// <summary>
        /// Classify our data using support vector machine classifer and save the model.
        /// </summary>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <param name="Classifier_Path">Path where we want to save the classifer on the disk.</param>
        /// <param name="Classifier_Name">Name of the classifer we wnat to save.</param>
        /// <returns></returns>
        public void SVM(double[][] train_data, double[][] test_data, int[] train_label, int[] test_label, String Classifier_Path, String Classifier_Name)
        {

            var learn = new SequentialMinimalOptimization<Gaussian>()
            {
                UseComplexityHeuristic = true,
                UseKernelEstimation = true
            };

            try
            {
                SupportVectorMachine<Gaussian> svm = learn.Learn(train_data, train_label);

                bool[] prediction = svm.Decide(test_data);

                var cm = GeneralConfusionMatrix.Estimate(svm, test_data, test_label);


                double error = cm.Error;

                Console.WriteLine(error);

                svm.Save(Path.Combine(Classifier_Path, Classifier_Name));

            }
            catch (Exception e)
            { Console.WriteLine(e.StackTrace); }

        }

        /// <summary>
        /// Classify our data using naive bias classifer and save the model.
        /// </summary>
        /// <param name="train_data">Frame objects that we will use to train classifers.</param>
        /// <param name="test_data">Frame objects that we will use to test classifers.</param>
        /// <param name="train_label">Labels of the train data.</param>
        /// <param name="test_label">Labels of the test data.</param>
        /// <param name="Classifier_Path">Path where we want to save the classifer on the disk.</param>
        /// <param name="Classifier_Name">Name of the classifer we wnat to save.</param>
        /// <returns></returns>
        public void Naive_Bias(double[][] train_data, double[][] test_data, int[] train_label, int[] test_label, String Classifier_Path, String Classifier_Name)
        {

            Accord.Math.Random.Generator.Seed = 0;

            int[][] tr_da = new int[train_data.Length][];

            for (int i = 0; i < train_data.Length; i++)
            {
                int[] temp = new int[2];
                temp[0] = (int)train_data[i][0];
                temp[1] = (int)train_data[i][1];

                tr_da[i] = temp;
            }

            int[][] te_da = new int[test_data.Length][];

            for (int i = 0; i < test_data.Length; i++)
            {
                int[] temp = new int[2];
                temp[0] = (int)test_data[i][0];
                temp[1] = (int)test_data[i][1];

                te_da[i] = temp;
            }


            // Let us create a learning algorithm
            var learner = new NaiveBayesLearning();

            // and teach a model on the data examples
            NaiveBayes nb = learner.Learn(tr_da, train_label);

            // Now, let's test  the model output for the first input sample:
            //int answer = nb.Decide(new int[] {20,10000}); // should be 1
            double[] scores = nb.Probability(te_da);


            nb.Save(Path.Combine(Classifier_Path, Classifier_Name));

        }

        /// <summary>
        /// Classify our data using hidden markov model classifer and save the model.
        /// </summary>
        /// <param name="Data_Path">Path of the data on the disk.</param>
        /// <param name="Classifier_Path">Path where we want to save the classifer on the disk.</param>
        /// <param name="Classifier_Name">Name of the classifer we wnat to save.</param>
        /// <returns></returns>
        public void HMM(String Data_Path, String Classifier_Path, String Classifier_Name)
        {
            double[][] input = Serialize.DeSerializeObject<double[][]>(Data_Path);
            int[][] sequences = new int[input.Length][];

            for (int i = 0; i < input.Length; i++)
            {
                int[] temp = new int[2];
                temp[0] = (int)input[i][0];
                temp[1] = (int)input[i][1];
                sequences[i] = temp;
            }

            // Create the learning algorithm
            var teacher = new BaumWelchLearning()
            {
                Topology = new Ergodic(3), // Create a new Hidden Markov Model with 3 states for
                NumberOfSymbols = 2,       // an output alphabet of two characters (zero and one)
                Tolerance = 0.0001,        // train until log-likelihood changes less than 0.0001
                Iterations = 0             // and use as many iterations as needed
            };

            // Estimate the model
            HiddenMarkovModel hmm = teacher.Learn(sequences);
            hmm.Save(Path.Combine(Classifier_Path, Classifier_Name));


            //for (int i = 0; i < sequences.Length; i++)
            //{
            //    double fl1 = hmm.LogLikelihood(sequences[i]);     
            //    Console.WriteLine(fl1);

            //}


        }




    }
}
