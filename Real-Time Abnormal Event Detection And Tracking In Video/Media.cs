using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Real_Time_Abnormal_Event_Detection_And_Tracking_In_Video
{
    class Media
    {
        public static int num = 0;
        public static void Crop_video(String vid,int start_time,int length)
        {
            var inputFile = new MediaFile { Filename = vid};
            var outputFile = new MediaFile { Filename = @"D:\2\Real-Time Abnormal Event Detection And Tracking In Video\croped_videos\crop"+num.ToString()+".mpg" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                var options = new ConversionOptions();

                // This example will create a 25 second video, starting from the 
                // 30th second of the original video.
                //// First parameter requests the starting frame to cut the media from.
                //// Second parameter requests how long to cut the video.
                options.CutMedia(TimeSpan.FromSeconds(start_time), TimeSpan.FromSeconds(length));

                engine.Convert(inputFile, outputFile, options);
            }

        }

        public static void thumbnail(String vid, int start_time)
        {
            var inputFile = new MediaFile { Filename = vid };
            var outputFile = new MediaFile { Filename = @"D:\2\Real-Time Abnormal Event Detection And Tracking In Video\croped_videos\crop" + num.ToString() + ".jpg" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                // Saves the frame located on the 15th second of the video.
                var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(start_time) };
                engine.GetThumbnail(inputFile, outputFile, options);
            }

        }
    }
}
