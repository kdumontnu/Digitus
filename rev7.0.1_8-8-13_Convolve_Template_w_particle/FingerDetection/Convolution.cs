using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FingerDetection
{
    public static class Convolution
    {
        // Runs a convolution of a separable function, using a 1D Kernal that's been separated
        public static void seperableConv(double[] kernal, HandFrame handFrame)
        {
            int imageWidth = handFrame.width;
            int imageHeight = handFrame.height;
            int kernLength = kernal.Length;
            float[,] tempx = new float[imageHeight, imageWidth];
            float[,] tempy = new float[imageHeight, imageWidth];
            double sumx;
            double sumy;

            // Run through convolution in the x direction
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth - kernLength; x++)
                {
                    sumx = 0;
                    sumy = 0;
                    for (int n = 0; n < kernLength; n++)
                    {
                        sumx += handFrame.dx[y, x + n] * kernal[n];
                        sumy += handFrame.dy[y, x + n] * kernal[n];
                    }

                    tempx[y, x + Convert.ToInt32(kernLength / 2)] = (float)sumx;
                    tempy[y, x + Convert.ToInt32(kernLength / 2)] = (float)sumy;                   
                }
            }

            // Run through the convolution in the y direction
            for (int x = 0; x < imageWidth; x++)
            {
                for (int y = 0; y < imageHeight - kernLength; y++)
                {
                    sumx = 0;
                    sumy = 0;
                    for (int n = 0; n < kernLength; n++)
                    {
                        sumx += tempx[y + n, x] * kernal[n];
                        sumy += tempy[y + n, x] * kernal[n];
                    }

                    // Scales value back down
                    handFrame.dx[y + Convert.ToInt32(kernLength / 2), x] = (float)sumx;
                    handFrame.dy[y + Convert.ToInt32(kernLength / 2), x] = (float)sumy;
                }
            }
        }
    }
}
