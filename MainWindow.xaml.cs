// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.HDFaceBasics
{
    using System;
    using System.Threading;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Face;
    using System.Windows.Shapes;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.IO;
    using System.Diagnostics;
    using System.Windows.Threading;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Net.Sockets;
    using System.Linq;
    using System.Net;
    using System.Diagnostics;
    using System.Text;
    using NAudio.Wave;


    ///888888888888888888888


    using System.Windows.Data;

    using System.Windows.Input;
    using System.Windows.Markup;

    using System.Windows.Navigation;
    using System.Collections;
    
    
    
    //OBJ->JSON converter
    using System.Runtime.Serialization.Json;
   
    



    public class JSonHelper
    {

        public string ConvertObjectToJSon<T>(T obj)
        {

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

            MemoryStream ms = new MemoryStream();

            ser.WriteObject(ms, obj);

            string jsonString = Encoding.UTF8.GetString(ms.ToArray());

            ms.Close();

            return jsonString;

        }
    }





    /// 8888888888888888888

    /// <summary>
    /// Main Window
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 


        //set up a timer to get nonverbal data per second
        private static System.Timers.Timer nonverbal_Timer;
        //set up nonverbal record OBJ array to store nonverbal feature as long as 20 minutes with unit:sec
        const int nonverbalAryNum=600;//for 10 mins interview
        nonverbalRecord[] nonverbalRecordOBJ = new nonverbalRecord[nonverbalAryNum];
       
        //this indicate nonverbalRecord array's index
        int nonverbal_index = 0;

        /// </summary>

        int buttonclick = 0;


        private KinectSensor sensor = null;
        private BodyFrameSource bodySource = null;
        private BodyFrameReader bodyReader = null;
        private HighDefinitionFaceFrameSource highDefinitionFaceFrameSource = null;
        private HighDefinitionFaceFrameReader highDefinitionFaceFrameReader = null;

        //capture face expression 
        private FaceAlignment currentFaceAlignment = null;
        //
        private FaceModel currentFaceModel = null;
        private FaceModelBuilder faceModelBuilder = null;
        private Body currentTrackedBody = null;
        private ulong currentTrackingId = 0;
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;

        private const int BytesPerSample = sizeof(float);
        private const int SamplesPerColumn = 40;
        private const int MinEnergy = -90;
        private const int EnergyBitmapWidth = 780;
        private const int EnergyBitmapHeight = 195;
        private readonly byte[] audioBuffer = null;
        private readonly float[] energy = new float[(uint)(EnergyBitmapWidth * 1.25)];
        private readonly object energyLock = new object();
        private AudioBeamFrameReader reader = null;
        private float beamAngle = 0;
        private float beamAngleConfidence = 0;
        private float accumulatedSquareSum;
        private int accumulatedSampleCount;
        private int energyIndex;
        private int newEnergyAvailable;

        private Body[] bodies = null;
        private int bodyCount = 1;
        private int displayWidth;
        private int displayHeight;
        private FaceFrameSource[] faceFrameSources = null;
        private FaceFrameReader[] faceFrameReaders = null;
        private FaceFrameResult[] faceFrameResults = null;

       
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 
        ///////////////////////////////888888888888888888//////////////////////////////////////////////////
        //public int ElevationAngle { get; set; }

        ////////////////////////////////888888888888888/////////////////////
        public MainWindow()
        {

            //init nonverbal recorder array
            for (int i = 0; i < nonverbalAryNum; i++)
            {
                nonverbalRecordOBJ[i] = new nonverbalRecord();
                
                nonverbalRecordOBJ[i].timeStamp = "-";
                nonverbalRecordOBJ[i].smileIntensity = -1;
                nonverbalRecordOBJ[i].nodIntensity = -1;
                nonverbalRecordOBJ[i].eyeContactIntensity = -1;
                //indicate the beginning of the interview
                nonverbalRecordOBJ[i].begin_end = "-";
                
            }
            //////////////////////////////////
            









            ////////////////////////////////////////


            //clear up speech_to_text.txt
            //System.IO.File.WriteAllText(@"speech_to_text.txt", string.Empty);
            //
            InitializeComponent();

            Initializewindow();

            this.sensor = KinectSensor.GetDefault();

            this.InitializeHDFace();




            ////8888888888888888888888888888888888888888

            //this.sensor.ElevationAngle = 0;





            ///888888888888888888888888888888888888888888
            // get the color frame details
            FrameDescription frameDescription = this.sensor.ColorFrameSource.FrameDescription;

            // set the display specifics
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // set the maximum number of bodies that would be tracked by Kinect
            this.bodyCount = this.sensor.BodyFrameSource.BodyCount;

            // allocate storage to store body objects
            this.bodies = new Body[this.bodyCount];


            // open the reader for the color frames
            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // set IsAvailableChanged event notifier
            this.sensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.sensor.Open();

            //this.InitializeComponent();
            this.DataContext = this;

            AudioSource audioSource = this.sensor.AudioSource;
            this.audioBuffer = new byte[audioSource.SubFrameLengthInBytes];
            this.reader = audioSource.OpenReader();
            this.reader.FrameArrived += this.Reader_FrameArrived;

            // specify the required face frame results
            FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.BoundingBoxInColorSpace
                | FaceFrameFeatures.PointsInColorSpace
                | FaceFrameFeatures.LookingAway;
            // create a face frame source + reader to track each face in the FOV
            this.faceFrameSources = new FaceFrameSource[this.bodyCount];
            this.faceFrameReaders = new FaceFrameReader[this.bodyCount];
            for (int i = 0; i < this.bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                this.faceFrameSources[i] = new FaceFrameSource(this.sensor, 0, faceFrameFeatures);

                // open the corresponding reader
                this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
            }
            // allocate storage to store face frame results for each face in the FOV
            this.faceFrameResults = new FaceFrameResult[this.bodyCount];


        }
        private  void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)

        {
            //if (smileave.Sum() != 0)
            //{
            outfile2.WriteLine(smileave.Sum());
            outfile2.Flush();
            //add nonverbal feature to nonverbalRecordOBJ array record per second
           
            nonverbalRecordOBJ[nonverbal_index].timeStamp = System.DateTime.Now.ToString("u");
            nonverbalRecordOBJ[nonverbal_index].smileIntensity = smileave.Sum();
            nonverbalRecordOBJ[nonverbal_index].nodIntensity = nodave.Sum();
            nonverbalRecordOBJ[nonverbal_index].eyeContactIntensity = eyeave.Sum();
            //indicate the beginning of the interview
            if (nonverbal_index == 0)
                nonverbalRecordOBJ[nonverbal_index].begin_end = "begin";
            else
                nonverbalRecordOBJ[nonverbal_index].begin_end = "-";
            //increment index
            nonverbal_index++;

            //Console.WriteLine("hi ben!");
            //}
        }

        public void Initializewindow()
        {

            SolidColorBrush transBrush = new SolidColorBrush();
            transBrush.Color = Colors.Transparent;

            SolidColorBrush yellowBrush = new SolidColorBrush();
            yellowBrush.Color = Colors.Yellow;


            string path = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/Images/";
            BitmapImage start = new BitmapImage(new Uri(path + "start.png"));
            Image diaask = new Image();
            diaask.Source = start;
            diaask.Width = 40;
            diaask.Height = 40;

            BitmapImage color = new BitmapImage(new Uri(path + "color.png"));
            Image diaask1 = new Image();
            diaask1.Source = color;
            diaask1.Width = 40;
            diaask1.Height = 40;

            BitmapImage action = new BitmapImage(new Uri(path + "action.png"));
            Image diaask2 = new Image();
            diaask2.Source = action;
            diaask2.Width = 40;
            diaask2.Height = 40;

            BitmapImage close = new BitmapImage(new Uri(path + "close.jpg"));
            Image diaask3 = new Image();
            diaask3.Source = close;
            diaask3.Width = 40;
            diaask3.Height = 40;

            ImageBrush brush1 = new ImageBrush();
            brush1.ImageSource = start;
            startbtn.Content = diaask;
            startbtn.BorderBrush = transBrush;
            startbtn.Click += startButton_Click;

            ImageBrush brush2 = new ImageBrush();
            brush2.ImageSource = color;
            colorbtn.Content = diaask1;
            colorbtn.BorderBrush = transBrush;
            colorbtn.Tag = "color close";
            colorbtn.Click += Button_Click_1;

            ImageBrush brush3 = new ImageBrush();
            brush3.ImageSource = action;
            actionbtn.Content = diaask2;
            actionbtn.BorderBrush = transBrush;
            actionbtn.Tag = "action close";
            actionbtn.Click += newwindow1_Click;

            ImageBrush brush4 = new ImageBrush();
            brush4.ImageSource = close;
            closebtn.Content = diaask3;
            closebtn.BorderBrush = transBrush;
            closebtn.Tag = "action close";
            closebtn.Click += newwindow1_Click;

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //3D人物
            
            web1.Navigate("http://localhost/3d.htm");// http://localhost/iisstart.htm
            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameReaders[i] != null)
                {
                    // wire handler for face frame arrival
                    this.faceFrameReaders[i].FrameArrived += this.Reader_FaceFrameArrived;
                }
            }


        }

        int lookingaway = 0;
        private void Reader_FaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
            {
                if (faceFrame != null)
                {
                    // get the index of the face source from the face source array
                    int index = this.GetFaceSourceIndex(faceFrame.FaceFrameSource);

                    // check if this face frame has valid face frame results
                    if (this.ValidateFaceBoxAndPoints(faceFrame.FaceFrameResult))
                    {
                        // store this face frame result to draw later
                        this.faceFrameResults[index] = faceFrame.FaceFrameResult;
                        foreach (var item in this.faceFrameResults[index].FaceProperties)
                        {
                            if (item.Key.ToString().Equals("LookingAway"))
                            {
                                // consider a "maybe" as a "no" to restrict 
                                // the detection result refresh rate
                                if (item.Value == DetectionResult.Maybe)
                                {
                                    lookingaway = 1;
                                }
                                else
                                {
                                    if (item.Value.ToString().Equals("No"))
                                    {
                                        lookingaway = 1;
                                    }
                                    else
                                    {
                                        lookingaway = 0;
                                    }
                                }
                            }
                        }
                        //    len.Text = lookingaway.ToString();
                    }
                    else
                    {
                        // indicates that the latest face frame result from this reader is invalid
                        this.faceFrameResults[index] = null;
                    }
                }
            }
        }

        private bool ValidateFaceBoxAndPoints(FaceFrameResult faceResult)
        {
            bool isFaceValid = faceResult != null;

            if (isFaceValid)
            {
                var faceBox = faceResult.FaceBoundingBoxInColorSpace;
                if (faceBox != null)
                {
                    // check if we have a valid rectangle within the bounds of the screen space
                    isFaceValid = (faceBox.Right - faceBox.Left) > 0 &&
                                  (faceBox.Bottom - faceBox.Top) > 0 &&
                                  faceBox.Right <= this.displayWidth &&
                                  faceBox.Bottom <= this.displayHeight;

                    if (isFaceValid)
                    {
                        var facePoints = faceResult.FacePointsInColorSpace;
                        if (facePoints != null)
                        {
                            foreach (PointF pointF in facePoints.Values)
                            {
                                // check if we have a valid face point within the bounds of the screen space
                                bool isFacePointValid = pointF.X > 0.0f &&
                                                        pointF.Y > 0.0f &&
                                                        pointF.X < this.displayWidth &&
                                                        pointF.Y < this.displayHeight;

                                if (!isFacePointValid)
                                {
                                    isFaceValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isFaceValid;
        }

        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }


        double db = 0;
        private void Reader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
        {
            AudioBeamFrameReference frameReference = e.FrameReference;
            AudioBeamFrameList frameList = frameReference.AcquireBeamFrames();

            if (frameList != null)
            {
                // AudioBeamFrameList is IDisposable
                using (frameList)
                {
                    // Only one audio beam is supported. Get the sub frame list for this beam
                    IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;

                    // Loop over all sub frames, extract audio buffer and beam information
                    foreach (AudioBeamSubFrame subFrame in subFrameList)
                    {
                        // Check if beam angle and/or confidence have changed

                        if (subFrame.BeamAngle != this.beamAngle)
                        {
                            this.beamAngle = subFrame.BeamAngle;
                        }

                        if (subFrame.BeamAngleConfidence != this.beamAngleConfidence)
                        {
                            this.beamAngleConfidence = subFrame.BeamAngleConfidence;
                        }

                        // Process audio buffer
                        subFrame.CopyFrameDataToArray(this.audioBuffer);

                        for (int i = 0; i < this.audioBuffer.Length; i += BytesPerSample)
                        {
                            // Extract the 32-bit IEEE float sample from the byte array
                            float audioSample = BitConverter.ToSingle(this.audioBuffer, i);

                            this.accumulatedSquareSum += audioSample * audioSample;
                            ++this.accumulatedSampleCount;

                            if (this.accumulatedSampleCount < SamplesPerColumn)
                            {
                                continue;
                            }

                            float meanSquare = this.accumulatedSquareSum / SamplesPerColumn;

                            if (meanSquare > 1.0f)
                            {
                                // A loud audio source right next to the sensor may result in mean square values
                                // greater than 1.0. Cap it at 1.0f for display purposes.
                                meanSquare = 1.0f;
                            }

                            // Calculate energy in dB, in the range [MinEnergy, 0], where MinEnergy < 0
                            float energy = MinEnergy;

                            if (meanSquare > 0)
                            {
                                energy = (float)(10.0 * Math.Log10(meanSquare));
                                db = energy + 90;
                            }
                            //end Calculate energy in dB



                            lock (this.energyLock)
                            {
                                // Normalize values to the range [0, 1] for display
                                this.energy[this.energyIndex] = (MinEnergy - energy) / MinEnergy;
                                this.energyIndex = (this.energyIndex + 1) % this.energy.Length;
                                ++this.newEnergyAvailable;
                            }

                            this.accumulatedSquareSum = 0;
                            this.accumulatedSampleCount = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the current tracked user id
        /// </summary>
        private ulong CurrentTrackingId
        {
            get
            {
                return this.currentTrackingId;
            }

            set
            {
                this.currentTrackingId = value;

                // this.StatusText = this.MakeStatusText();
            }
        }

        /// <summary>
        /// Called when disposed of
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose based on whether or not managed or native resources should be freed
        /// </summary>
        /// <param name="disposing">Set to true to free both native and managed resources, false otherwise</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.currentFaceModel != null)
                {
                    this.currentFaceModel.Dispose();
                    this.currentFaceModel = null;
                }
            }
        }

        /// <summary>
        /// Returns the length of a vector from origin
        /// </summary>
        /// <param name="point">Point in space to find it's distance from origin</param>
        /// <returns>Distance from origin</returns>
        private static double VectorLength(CameraSpacePoint point)
        {
            var result = Math.Pow(point.X, 2) + Math.Pow(point.Y, 2) + Math.Pow(point.Z, 2);

            result = Math.Sqrt(result);

            return result;
        }

        /// <summary>
        /// Finds the closest body from the sensor if any
        /// </summary>
        /// <param name="bodyFrame">A body frame</param>
        /// <returns>Closest body, null of none</returns>
        private static Body FindClosestBody(BodyFrame bodyFrame)
        {
            Body result = null;
            double closestBodyDistance = double.MaxValue;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    var currentLocation = body.Joints[JointType.SpineBase].Position;

                    var currentDistance = VectorLength(currentLocation);

                    if (result == null || currentDistance < closestBodyDistance)
                    {
                        result = body;
                        closestBodyDistance = currentDistance;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Find if there is a body tracked with the given trackingId
        /// </summary>
        /// <param name="bodyFrame">A body frame</param>
        /// <param name="trackingId">The tracking Id</param>
        /// <returns>The body object, null of none</returns>
        private static Body FindBodyWithTrackingId(BodyFrame bodyFrame, ulong trackingId)
        {
            Body result = null;

            Body[] bodies = new Body[bodyFrame.BodyCount];
            bodyFrame.GetAndRefreshBodyData(bodies);

            foreach (var body in bodies)
            {
                if (body.IsTracked)
                {
                    if (body.TrackingId == trackingId)
                    {
                        result = body;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Initialize Kinect object
        /// </summary>
        private void InitializeHDFace()
        {

            //this.sensor = KinectSensor.GetDefault();
            this.bodySource = this.sensor.BodyFrameSource;
            this.bodyReader = this.bodySource.OpenReader();
            this.bodyReader.FrameArrived += this.BodyReader_FrameArrived;

            this.highDefinitionFaceFrameSource = new HighDefinitionFaceFrameSource(this.sensor);
            this.highDefinitionFaceFrameSource.TrackingIdLost += this.HdFaceSource_TrackingIdLost;

            this.highDefinitionFaceFrameReader = this.highDefinitionFaceFrameSource.OpenReader();
            this.highDefinitionFaceFrameReader.FrameArrived += this.HdFaceReader_FrameArrived;

            this.currentFaceModel = new FaceModel();
            //init HD face expression tracker
            this.currentFaceAlignment = new FaceAlignment();

        }

        string sitepal = null;
        int number = -1;
        int flag = 0;
        double mouthopen = 0;
        int answeropensmile = 0;
        int threadtime = 0;
        String line = "";
        int[] smileave = new int[30];
        int[] nodave = new int[30];
        int[] eyeave = new int[30];
        double[] volumeave = new double[30];
        double speakingrate = 0;
        String interview_state = "";
        StreamWriter outfile1 = new StreamWriter("AllTxtFiles1.txt");
        
        StreamWriter outfile2 = new StreamWriter("smile_intensity.txt");
        StreamWriter outfile3 = new StreamWriter("Smile_Feature_TrainingData.txt");

        //for scroll down



        private void UpdateMesh()
        {
            MediaElement mysound = new MediaElement();
            richTextBox1.Document.Blocks.Clear();
            String[] questext = new String[5];
            String[] anstext = new String[5];
            String summary = "";
            String smile = "", nod = "", eye = "", volume = "";

            //按鈕圖片
            string chartPath = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/Images/";
            Image[] diaask = new Image[10];
            BitmapImage eyechart = new BitmapImage(new Uri(chartPath + "eye.png"));
            BitmapImage smilechart = new BitmapImage(new Uri(chartPath + "smile.png"));
            BitmapImage nodchart = new BitmapImage(new Uri(chartPath + "nod.png"));
            BitmapImage volumechart = new BitmapImage(new Uri(chartPath + "volume.png"));
            BitmapImage speakingratechart = new BitmapImage(new Uri(chartPath + "slow.png"));
            BitmapImage speakingratechart1 = new BitmapImage(new Uri(chartPath + "fast.png"));

            //介面顏色設定
            SolidColorBrush RedBrush = new SolidColorBrush();
            RedBrush.Color = Colors.Red;
            SolidColorBrush blueBrush = new SolidColorBrush();
            blueBrush.Color = Colors.Blue;
            SolidColorBrush greenBrush = new SolidColorBrush();
            greenBrush.Color = Colors.Green;
            SolidColorBrush yellowBrush = new SolidColorBrush();
            yellowBrush.Color = Colors.YellowGreen;
            SolidColorBrush orangeBrush = new SolidColorBrush();
            orangeBrush.Color = Colors.LightSalmon;
            SolidColorBrush yellow1Brush = new SolidColorBrush();
            yellow1Brush.Color = Colors.Yellow;

            string[] qa;
            //給虛擬人物說話的內容
            if (sitepal != null && flag == 0)
            {
                var document = (mshtml.IHTMLDocument3)web1.Document;
                mshtml.IHTMLElement btn = document.getElementById("test");
                if (btn != null)
                {
                    btn.setAttribute("value", sitepal);
                    btn.click();
                }
                answeropensmile++;
                flag = 1;
                //richTextBox1.IsReadOnly;
            }
            else if (sitepal == null)
            {
                flag = 0;
            }

            //抓取使用者AU
            var vertices = this.currentFaceModel.CalculateVerticesForAlignment(this.currentFaceAlignment);
            float mundOffen0 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.JawOpen];
            float mundOffen1 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.JawSlideRight];
            float mundOffen2 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LeftcheekPuff];
            float mundOffen3 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LefteyebrowLowerer];
            float mundOffen4 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LefteyeClosed];
            float mundOffen5 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerDepressorLeft];
            float mundOffen6 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerDepressorRight];
            float mundOffen7 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerPullerLeft];
            float mundOffen8 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipCornerPullerRight];
            float mundOffen9 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipPucker];
            float mundOffen10 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipStretcherLeft];
            float mundOffen11 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LipStretcherRight];
            float mundOffen12 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LowerlipDepressorLeft];
            float mundOffen13 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.LowerlipDepressorRight];
            float mundOffen14 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.RightcheekPuff];
            float mundOffen15 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.RighteyebrowLowerer];
            float mundOffen16 = currentFaceAlignment.AnimationUnits[FaceShapeAnimations.RighteyeClosed];
            float mundOffen17 = currentFaceAlignment.FaceOrientation.X;
            float mundOffen18 = currentFaceAlignment.FaceOrientation.Z;


            String AU, AU1, AUSmile;
            //AU所輸出的值
            AU1 = "JawOpen " + mundOffen0.ToString() + " JawSlideRight " + mundOffen1.ToString() + " LeftcheekPuff " + mundOffen2.ToString() + " LefteyebrowLowerer " + mundOffen3.ToString() + " LefteyeClosed " + mundOffen4.ToString()
                + " LipCornerDepressorLeft " + mundOffen5.ToString() + " LipCornerDepressorRight " + mundOffen6.ToString() + " LipCornerPullerLeft " + mundOffen7.ToString() + " LipCornerPullerRight " + mundOffen8.ToString()
                 + " LipPucker " + mundOffen9.ToString() + " LipStretcherLeft " + mundOffen10.ToString() + " LipStretcherRight " + mundOffen11.ToString() + " LowerlipDepressorLeft " + mundOffen12.ToString() + " LowerlipDepressorRight " + mundOffen13.ToString()
                 + " RightcheekPuff " + mundOffen14.ToString() + " RighteyebrowLowerer " + mundOffen15.ToString() + " RighteyeClosed " + mundOffen16.ToString() + " Nod " + mundOffen17.ToString() + " Tilt " + mundOffen18.ToString();
            AU = mundOffen0.ToString();
            //
            outfile1.WriteLine(AU1);
            outfile1.Flush();

            //get smile feature to file
            /*AU1 = " LipCornerDepressorLeft " + mundOffen5.ToString() + " LipCornerDepressorRight " + mundOffen6.ToString() + "JawOpen " + mundOffen0.ToString() + " LipStretcherLeft " + mundOffen10.ToString() + " LipStretcherRight " + mundOffen11.ToString() +
                " LeftcheekPuff " + mundOffen2.ToString() + " RightcheekPuff " + mundOffen14.ToString() + " LefteyeClosed " + mundOffen4.ToString()
                + " RighteyeClosed " + mundOffen16.ToString();
            */
            AUSmile = " 1:" + mundOffen5.ToString() + " 2:" + mundOffen6.ToString() + " 3:" + mundOffen0.ToString() + " 4:" + mundOffen10.ToString() + " 5:" + mundOffen11.ToString() +
                " 6:" + mundOffen2.ToString() + " 7:" + mundOffen14.ToString() + " 8:" + mundOffen4.ToString()
                + " 9:" + mundOffen16.ToString()+"\n";
            outfile3.WriteLine(AUSmile);
            outfile3.Flush();
            //end get smile feature to file

            mouthopen = mundOffen0;
            //len.Text = mundOffen17.ToString();

            //test.Text = mundOffen5.ToString();
            // smile = " LipCornerDepressorLeft " + mundOffen5.ToString() + " LipCornerDepressorRight " + mundOffen6.ToString();
            //  if (buttonclick == 1)
            {
                //辨識微笑、點頭.....並且記錄每個frame有沒有這些動作
                //可調下方的值已控制微笑的偵測靈敏度
                //if (mundOffen5 < 0.1 && mundOffen6 < 0.1)
                //if (mundOffen5 < 0.23 && mundOffen6 < 0.23)
                //if (mundOffen5 < 0.35 && mundOffen6 < 0.35)
                //avoid mistook head tilt for smile 
                //0.055
                if (mundOffen5 < 0.1 && mundOffen6 < 0.1 && -0.1 < mundOffen18 && mundOffen18 < 0.1)
                {
                    smile = "1";
                    smileave[threadtime] = 1;
                }
                else
                {
                    smileave[threadtime] = 0;
                    smile = "0";
                }
                //可調下方的值已控制nod的偵測靈敏度
                if (mundOffen17 >0.01)//0.006
                {
                    //len.Text = mundOffen17.ToString();
                    nodave[threadtime] = 1;
                    nod = "1";
                }
                else
                {
                    nodave[threadtime] = 0;
                    nod = "0";
                }
                //可調下方的值已控制eyecontact的偵測靈敏度
                if (lookingaway == 1 && -0.1 < mundOffen18 && mundOffen18 < 0.1 && -0.1 < mundOffen17 && mundOffen17 < 0.1)
                {
                    eyeave[threadtime] = 1;
                    eye = "1";
                }
                else
                {
                    eyeave[threadtime] = 0;
                    eye = "0";
                }

                //紀錄音量至array
                volumeave[threadtime] = db;
                volume = db.ToString();
            }

            

            canvas1.Children.Clear();
            canvas2.Children.Clear();
            //canvas3.Children.Clear();

            //real-time feedback的圖片
            diaask[1] = new Image();
            diaask[1].Source = smilechart;
            diaask[1].Width = 40;
            diaask[1].Height = 40;
            Canvas.SetLeft(diaask[1], 15);
            Canvas.SetTop(diaask[1], 28);

            diaask[0] = new Image();
            diaask[0].Source = eyechart;
            diaask[0].Width = 40;
            diaask[0].Height = 40;
            Canvas.SetLeft(diaask[0], 15);
            Canvas.SetTop(diaask[0], 70);

            diaask[2] = new Image();
            diaask[2].Source = nodchart;
            diaask[2].Width = 40;
            diaask[2].Height = 40;
            Canvas.SetLeft(diaask[2], 15);
            Canvas.SetTop(diaask[2], 110);

            diaask[3] = new Image();
            diaask[3].Source = volumechart;
            diaask[3].Width = 40;
            diaask[3].Height = 40;
            Canvas.SetLeft(diaask[3], 15);
            Canvas.SetTop(diaask[3], 150);

            diaask[4] = new Image();
            diaask[4].Source = speakingratechart;
            diaask[4].Width = 40;
            diaask[4].Height = 40;
            Canvas.SetLeft(diaask[4], 15);
            Canvas.SetTop(diaask[4], 190);

            diaask[5] = new Image();
            diaask[5].Source = speakingratechart1;
            diaask[5].Width = 40;
            diaask[5].Height = 30;
            Canvas.SetLeft(diaask[5], 340);
            Canvas.SetTop(diaask[5], 195);

            canvas1.Children.Add(diaask[0]);
            canvas1.Children.Add(diaask[1]);
            canvas1.Children.Add(diaask[2]);
            canvas1.Children.Add(diaask[3]);
            canvas1.Children.Add(diaask[4]);
            canvas1.Children.Add(diaask[5]);

            Rectangle rec = new Rectangle();
            rec.Height = 75;
            rec.Width = 75;
            rec.StrokeThickness = 4;
            rec.Stroke = RedBrush;

            //系統和使用者對話內容對話框
            qa = line.Split('\n');
            int flag1 = 0;
            for (int i = 0; i < qa.Length; i++)
            {
                if (i % 2 == 0 && qa[i].Length > 9)
                {
                    Run redRun = new Run();
                    redRun.Text = qa[i];
                    redRun.FontSize = 18;
                    redRun.Foreground = RedBrush;
                    Paragraph para = new Paragraph(redRun);
                    para.LineHeight = 10;
                    
                    richTextBox1.Document.Blocks.Add(para);
                    richTextBox1.ScrollToEnd();
                    flag1 = 1;
                }
                if (i % 2 == 0 && qa[i].Length <= 9)
                {
                    Run redRun = new Run();
                    redRun.Text = qa[i];
                    redRun.FontSize = 18;
                    redRun.Foreground = RedBrush;
                    richTextBox1.Document.Blocks.Add(new Paragraph(redRun));
                    richTextBox1.ScrollToEnd();
                }
                if (i % 2 == 1 && qa[i].Length > 9)
                {
                    Run redRun = new Run();
                    redRun.Text = qa[i];
                    redRun.FontSize = 18;
                    redRun.Foreground = greenBrush;
                    richTextBox1.Document.Blocks.Add(new Paragraph(redRun));
                    richTextBox1.ScrollToEnd();
                    if (flag1 == 1)
                    {
                        flag1 = 0;
                    }
                }
                if (i % 2 == 1 && qa[i].Length <= 9)
                {
                    Run redRun = new Run();
                    redRun.Text = qa[i];
                    redRun.FontSize = 18;
                    redRun.Foreground = greenBrush;
                    richTextBox1.Document.Blocks.Add(new Paragraph(redRun));
                    richTextBox1.ScrollToEnd();
                    if (flag1 == 1)
                    {
                        flag1 = 0;
                    }
                }
            }
            //
            /*if (number > -1 && number < 5 && (number * 2 + 1) <= qa.Length)
             {
                 WaveFileReader reader = new WaveFileReader("test" + number + ".wav");
                 TimeSpan duration = reader.TotalTime;
                 double length = duration.TotalMilliseconds;
                 //len.Text = length.ToString();
                 if (!qa[number * 2 + 1].Split(':')[1].Equals("null"))
                 {
                     speakingrate = qa[number * 2 + 1].Length / (length/1000);
                     outfile2.WriteLine(number.ToString() + " " + speakingrate.ToString());
                     outfile2.Flush();
                 }
                 else
                 {
                     speakingrate = 0;
                 }
                 //len.Text = speakingrate.ToString();
             }*/
            //
            //smile長條圖
            System.Windows.Controls.ProgressBar smilebar = new System.Windows.Controls.ProgressBar();
            //display the Sum of the current smile value in array.
            smilebar.Value = smileave.Sum();
            ///

            /*
            if(frameNum<30)
            {
                outfile2.WriteLine(smilebar.Value);
                outfile2.Flush();
                frameNum = 0;
            }
            else
                frameNum++;
                */
            ///
            Canvas.SetLeft(smilebar, 70);
            Canvas.SetTop(smilebar, 30);
            //smilebar.MaxWidth = 200;
            smilebar.Maximum = 30;
            smilebar.Height = 35;
            smilebar.Width = 300;
            smilebar.Orientation = System.Windows.Controls.Orientation.Horizontal;





            ///
            /*
            TextBlock smilear7 = new TextBlock();
            smilear7.Text = smilebar.Value.ToString();
            Canvas.SetLeft(smilear7, 300);
            Canvas.SetTop(smilear7, 30);
            canvas1.Children.Add(smilear7);
            *////
            //每五秒微笑佔的時間比例
            TextBlock smilearl = new TextBlock();
            //30if (smileave.Sum() < 45)
            /* only when smileave.Sum()>35  can be counted as a true smile.
                 other than that are just mistook speaking for smileing.
                 So, set smileave.Sum()=35=>smile分數 0*/

            //if smileave.sum < 3 , it means you smile rarely
            if (smileave.Sum() < 3)
            {
                smilebar.Foreground = RedBrush;

                smilearl.Text = smilebar.Value.ToString();
                Canvas.SetLeft(smilearl, 330);
                Canvas.SetTop(smilearl, 30);
            }
            else
            {
                smilebar.Foreground = yellow1Brush;
            }
            //點頭長條圖
            //we can count the value of nodbar as nod times directly!
            System.Windows.Controls.ProgressBar nodbar = new System.Windows.Controls.ProgressBar();
            nodbar.Foreground = blueBrush;
            //display the Sum of the current nod value in array.
            nodbar.Value = nodave.Sum();
            Canvas.SetLeft(nodbar, 70);
            Canvas.SetTop(nodbar, 110);
            nodbar.Maximum = 30;
            nodbar.Height = 35;
            nodbar.Width = 300;
            nodbar.Orientation = System.Windows.Controls.Orientation.Horizontal;
            //眼神接觸長條圖

            // we still need to trace user's eyeball to check whether he is making a true eyecontact
            System.Windows.Controls.ProgressBar eyebar = new System.Windows.Controls.ProgressBar();
            //display the Sum of the current eye value in array.
            eyebar.Value = eyeave.Sum();
            Canvas.SetLeft(eyebar, 70);
            Canvas.SetTop(eyebar, 70);
            eyebar.Maximum = 30;
            eyebar.Height = 35;
            eyebar.Width = 300;
            eyebar.Orientation = System.Windows.Controls.Orientation.Horizontal;
            //每五秒眼神接觸佔的時間比例
            TextBlock eyearl = new TextBlock();
            if (eyeave.Sum() < 12)
            {
                eyebar.Foreground = RedBrush;
                eyearl.Text = "過少";
            }
            else if (eyeave.Sum() > 27)
            {
                eyebar.Foreground = RedBrush;
                eyearl.Text = "過多";
            }
            Canvas.SetLeft(eyearl, 330);
            Canvas.SetTop(eyearl, 70);

            System.Windows.Controls.ProgressBar volumebar = new System.Windows.Controls.ProgressBar();
            volumebar.Foreground = yellowBrush;
            //display the Ave of the current volume value in array.
            volumebar.Value = volumeave.Average();
            Canvas.SetLeft(volumebar, 70);
            Canvas.SetTop(volumebar, 150);
            volumebar.MaxWidth = 300;
            volumebar.Height = 35;
            volumebar.Width = 300;
            volumebar.Orientation = System.Windows.Controls.Orientation.Horizontal;

            speakrate.Value = speakingrate * 0.2;

            canvas1.Children.Add(smilebar);
            canvas1.Children.Add(smilearl);
            canvas1.Children.Add(nodbar);
            canvas1.Children.Add(eyebar);
            canvas1.Children.Add(eyearl);
            canvas1.Children.Add(volumebar);
            //    canvas1.Children.Add(speakingratebar);

//現場DEMO須調整的
            //adjust the face capturing square position
            for (int i = 0; i < vertices.Count; i++)
            {
                CameraSpacePoint vert = vertices[i];
                DepthSpacePoint point = sensor.CoordinateMapper.MapCameraPointToDepthSpace(vert);
                if (float.IsInfinity(vert.X) || float.IsInfinity(vert.Y)) return;
                
                //adjust the x position of face capturing square position
                Canvas.SetLeft(rec, point.X / 1.5); //123456789  1.5   2.5
                //adjust the y position of face capturing square position
                Canvas.SetTop(rec, point.Y / 5.5);
            }
            canvas2.Children.Add(rec);

            if (buttonclick == 1)
            {
                summary = interview_state + " " + smile + " " + nod + " " + eye + " " + volume + " " + sitepal;
                

                
            }

            //increase index of array
            threadtime++;
            //when array reaches arraySize:150 , it would replace the previous data in the array with newly comming data.
            if (threadtime == 30)
            {
                threadtime = 0;
            }
        }


        /// <summary>
        /// This event fires when a BodyFrame is ready for consumption
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {

            var frameReference = e.FrameReference;
            using (var frame = frameReference.AcquireFrame())
            {
                if (frame == null)
                {
                    // We might miss the chance to acquire the frame, it will be null if it's missed
                    return;
                }

                if (this.currentTrackedBody != null)
                {
                    this.currentTrackedBody = FindBodyWithTrackingId(frame, this.CurrentTrackingId);

                    if (this.currentTrackedBody != null)
                    {
                        return;
                    }
                }

                Body selectedBody = FindClosestBody(frame);

                if (selectedBody == null)
                {
                    return;
                }

                if (frame != null)
                {
                    // update body data
                    frame.GetAndRefreshBodyData(this.bodies);

                    // iterate through each face source
                    for (int i = 0; i < this.bodyCount; i++)
                    {
                        // check if the corresponding body is tracked 
                        if (this.bodies[i].IsTracked)
                        {
                            // update the face frame source to track this body
                            this.faceFrameSources[i].TrackingId = this.bodies[i].TrackingId;
                        }

                    }

                }

                this.currentTrackedBody = selectedBody;
                this.CurrentTrackingId = selectedBody.TrackingId;

                this.highDefinitionFaceFrameSource.TrackingId = this.CurrentTrackingId;
            }
        }

        /// <summary>
        /// This event is fired when a tracking is lost for a body tracked by HDFace Tracker
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void HdFaceSource_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            var lostTrackingID = e.TrackingId;

            if (this.CurrentTrackingId == lostTrackingID)
            {
                this.CurrentTrackingId = 0;
                this.currentTrackedBody = null;
                if (this.faceModelBuilder != null)
                {
                    this.faceModelBuilder.Dispose();
                    this.faceModelBuilder = null;
                }

                this.highDefinitionFaceFrameSource.TrackingId = 0;
            }
        }

        /// <summary>
        /// This event is fired when a new HDFace frame is ready for consumption
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void HdFaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                // We might miss the chance to acquire the frame; it will be null if it's missed.
                // Also ignore this frame if face tracking failed.

                if (frame == null || !frame.IsFaceTracked)
                {
                    canvas2.Children.Clear();
                    return;
                }

                frame.GetAndRefreshFaceAlignmentResult(this.currentFaceAlignment);
                this.UpdateMesh();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /* 原本為錄影程式  但是現在沒用到所以先註解掉  因為一但開啟可能會停不掉
             * Process videocmd = new Process();
               videocmd.StartInfo.FileName = "ColorBasics-WPF.exe";
               videocmd.Start();*/

            //開啟錄音程式
            /*
            Process cmd = new Process();
            cmd.StartInfo.FileName = "java.exe";
            cmd.StartInfo.Arguments = "-jar E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/record.jar";
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();
            */

            buttonclick = 1;
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {

                        face.Source = this.colorBitmap;
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        //不要刪除這個函式
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            //this.StatusText = "aaa";
        }

        // UserControl1 test = new UserControl1();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //feedback.Visibility = Visibility.Hidden;
            if (colorbtn.Tag.ToString() == "color open")
            {
                face.Visibility = Visibility.Visible;
                canvas2.Visibility = Visibility.Visible;
                colorbtn.Tag = "color close";
            }
            else if (colorbtn.Tag.ToString() == "color close")
            {
                face.Visibility = Visibility.Hidden;
                canvas2.Visibility = Visibility.Hidden;
                colorbtn.Tag = "color open";
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            oThread.Abort();
        }

        private void newwindow1_Click(object sender, RoutedEventArgs e)
        {

            if (actionbtn.Tag.ToString() == "action open")
            {
                canvas1.Visibility = Visibility.Visible;
                actionbtn.Tag = "action close";
            }
            else if (actionbtn.Tag.ToString() == "action close")
            {
                canvas1.Visibility = Visibility.Hidden;
                actionbtn.Tag = "action open";
            }
        }

        Thread oThread = null;
        Thread Camerathread = null;
        //System.Windows.Forms.Timer timer1;
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            /* Process videocmd = new Process();
             videocmd.StartInfo.FileName = "ColorBasics-WPF.exe";
             videocmd.Start();*/
            /* Process cmd0 = new Process();
             cmd0.StartInfo.FileName = "java.exe";
             cmd0.StartInfo.Arguments = "-jar D:/record/voicerecord_3/recordall.jar";
             cmd0.Start();*/
            //開啟面試對話函式
            oThread = new Thread(new ThreadStart(work));
            //  Camerathread = new Thread(new ThreadStart(Camera));
            oThread.Start();
            // Camerathread.Start();
        }





        private void work()
        {
            //capture smile value per second
            // Create a timer with a two second interval.
            nonverbal_Timer = new System.Timers.Timer(1000);

            nonverbal_Timer.Elapsed += OnTimedEvent;
            //nonverbal_Timer.Tick += new EventHandler(captureSmilePS);
            //nonverbal_Timer.Interval = 1000;
            //start record user's nonverbal feature
            nonverbal_Timer.Start();

            


            /* Process videocmd = new Process();
             videocmd.StartInfo.FileName = "ColorBasics-WPF.exe";
             videocmd.Start();*/
            buttonclick = 1;
            double[] mouthopentime = new double[3];
            String[,] question = new String[6, 6];
            String[] temp = new String[5];
            String answer = "";
            int speaktime = 0;
            string nextquestion = "你好,";
            Random qnumber = new Random();
            int ran = 0;
            //記錄所有qa，並在UpdateMesh()去讀取，然後再印在對話框
            StreamWriter qa = new StreamWriter("E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/allQA.txt");
            //StreamWriter qa = new StreamWriter("E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/speech_to_text.txt");


            /*  StreamReader ques = new StreamReader("/question/1.txt", System.Text.Encoding.Default);
              question[0,0] = ques.ReadLine();
              ques.Close();*/

            /*  question[0,0] = "請自我介紹";
              question[1,0] = "為什麼你要報考這個科系";
              question[2,0] = "你在大學最拿手的科目是什麼";
              question[3,0] = "你未來的期望是什麼";
              question[4,0] = "你常做什麼運動";*/
            question[5, 0] = "面試到此結束";
            nextquestion = message3("start");
            //set up speech to text .txt 's reader
            //System.IO.StreamReader speech2textReader = new System.IO.StreamReader("speech_to_text.txt");


            for (int i = 0; i < 12; i++)
            {
                FileStream fs = new FileStream("name.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(i);
                sw.Flush();
                sw.Close();
                fs.Close();

                if (i < 10)
                {
                    //nextquestion 
                    //sitepal = question[i, ran];
                    sitepal = nextquestion.Split(';')[1];
                    //speaktime = question[i].Length*300
                    Thread.Sleep(nextquestion.Split(';')[1].Length * 300);
                    sitepal = null;
                }
                if (nextquestion.Split(';')[0] == "7" || i == 10)
                {
                    // message1();
                    sitepal = "面試結束";
                    //speaktime = question[i].Length*300
                    Thread.Sleep(nextquestion.Split(';')[0].Length * 350);
                    sitepal = null;
                    /*  Process videoclose = new Process();
                      videoclose.StartInfo.FileName = "cmd.exe";
                      videoclose.StartInfo.Arguments = "/k taskkill /f /im ColorBasics-WPF.exe";
                      videoclose.Start();*/
                    break;
                }
                // Thread.Sleep(1000); 
                /*java recording program
                 Process cmd = new Process();
                 cmd.StartInfo.FileName = "java.exe";
                 cmd.StartInfo.Arguments = "-jar D:/record/voicerecord_3/record.jar";
                 cmd.Start();
                 */

                //11/4 the following code's function has been replace by python recording and speech to text program
                /*
                interview_state = i + "_1";
                string strFolderPath = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/wave" + i.ToString() + "/";
                if (Directory.Exists(strFolderPath))
                {
                    DirectoryInfo DIFO = new DirectoryInfo(strFolderPath);
                    DIFO.Delete(true);
                }
                string wavFolderPath = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/waveA" + i.ToString() + "/";
                DirectoryInfo wavDIFO = new DirectoryInfo(wavFolderPath);
                wavDIFO.Create();
                */

                //call record program
                Process cmd1 = new Process();
                cmd1.StartInfo.FileName = "python";
                cmd1.StartInfo.Arguments = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/record.py ";
                cmd1.Start();
                cmd1.WaitForExit();
                //call asr

                //speech to text and store text in speech_to_text.txt
                Process cmd2 = new Process();
                cmd2.StartInfo.FileName = "python";
                cmd2.StartInfo.Arguments = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/speechtotext.py demo.wav";
                cmd2.Start();


                //11 / 4 the following code's function has been replace by python recording and speech to text program
                // 執行檔路徑下的 MyDir 資料夾
                //string folderName = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/wave" + i.ToString() + "/";

                // 取得資料夾內所有檔案

                string text = "";
                /*
               if (Directory.Exists(folderName))
               {
                   int filenum = 0;
                   foreach (string fname in Directory.GetFiles(folderName, "*.txt"))
                   {
                       string line1;
                       // 一次讀取一行
                       System.IO.StreamReader file = new System.IO.StreamReader(fname);
                       line1 = file.ReadToEnd();
                       if (filenum == (Directory.GetFiles(folderName, "*.txt").Length - 1) && !line1.Split('\n')[0].Equals(""))
                       {
                           text += line1.Split('\n')[0];
                       }
                       else if (!line1.Split('\n')[0].Equals(""))
                       {
                           text += line1.Split('\n')[0] + ",";
                       }

                       file.Close();
                       WaveFileReader reader = new WaveFileReader(fname.Replace(".txt", ""));
                       TimeSpan duration = reader.TotalTime;
                       double length = duration.TotalMilliseconds;
                       speakingrate = duration.TotalMilliseconds / text.Length;
                       filenum++;
                   }
               }
                */


                ///11/1
                ////*
                /*
                try
                {
                */

                // Your processing code here  
                //text = File.ReadAllLines("speech_to_text.txt")[0];
                //wait 1.8 second for python speechtotext program to write text to file.
                //但speech_to_text.txt的內容都沒更新!!solved
                //但speech_to_text.txt可事先不存在也可以囉!
                System.Threading.Thread.Sleep(1800);


                //text = speech2textReader.ReadLine();
                ///////////////////////////////////////////////////////////////////
                //StreamReader reader = new StreamReader("speech_to_text.txt");
                text = File.ReadAllLines("speech_to_text.txt")[0];

                /*
                }
                */
                /*
                catch (IndexOutOfRangeException e)
                {


                        cmd1.StartInfo.FileName = "python";
                        cmd1.StartInfo.Arguments = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/record.py ";
                        cmd1.Start();
                        cmd1.WaitForExit();

                        cmd2.StartInfo.FileName = "python";
                        cmd2.StartInfo.Arguments = "E:/temp/HDFaceBasics-WPF/KinectHDFaceTracking/bin/x64/Debug/speechtotext.py demo.wav";
                        cmd2.Start();

                        text = File.ReadAllLines("speech_to_text.txt")[0];
                        //StreamReader reader = new StreamReader("speech_to_text.txt");
                        //text = reader.ReadLine();


                }
                */


                Console.Read();


                //若錄音城市不成功就用下方此方式輸入你要說的回答句
                //string text = "你;好;你好\n";
                //text = "你;好;你好\n";
                string ckip = text;

                line += "System:" + nextquestion.Split(';')[1].Replace("\n", "") + "\n" + "User:" + ckip.Replace("\n", "") + "\n";
                answer += nextquestion.Split(';')[2].Replace("\n", "").Replace("\r", "") + "#" + nextquestion.Split(';')[1].Replace("\n", "").Replace("\r", "") + "#" + ckip.Replace("\n", "").Replace("\r", "") + "\n";


                //print user answer
                //Console.WriteLine(ckip);
                nextquestion = message3(ckip);

                number = i;


            }
            qa.Write(answer);
            qa.Close();

            //stop nonverbal timer because the interview has come to an end
            // stop the timer
            nonverbal_Timer.Stop();


            //System.Threading.Thread.Sleep(1800);
            
                //begin send nonverbal feature to back end 
            //Turn it into json
            JSonHelper helper0 = new JSonHelper();
            string jsonResult = helper0.ConvertObjectToJSon(nonverbalRecordOBJ);
            //Console.WriteLine("jsonResult Converter test" + jsonResult);

            //tcp to send json to back end
            TcpClient tcpclnt = new TcpClient();

            tcpclnt.Connect("54.191.185.244", 8084); // use the ipaddress as in the server program

            Stream stm = tcpclnt.GetStream();

            //encode UTF8
            Byte[] nonverbal_data = Encoding.UTF8.GetBytes(jsonResult);

            //encode ASCII
            /*
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(jsonResult);
            */

            stm.Write(nonverbal_data, 0, nonverbal_data.Length);

            tcpclnt.Close();
            //end send nonverbal feature to back end 

            Console.WriteLine("jsonResult Converter test" + jsonResult);
        }

        private void message()
        {
            TcpClient tc = new TcpClient("127.0.0.1", 1800);
            Console.WriteLine("Server invoked");
            NetworkStream ns = tc.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            sw.WriteLine("a");
            sw.Flush();
            //   sw.Close();
            StreamReader sr = new StreamReader(ns);
            Console.WriteLine(sr.ReadLine());
        }

        private void message1()
        {
            TcpClient tc = new TcpClient("127.0.0.1", 2000);
            Console.WriteLine("Server invoked");
            NetworkStream ns = tc.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            sw.WriteLine("a");
            sw.Flush();
            //   sw.Close();
            StreamReader sr = new StreamReader(ns);
            Console.WriteLine(sr.ReadLine());
        }

        private void message2()
        {
            string returnData;
            byte[] receiveBytes;
            //ConsoleKeyInfo cki = new ConsoleKeyInfo();

            using (UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3800)))
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3800);
                //   while (true)
                {
                    receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
                    returnData = Encoding.ASCII.GetString(receiveBytes);
                    Console.WriteLine(returnData);
                    udpClient.Close();

                    //if(returnData.Equals("2"))
                    {
                        //  break;
                    }
                }
            }
        }


        private string message3(string answer)
        {
            string returnData = "";
            //run back end Q generation program on 學長's PC
            //UdpClient udpClient = new UdpClient("140.116.82.104", 8082);

            //run back end Q generation program on AWS cloud server
            UdpClient udpClient = new UdpClient("54.244.174.89", 8082);

            //
            Byte[] sendBytes = Encoding.UTF8.GetBytes(answer);
            try
            {
                udpClient.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Thread.Sleep(500);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {

                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);

                returnData = Encoding.UTF8.GetString(receiveBytes);

                /* Console.WriteLine("This is the message you received " +
                                             returnData.ToString());
                 Console.WriteLine("This message was sent from " +
                                             RemoteIpEndPoint.Address.ToString() +
                                             " on their port number " +
                                             RemoteIpEndPoint.Port.ToString());*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine(returnData);
            return returnData;
        }

        static string seg(String answer)
        {
            string[] strDic = new StreamReader("GigaWord_Dic.txt", Encoding.Default).ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //string[] sentence = new StreamReader("questioncorpus_ckip8.txt", Encoding.Default).ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            string line = answer;
            string line_temp = "";
            int n = 0;
            while (n < strDic.Length)
            {
                if (line.Substring(0, 1) != strDic[n].ToString().Substring(0, 1))
                {
                    n++;
                    if (n == strDic.Length)
                    {
                        line = line.Remove(0, 1);
                        Console.Write(line);
                        n = 0;
                    }
                    if (line == "")
                    {
                        break;
                    }
                    continue;
                }
                else
                {
                    if (line.IndexOf(strDic[n].ToString()) == 0)
                    {
                        line_temp += strDic[n].ToString() + " ";
                        line = line.Remove(0, strDic[n].ToString().Length);
                        n = 0;
                        if (line == "")
                        {
                            break;
                        }
                    }
                    else
                    {
                        n++;
                    }
                }
            }
            Console.Write(line_temp);
            return line_temp;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // UserControl2 sw = new UserControl2();
            // sw._Value2 = "I'm number " + SubWindowsNum.ToString() + " SubWindow!";
            Form1 a = new Form1();
            a.Show();
            //return;
        }
        /*
        private void close_Click(object sender, RoutedEventArgs e)
        {
            if(right.Content.ToString() == "close all")
            {
                mainwindow.Width = 850;
                right.Content = "open all";
            }
            else if(right.Content.ToString() == "open all")
            {
                face.Visibility = Visibility.Visible;
                canvas2.Visibility = Visibility.Visible;
                canvas1.Visibility = Visibility.Visible;
                mainwindow.Width = 1275;
                right.Content = "close all";
            }
        }*/

    }
}