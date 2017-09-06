using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Kinect;

namespace FingerDetection
{
    // how to organize the hand frame?? Possibly by setting the parameters x_shift, y_shift, w, h, etc then use script to them
    // Keep the dx, dy, abs values separate???
    public struct DepthFrameParams
    {
        public int Bitmask;
        public int Width;
        public int Height;
    }

    public static class ProcessDepthImage
    {
        public static int updateFingersFromDepth(short[] depthFrame, DepthFrameParams DFP, HandPoint[] hands)
        {
            // Check left hand
            if (hands[(int)HandPoint.HANDS.LEFT].tracked)
            {
                // Zoom on hand and get dx, dy
                HandFrame HF = ProcessDepthImage.zoomOnHand(depthFrame, DFP, hands[(int)HandPoint.HANDS.LEFT]);
                // Smooth dx, dy;
                Convolution.seperableConv(DetectionParameters.seperableKernal, HF);
                hands[(int)HandPoint.HANDS.LEFT].tracked = ParticleGradient(HF, hands[(int)HandPoint.HANDS.LEFT]);
            }

            // Check right hand
            if (hands[(int)HandPoint.HANDS.RIGHT].tracked)
            {
                // Zoom on hand and get dx, dy
                HandFrame HF = ProcessDepthImage.zoomOnHand(depthFrame, DFP, hands[(int)HandPoint.HANDS.RIGHT]);
                // Smooth dx, dy;
                Convolution.seperableConv(DetectionParameters.seperableKernal, HF);
                hands[(int)HandPoint.HANDS.RIGHT].tracked = ParticleGradient(HF, hands[(int)HandPoint.HANDS.RIGHT]);
            }

            return 1;
        }

        public static HandFrame zoomOnHand(short[] depthFrame, DepthFrameParams DFSettings, ObjectPoint handPos)
        {
            HandFrame HF = new HandFrame(DFSettings.Width, DFSettings.Height, handPos.X, handPos.Y, getRadiusFromHand(handPos.Depth));

            // converts the 2d X and Y from output matrix to the 1d array Depth Image: DeptImgWidth*Y + X
            // should have xShift and yShift incorporated??????
            int startPoint = DFSettings.Width * Math.Max(1, (handPos.Y - HF.radius)) + Math.Max(1, handPos.X - HF.radius);
            int endPoint = Math.Min(depthFrame.Length - 1, DFSettings.Width * (handPos.Y + HF.radius) + handPos.X + HF.radius);

            int x = 0;
            int y = 0;

            for (int i = startPoint; i <= endPoint; i++)
            {
                // wraps the linear array to create the 2D Matrix
                if (x >= HF.width)
                {
                    i += DFSettings.Width - HF.width - 1;
                    x = 0;
                    y++;
                    continue;
                }

                depthFrame[i] = (short)(depthFrame[i] >> DFSettings.Bitmask);

                // Kinect reports unknown values as -1 or 0
                if (depthFrame[i] <= 0 || depthFrame[i] >= handPos.Depth + HF.radius)
                    depthFrame[i] = (short)(handPos.Depth + HF.radius);

                HF.abs[y, x] = (short)(depthFrame[i]);
                HF.dx[y, x] = threshold(depthFrame[i] - depthFrame[i - 1], FingerDetection.DetectionParameters.dxThreshold);
                HF.dy[y, x] = threshold(depthFrame[i] - depthFrame[i - DFSettings.Width], FingerDetection.DetectionParameters.dyThreshold);

                x++;
            }

            return HF;
        }

        /**
         * returns and integer value of how far from the hand to zoom the depth Image
         * Will scale depending on hand depth
         **/
        private static int getRadiusFromHand(double d)
        {
            // TO DO: scale value based on hand depth
            return DetectionParameters.radiusFromHandPixel;
        }

        /** 
         * sets given plus/minus threshold on value
         **/
        private static float threshold(int var, int th)
        {
            if (var > th)
                return th;
            if (var < -th)
                return -th;
            else
                return var;
        }

        /**
         * Match tempalte across entire image
        **/
        private static double[,] MapGradient(HandFrame handFrame, double[,] xTemplate, double[,] yTemplate)
        {
            int imageWidth = handFrame.width;
            int imageHeight = handFrame.height;
            int templateWidth = xTemplate.GetLength(1);
            int templateHeight = xTemplate.GetLength(0);

            double[,] outMatrix = new double[imageHeight - templateHeight, imageWidth - templateWidth];

            // Iterate through each of the image points
            for (int Y = 0; Y < imageHeight - templateHeight; Y++)
            {
                for (int X = 0; X < imageWidth - templateWidth; X++)
                {
                    double sum = 0;
                    // Iterate through each point in the template
                    for (int y = 0; y < templateHeight; y++)
                    {
                        for (int x = 0; x < templateWidth; x++)
                        {
                            sum += Math.Sqrt(Math.Pow(handFrame.dy[Y + y, X + x] - yTemplate[y, x], 2) + Math.Pow(handFrame.dx[Y + y, X + x] - xTemplate[y, x], 2));
                        }
                    }
                    outMatrix[Y, X] = sum;
                }
            }
            return outMatrix;
        }

        private static bool ParticleGradient(HandFrame handFrame, HandPoint handObj)
        {
            int radius = DetectionParameters.radiusFromHandPixel;
            int imageWidth = handFrame.width;
            int imageHeight = handFrame.height;
            int templateWidth = DetectionParameters.dxTemplate.GetLength(1);
            int templateHeight = DetectionParameters.dxTemplate.GetLength(0);

            short x = 0;
            short y = 0;
            short dx = 6;
            short dy = (short)(Math.Sqrt(3) * dx / 2);

            int distanceBetweenParticles = (int)(templateWidth * (0.4)); // Half the width of a "finger"

            // TO DO: center the particles
            int numParticles = (imageWidth / dx) * (imageHeight / dy);
            particle[] particles = new particle[numParticles];

            // Initialize particles
            for (int i = 0; i < particles.Length; i++)
            {
                // Set initial x and y
                //short x = (short)((i * distanceBetweenParticles) % imageWidth);
                //short y = (short)(distanceBetweenParticles * ((i * distanceBetweenParticles) / imageWidth));

                // If the particle is within the depth range, initialize it.
                if (((handFrame.abs[y, x] > (handObj.Depth - 100)) && (handFrame.abs[y, x] < (handObj.Depth + 100))) && (Math.Sqrt((x - radius) * (x - radius) + (y - radius) * (y - radius)) < (radius - 1)))
                    particles[i] = new particle((short)(x), (short)(y));

                x += dx;
                if (x >= imageWidth)
                {
                    x = 0;
                    y += dy;
                    if (y % 2 == 1)
                    {
                        x += (short)(dx / 2);
                    }
                }
            }

            // run for cycles up to the template width.    
            int iterations = (int)(templateWidth / 2);
            for (int t = 0; t < iterations; t++)
                particle.runTrace(particles, DetectionParameters.dxTemplate, DetectionParameters.dyTemplate, handFrame);

            DetectFingers.Detect(particles, handObj, handFrame);

            return true;
        }
    }
}

