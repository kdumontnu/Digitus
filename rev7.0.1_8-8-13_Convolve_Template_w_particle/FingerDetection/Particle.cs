using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FingerDetection
{
    internal class particle
    {
        // The current x and y position of the particle on the hand map
        public short x, y, z;
        // The most recent match coefficient with the template
        public double match_coefficient;
        // The previous match coefficient with the template
        public double previous_match_coefficient;
        // The previous direction the particle moved. Can be 1,2,3,4 respective to x - 1, y - 1, x + 1, y + 1 quadrant
        // Should be made to two bool for simpler rotation math
        public sbyte previous_direction;
        // Specify whether to continue evaluating the particle
        public bool alive;

        // Decide for each quadrent where to move the pixel.
        public static sbyte[] moveX = { -1, 0, 1, 0 };
        public static sbyte[] moveY = { 0, 1, 0, -1 };

        // X = Roll; Y = Pitch; Z = Yaw
        public Point3D angle_from_hand_degrees = new Point3D();

        // Constructor
        public particle(short x, short y)
        {
            this.x = x;
            this.y = y;
            this.alive = true;
            this.previous_direction = -1;
            previous_match_coefficient = 0;
            match_coefficient = 0;
        }

        // Run one iteration of life-cycle for each particle and update
        internal static void runTrace(particle[] p, double[,] xT, double[,] yT, HandFrame image)
        {
            int templateHeight = xT.GetLength(0);
            int templateWidth = xT.GetLength(1);
            int halfTemplateHeight = Convert.ToInt32(templateHeight / 2);
            int halfTemplateWidth = Convert.ToInt32(templateWidth / 2);

            // for each particle
            for (int i = 0; i < p.Length; i++)
            {
                // check if initialized
                if ((p[i] != null) && (p[i].alive))
                {
                    if (image.evaluated[p[i].y, p[i].x])
                    {
                        p[i] = null;
                        continue;
                    }
                    image.evaluated[p[i].y, p[i].x] = true;

                    // run match for each direction
                    int testDirection = p[i].previous_direction; // set the initial direction to test to the previous direction
                    double testCoefficient = p[i].previous_match_coefficient;
                    double tempCoefficient;

                    // test each direction around the point until you reach the starting point
                    // adds one more iteration if the 'first' bit is set.
                    int d = (p[i].previous_direction + 1) % 4;
                    if (p[i].previous_direction < 0)
                        p[i].previous_direction = 0;

                    do
                    {
                        // Run match
                        tempCoefficient = matchFilter(xT, yT, p[i].x + moveX[d] - halfTemplateWidth, p[i].y + moveY[d] - halfTemplateHeight, templateWidth, templateHeight, image);

                        // Test if value is better than previous direction
                        if (tempCoefficient > testCoefficient)
                        {
                            testCoefficient = tempCoefficient;
                            testDirection = d;
                        }
                        d = ++d % 4;
                    } while (d != p[i].previous_direction);

                    
                    // Test if final value is higher than the current center value
                    if (testCoefficient > p[i].match_coefficient)
                    {
                        //if (image.evaluate[p[i].y, p[i].x])
                        //{
                        //    p[i] = null;
                        //    continue;
                        //}
                        // Update particle values

                        // adjust the previous direction so that it is 180degrees when it is saved.
                        p[i].previous_direction = (sbyte)((testDirection + 2) % 4);
                        p[i].previous_match_coefficient = p[i].match_coefficient;
                        p[i].match_coefficient = testCoefficient;
                        p[i].x = (short)(p[i].x + moveX[testDirection]);
                        p[i].y = (short)(p[i].y + moveY[testDirection]);
                        //image.evaluate[p[i].y, p[i].x] = true;
                    }
                    // Otherwise, stop evaluating.
                    else
                    {
                        p[i].alive = false;
                    }
                }
            }
        }

        // Run one match filter
        static double matchFilter(double[,] xT, double[,] yT, int startX, int startY, int templateWidth, int templateHeight, HandFrame image)
        {
            // make sure we're in bounds
            if ((startX - 1 < 0) || (startX + templateWidth + 1 > image.width) || (startY - 1 < 0) || (startY + templateHeight + 1> image.height))
                return 0;

            // Reset the sum
            double sum = 0;

            // iterate through x and y values running a absolute value of the difference
            for (int y = 0; y < templateHeight; y+=2)
            {
                for (int x = 0; x < templateWidth; x+=2)
                {
                    sum += image.dy[startY + y, startX + x] * yT[y, x] + image.dx[startY + y, startX + x] * xT[y, x];
                }
            }
            return sum;
        }
    }
}