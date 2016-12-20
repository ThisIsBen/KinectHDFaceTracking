using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using System.Threading;
using System.Diagnostics;

using BytescoutScreenCapturingLib; // import bytescout screen capturing activex object 
using System.Drawing;
using Microsoft.Kinect;

namespace SimpleCaptureCSharp
{
    class screenRecorder
    {
        private Capturer capturer = new Capturer(); // create new screen capturer object
        public screenRecorder()
        {
            

            capturer.CapturingType = CaptureAreaType.catRegion; // set capturing area type to catScreen to capture whole screen

            capturer.OutputFileName = "C:/Users/Ben/Videos/interviewVideo.avi"; // set output video filename to .WMV or .AVI file

            // set output video width and height
            capturer.CaptureRectLeft = 1050; // set left coordinate of the rectangle region to record video from
            capturer.CaptureRectTop = 660; // set top coordinate of the rectangle region to record video from

            capturer.CaptureRectWidth = 363; // set width of the rectangle region to record video from
            capturer.CaptureRectHeight = 227; // set height of the rectangle region to record 
            // uncomment to set Bytescout Lossless Video format output video compression method
            // do not forget to set file to .avi format if you use Video Codec Name
            // capturer.CurrentVideoCodecName = "Bytescout Lossless";             

            capturer.OutputWidth = 320;
            capturer.OutputHeight = 240;

        }


        public void startRecording()
        {
            

            capturer.Run(); // run screen video capturing 

            Thread.Sleep(1000); // wait for 15 seconds

            capturer.Stop(); // stop video capturing

           
            //Process.Start("EntireScreenCaptured.avi");
        }


        
    }




}
