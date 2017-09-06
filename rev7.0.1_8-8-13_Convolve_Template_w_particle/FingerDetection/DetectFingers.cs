using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FingerDetection
{
    internal class DetectFingers
    {
        // Radius around a selected finger to remove all other particles from
        static int SIZE_OF_FINGER_PIXELS = 4;

        public static void Detect(particle[] particles, HandPoint h, HandFrame hf)
        {
            int radius = hf.radius;

            // Initialize an array of particles matching fingers
            particle[] f = new particle[5];
            for ( int i = 0; i < f.Length; i++ )
                f[i] = new particle((short)(radius - 1), (short)(radius - 1));

            if (particles != null)
            {
                // for each finger
                for (int i = 0; i < f.Length; i++)
                {
                    //h.fingers[i].position_Pixels += 0.1 * h.fingers[i].elapsedMillis * h.fingers[i].velocity_Pixels;

                    //double a = .25;
                    // get the best value
                    for (int j = 0; j < particles.Length; j++)
                    {
                        if (particles[j] != null && particles[j].match_coefficient > f[i].match_coefficient)
                        {
                            f[i].x = particles[j].x;
                            f[i].y = particles[j].y;
                            f[i].z = hf.abs[particles[j].y, particles[j].x];
                            f[i].match_coefficient = particles[j].match_coefficient;

                            //f[i].matchCoefficient /= (hf.abs[f[i].y, f[i].x] - h.Depth + 100);
                            //f[i].matchCoefficient /= ((1 - a) + a * DetectionParameters.gaussian(Math.Sqrt(
                            //     (particles[j].x - (h.fingers[i].X - h.X + radius))
                            //    * (particles[j].x - (h.fingers[i].X - h.X + radius)) 
                            //    + (particles[j].y - (h.fingers[i].Y - h.Y + radius)) 
                            //    * (particles[j].y - (h.fingers[i].Y - h.Y + radius))), 10));
                        }
                    }
                    
                    // remove all particles within the finger area
                    for (int j = 0; j < particles.Length; j++)
                    {
                        // Remove particles within the radius of a finger
                        if (particles[j] != null &&
                            Math.Sqrt(
                            (particles[j].x - f[i].x) * (particles[j].x - f[i].x) + 
                            (particles[j].y - f[i].y) * (particles[j].y - f[i].y)) <= SIZE_OF_FINGER_PIXELS)
                        //if (particles[j] != null && particles[j].x == f[i].x && particles[j].y == f[i].y)
                        {
                            particles[j] = null;
                        }
                    }

                    // Calculate rotations for the finger
                    SetFingerRotations(f[i],hf.abs[f[i].y,f[i].x], h.angle_from_elbow_degrees, radius);
                }
            }

            Array.Sort(f, Compare); // Sort the 5 fingers clockwise around the hand

            for (int i = 0; i < h.fingers.Length; i++)
            {
                h.fingers[i].updateKalman(new Point3D { X = f[i].x - radius, Y = f[i].y - radius, Z = hf.abs[f[i].y, f[i].x] - h.position_pixels_absolute.Z});
                h.fingers[i].angle_from_hand_degrees.X = f[i].angle_from_hand_degrees.X;
            }
        }

        public static int Compare(particle p1, particle p2)
        {
            //double pitch1 = Math.Sin(p1.angle_from_hand_degrees.Y * 3.14159 / 180);
            //double pitch2 = Math.Sin(p2.angle_from_hand_degrees.Y * 3.14159 / 180);
            return (p1.angle_from_hand_degrees.X.CompareTo(p2.angle_from_hand_degrees.X));
            //    (pitch1*p1.angle_from_hand_degrees.X + (1-pitch1)*p1.angle_from_hand_degrees.Z).CompareTo(
            //    pitch2*p2.angle_from_hand_degrees.X + (1-pitch2)*p2.angle_from_hand_degrees.Z));
        }

        public static void SetFingerRotations(particle f, int depth, Point3D h_rotation, int radius)
        {
            Point3D meters = ObjectPoint.PixelsToMeters(new Point3D(){X = f.x - radius, Y = f.y - radius, Z = depth});
            f.angle_from_hand_degrees.X = -Math.Atan2(meters.Y, meters.X) * 180 / 3.14159;
            f.angle_from_hand_degrees.X += (720 - h_rotation.X);
            f.angle_from_hand_degrees.X = f.angle_from_hand_degrees.X % 360;

            //f.angle_from_hand_degrees.Y = -Math.Atan2(meters.Y, -meters.Z) * 180 / 3.14159;
            //f.angle_from_hand_degrees.Y += 720;
            //f.angle_from_hand_degrees.Y = f.angle_from_hand_degrees.Y % 360;

            //f.angle_from_hand_degrees.Z = -Math.Atan2(meters.X, meters.Z) * 180 / 3.14159;
            //f.angle_from_hand_degrees.Z += (720 - h_rotation.Z);
            //f.angle_from_hand_degrees.Z = f.angle_from_hand_degrees.Z % 360;
        }
    }
}