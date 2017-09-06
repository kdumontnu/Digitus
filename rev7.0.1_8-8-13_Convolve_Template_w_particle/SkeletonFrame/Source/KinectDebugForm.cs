using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Kinect;
using FingerDetection;
using System.Diagnostics;


namespace KinectHand
{
    public partial class KinectDebugForm : Form
    {
        #region DEEBUG_DATA

        int FPScount;
        string version = FingerDetection.DetectionParameters.version;

        #endregion

        #region BITMAP_DATA
        private Bitmap bodyBitmap;
        private Rectangle bodybitmapDimensions;
        private Bitmap[] debugBitmaps = new Bitmap[3];
        private Rectangle[] debugBitmapSizes = new Rectangle[3];
        private System.Drawing.Imaging.BitmapData[] bmpData = new System.Drawing.Imaging.BitmapData[3];
        byte[] rgbValues;
        #endregion

        private DepthFrameParams DFP = new DepthFrameParams();
        // Init sensor variable
        private KinectSensor sensor;

        // MS Kinect skeleton variable
        Skeleton skeleton;

        // Hand points
        HandPoint[] Hands = new HandPoint[2];

        // Array to hold depth stream
        private short[] depthPixels;

        public KinectDebugForm()
        {
            // there are two methods to draw to the form. One is by manually setting each pixel on a (can be multiple bitmaps) and then writing the bitmap to the form (shown at the bottom).
            // The other is by using the e.graphics method to do rectangles and lines, also at the bottom.

            // The ImagePanel.Invalidate() calls the draw function at the bottom.

            InitializeComponent();
            this.bodyBitmap = new Bitmap(640, 480);
            bodybitmapDimensions = new Rectangle(0, 0, bodyBitmap.Width, bodyBitmap.Height);

            for (int i = 0; i < 3; i++)
            {
                this.debugBitmaps[i] = new Bitmap(160, 160);
                this.debugBitmapSizes[i] = new Rectangle(640, i * 160, this.debugBitmaps[i].Width, this.debugBitmaps[i].Height);
                this.bmpData[i] = debugBitmaps[i].LockBits(new Rectangle(0, 0, debugBitmapSizes[i].Width, debugBitmapSizes[i].Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, debugBitmaps[i].PixelFormat);
            }
            this.rgbValues = new byte[bmpData[0].Stride * bmpData[0].Height];

            startKinect();

            Hands[0] = new HandPoint();
            Hands[0].InitializeFingers(Hands[0]);
            Hands[1] = new HandPoint();
            Hands[1].InitializeFingers(Hands[1]);

            this.VersionInfo.Text = "Version: " + version;

            this.Invalidate();

        }

        // Initialize Kinect and stream data
        private void startKinect()
        {
            // Connect to first connected sensor
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Set smoothing and prediction setting for hand updates
                TransformSmoothParameters smoothingParam = new TransformSmoothParameters();
                {
                    smoothingParam.Smoothing = 0.5f;
                    smoothingParam.Correction = 0.5f;
                    smoothingParam.Prediction = 0.5f;
                    smoothingParam.JitterRadius = 0.05f;
                    smoothingParam.MaxDeviationRadius = 0.04f;
                };

                this.sensor.SkeletonStream.Enable(smoothingParam);                      // Enable skeletal tracking
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;  // Set to seated mode

                // Enable the two depth streams in highest resolution
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                //this.sensor.DepthStream.Range = DepthRange.Near; // Near mode must be removed for skeleton tracking (look for way around??)

                // Initialize bitmaps to correct size for input streams
                this.depthPixels = new short[this.sensor.DepthStream.FramePixelDataLength];

                //add threading
                Thread streamThread = new Thread(new ThreadStart(streamThreadInit));
                Thread skeletonThread = new Thread(new ThreadStart(skeletonThreadInit));
                streamThread.Start();
                skeletonThread.Start();

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        private void streamThreadInit()
        {
            // Add an event handler to be called whenever there is new frame data
            this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
        }
        private void skeletonThreadInit()
        {
            // Skeleton even is kept on a different thread from the depth and RGB stream event
            this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
        }

        // Event for handling depth stream updates from the Kinect. Calls the finger update function
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            // "using" param ensures depth frame is disposed after use.
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                // Set depth frame parameters
                DFP.Bitmask = DepthImageFrame.PlayerIndexBitmaskWidth;
                DFP.Width = depthFrame.Width;
                DFP.Height = depthFrame.Height;

                #region DEBUG_HAND_OUTLINE
                depthFrame.CopyPixelDataTo(this.depthPixels);
                short[] tempDepth = new short[depthPixels.Length];

                depthFrame.CopyPixelDataTo(tempDepth);

                // Updates the hand position based on the kinematic data. Dampened by a multiple to reduce oscillation.
                //Hands[0].position_Pixels += 0.8 * Hands[0].elapsedMillis * Hands[0].velocity_Pixels;
                HandFrame HF = ProcessDepthImage.zoomOnHand(tempDepth, DFP, Hands[0]);

                IntPtr dataStartPtr = bmpData[0].Scan0;
                int dataLength = bmpData[0].Stride * bmpData[0].Height;

                // Print to debug left hand
                rgbValues = new byte[bmpData[0].Stride * bmpData[0].Height];
                for (int y = 1; y < HF.dx.GetLength(0); y++)
                    for (int x = 1; x < HF.dy.GetLength(1); x++)
                    {
                        int dD = (byte)(Math.Sqrt(HF.dx[y, x] * HF.dx[y, x] + HF.dy[y, x] * HF.dy[y, x]));

                        int val = debugBitmapSizes[0].Width * (y + (debugBitmapSizes[0].Height - HF.dx.GetLength(0)) / 2) + x + (debugBitmapSizes[0].Width - HF.dx.GetLength(1)) / 2;
                        if (dD > 5 && x + (debugBitmapSizes[0].Width - HF.dx.GetLength(1)) / 2 > 0 && x + (debugBitmapSizes[0].Width - HF.dx.GetLength(1)) / 2 < debugBitmapSizes[0].Width && y + (debugBitmapSizes[0].Height - HF.dx.GetLength(0)) / 2 > 0 && y + (debugBitmapSizes[0].Height - HF.dx.GetLength(0)) / 2 < debugBitmapSizes[0].Height)
                        {
                            rgbValues[4 * val + 0] = 255;
                            rgbValues[4 * val + 1] = 255;
                            rgbValues[4 * val + 2] = 255;
                            rgbValues[4 * val + 3] = 255;
                        }
                    }
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, dataStartPtr, dataLength);
                #endregion

                if (checkBox2.Checked)
                {
                    Hands[1].tracked = false;
                    foreach (ObjectPoint f in Hands[1].fingers)
                    {
                        f.tracked = false;
                    }
                }
                ProcessDepthImage.updateFingersFromDepth(depthPixels, DFP, Hands);
                this.FPScount++;
            }
        }

        // Event for handling skeleton stream updates.
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            skeleton = null;
            if (skeletons.Length != 0)
            {
                // Iterate through available skeletons and choose the one closest to the previous skeleton
                foreach (Skeleton skel in skeletons)
                {
                    // TODO: add in processing to only follow the closest skeleton to the previously tracked
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeleton = skel;
                        break;
                    }
                }
            }

            if (skeleton != null)
            {
                if (skeleton.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked)
                {
                    DepthImagePoint leftHandPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                        skeleton.Joints[JointType.HandLeft].Position, DepthImageFormat.Resolution640x480Fps30);
                    leftHandPoint.Y -= 15;  // Kinect sensor seems to set the hand a bit high. Adjusted.
                    Hands[(int)HandPoint.HANDS.LEFT].position_pixels_absolute = new Point3D { X = leftHandPoint.X, Y = leftHandPoint.Y, Z = leftHandPoint.Depth };
                    // Update Roll angle
                    Hands[(int)HandPoint.HANDS.LEFT].angle_from_elbow_degrees.X = (
                        180 + (180/Math.PI)*Math.Atan2(
                        skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ElbowLeft].Position.Y,
                        skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ElbowLeft].Position.X)) % 360;
                    // Update Pitch angle
                    Hands[(int)HandPoint.HANDS.LEFT].angle_from_elbow_degrees.Y = (
                        180 + (180 / Math.PI) * Math.Atan2(
                        skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.ElbowLeft].Position.Z,
                        skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ElbowLeft].Position.Y)) % 360;
                    // Update Yaw angle
                    Hands[(int)HandPoint.HANDS.LEFT].angle_from_elbow_degrees.Z = (
                        180 + (180 / Math.PI) * Math.Atan2(
                        skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.ElbowLeft].Position.Z,
                        skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ElbowLeft].Position.X)) % 360;
                }

                if (skeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                {
                    DepthImagePoint rightHandPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                        skeleton.Joints[JointType.HandRight].Position, DepthImageFormat.Resolution640x480Fps30);
                    rightHandPoint.Y -= 15;  // Kinect sensor seems to set the hand a bit high. Adjusted.
                    Hands[(int)HandPoint.HANDS.RIGHT].position_pixels_absolute = new Point3D { X = rightHandPoint.X, Y = rightHandPoint.Y, Z = rightHandPoint.Depth };
                    // Update Roll angle
                    Hands[(int)HandPoint.HANDS.RIGHT].angle_from_elbow_degrees.X = (
                        180 + (180 / Math.PI) * Math.Atan2(
                        skeleton.Joints[JointType.HandRight].Position.Y - skeleton.Joints[JointType.ElbowRight].Position.Y,
                        skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.ElbowRight].Position.X)) % 360;
                    // Update Pitch angle
                    Hands[(int)HandPoint.HANDS.LEFT].angle_from_elbow_degrees.Y = (
                        180 + (180 / Math.PI) * Math.Atan2(
                        skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.ElbowLeft].Position.Z,
                        skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ElbowLeft].Position.Y)) % 360;
                    // Update Yaw angle
                    Hands[(int)HandPoint.HANDS.RIGHT].angle_from_elbow_degrees.Z = (
                        180 + (180 / Math.PI) * Math.Atan2(
                        skeleton.Joints[JointType.HandRight].Position.Z - skeleton.Joints[JointType.ElbowRight].Position.Z,
                        skeleton.Joints[JointType.HandRight].Position.X - skeleton.Joints[JointType.ElbowRight].Position.X)) % 360;
                }
            } // end if skeleton != null

            else
            {
                Hands[(int)HandPoint.HANDS.LEFT].tracked = false;
                Hands[(int)HandPoint.HANDS.RIGHT].tracked = false;
            }

            this.Invalidate();
        }

        #region DRAW_FUNCTIONS
        private void OnPaint(object sender, PaintEventArgs e)
        {
            //in the Paint event we *ONLY* Paint!!!

            // To Fill to screen: e.Graphics.DrawImage(bitmap, 0, 0, ClientRectangle.Width, ClientRectangle.Height);
            //using (Graphics g = e.Graphics)
            //{
            // paint the bitmap on the form
            e.Graphics.DrawImageUnscaled(bodyBitmap, bodybitmapDimensions);
            e.Graphics.DrawRectangle(Pens.WhiteSmoke, bodybitmapDimensions);

            for (int i = 0; i < debugBitmaps.Length; i++)
            {
                e.Graphics.DrawRectangle(Pens.WhiteSmoke, debugBitmapSizes[i]);
            }

            debugBitmaps[0].UnlockBits(bmpData[0]);
            e.Graphics.DrawImageUnscaled(debugBitmaps[0], debugBitmapSizes[0]);
            this.bmpData[0] = debugBitmaps[0].LockBits(new Rectangle(0, 0, debugBitmapSizes[0].Width, debugBitmapSizes[0].Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, debugBitmaps[0].PixelFormat);

            DrawSkeleton(e.Graphics);
            DrawFrame1(e.Graphics);
            DrawFrame2(e.Graphics);

            this.HandDepth.Text = "Z: " + Hands[0].position_meters_absolute.Z.ToString();

            //}
        }
        private void DrawSkeleton(Graphics g)
        {
            if (skeleton != null)
            {
                System.Drawing.Point headPoint = new System.Drawing.Point(this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30).X, this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[JointType.Head].Position, DepthImageFormat.Resolution640x480Fps30).Y);
                System.Drawing.Point leftElbowPoint = new System.Drawing.Point(this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution640x480Fps30).X, this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[JointType.ElbowLeft].Position, DepthImageFormat.Resolution640x480Fps30).Y);
                System.Drawing.Point leftHandPoint = new System.Drawing.Point(Hands[0].X, Hands[0].Y);
                System.Drawing.Point rightElbowPoint = new System.Drawing.Point(this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution640x480Fps30).X, this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeleton.Joints[JointType.ElbowRight].Position, DepthImageFormat.Resolution640x480Fps30).Y);
                System.Drawing.Point rightHandPoint = new System.Drawing.Point(Hands[1].X, Hands[1].Y);
                System.Drawing.Point chestPoint = new System.Drawing.Point(headPoint.X, headPoint.Y + 40);

                // Draw Head / hand Points
                g.FillEllipse(Brushes.Gold, headPoint.X - 10, headPoint.Y - 10, 20, 20);
                g.FillEllipse(Brushes.Gold, leftHandPoint.X - 6, leftHandPoint.Y - 6, 12, 12);
                g.FillEllipse(Brushes.Gold, rightHandPoint.X - 6, rightHandPoint.Y - 6, 12, 12);

                // Draw Fingers Left
                for (int i = 0; i < Hands[0].fingers.Length; i++)
                {
                    if (Hands[0].fingers[i].tracked == true)
                    {
                        System.Drawing.Point fingerPoint = new System.Drawing.Point(Hands[0].fingers[i].X, Hands[0].fingers[i].Y);
                        g.FillEllipse(Brushes.White, fingerPoint.X - 4, fingerPoint.Y - 4, 8, 8);
                        g.DrawLine(Pens.Aqua, leftHandPoint, fingerPoint);

                        // If the DrawAngle box is checked, draw the angle of the fingers from the hand
                        if (this.checkBox1.Checked)
                        {
                            g.DrawString((Math.Truncate(Hands[0].fingers[i].angle_from_hand_degrees.X * 100) / 100).ToString(),
                                System.Drawing.SystemFonts.SmallCaptionFont, Brushes.White,
                                (int)(Hands[0].X + 1.3 * (fingerPoint.X - Hands[0].X - 10)),
                                (int)(Hands[0].Y + 1.5 * (fingerPoint.Y - Hands[0].Y) - 5));
                        }
                    }
                }

                // Draw Fingers Right
                for (int i = 0; i < Hands[1].fingers.Length; i++)
                {
                    if (Hands[1].fingers[i].tracked == true)
                    {
                        System.Drawing.Point fingerPoint = new System.Drawing.Point(Hands[1].fingers[i].X, Hands[1].fingers[i].Y);
                        g.FillEllipse(Brushes.White, fingerPoint.X - 4, fingerPoint.Y - 4, 8, 8);
                        g.DrawLine(Pens.Aqua, rightHandPoint, fingerPoint);

                        // If the DrawAngle box is checked, draw the angle of the fingers from the hand
                        if (this.checkBox1.Checked)
                        {
                            g.DrawString((Math.Truncate(Hands[1].fingers[i].angle_from_hand_degrees.X * 100) / 100).ToString(),
                                System.Drawing.SystemFonts.SmallCaptionFont, Brushes.White,
                                (int)(Hands[1].X + 1.3 * (fingerPoint.X - Hands[1].X - 10)),
                                (int)(Hands[1].Y + 1.5 * (fingerPoint.Y - Hands[1].Y) - 5));
                        }
                    }
                }

                g.DrawLine(Pens.Goldenrod, headPoint, chestPoint);
                g.DrawLine(Pens.Goldenrod, chestPoint, leftElbowPoint);
                g.DrawLine(Pens.Goldenrod, leftElbowPoint, leftHandPoint);
                g.DrawLine(Pens.Goldenrod, chestPoint, rightElbowPoint);
                g.DrawLine(Pens.Goldenrod, rightElbowPoint, rightHandPoint);
            }
        }
        private void DrawFrame1(Graphics g)
        {
            System.Drawing.Point leftHand = new System.Drawing.Point(Hands[0].X, Hands[0].Y);
            for (int i = 0; i < Hands[0].fingers.Length; i++)
            {
                System.Drawing.Point leftFinger = new System.Drawing.Point(Hands[0].fingers[i].X - Hands[0].X + debugBitmapSizes[0].Left + debugBitmapSizes[0].Width / 2, Hands[0].fingers[i].Y - Hands[0].Y + debugBitmapSizes[0].Top + debugBitmapSizes[0].Height / 2);
                g.FillEllipse(Brushes.Yellow, leftFinger.X - 6, leftFinger.Y - 6, 8, 8);
                g.DrawLine(Pens.BlueViolet, new System.Drawing.Point(debugBitmapSizes[0].Left + debugBitmapSizes[0].Width / 2, debugBitmapSizes[0].Top + debugBitmapSizes[0].Height / 2), leftFinger);
            }

            System.Drawing.Point Finger = new System.Drawing.Point(Hands[0].fingers[0].X - Hands[0].X + debugBitmapSizes[0].Left + debugBitmapSizes[0].Width / 2, Hands[0].fingers[0].Y - Hands[0].Y + debugBitmapSizes[0].Top + debugBitmapSizes[0].Height / 2);
            g.FillEllipse(Brushes.Green, Finger.X - 6, Finger.Y - 6, 8, 8);
        }
        private void DrawFrame2(Graphics g)
        {
            System.Drawing.Point leftHand = new System.Drawing.Point(Hands[1].X, Hands[1].Y);
            for (int i = 0; i < Hands[1].fingers.Length; i++)
            {
                System.Drawing.Point leftFinger = new System.Drawing.Point(Hands[1].fingers[i].X - Hands[1].X + debugBitmapSizes[2].Left + debugBitmapSizes[2].Width / 2, Hands[1].fingers[i].Y - Hands[1].Y + debugBitmapSizes[2].Top + debugBitmapSizes[2].Height / 2);
                g.FillEllipse(Brushes.Yellow, leftFinger.X - 6, leftFinger.Y - 6, 8, 8);
                g.DrawLine(Pens.BlueViolet, new System.Drawing.Point(debugBitmapSizes[2].Left + debugBitmapSizes[2].Width / 2, debugBitmapSizes[2].Top + debugBitmapSizes[2].Height / 2), leftFinger);
            }

            System.Drawing.Point Finger = new System.Drawing.Point(Hands[1].fingers[0].X - Hands[1].X + debugBitmapSizes[2].Left + debugBitmapSizes[2].Width / 2, Hands[1].fingers[0].Y - Hands[1].Y + debugBitmapSizes[2].Top + debugBitmapSizes[2].Height / 2);
            g.FillEllipse(Brushes.Green, Finger.X - 6, Finger.Y - 6, 8, 8);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.FPSInfo.Text = "FPS: " + this.FPScount.ToString();
            FPScount = 0;
        }
        #endregion
    }
}
