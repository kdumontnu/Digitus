using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FingerDetection
{
    // Contains a 3-Dimension double vector
    public class Point3D
    {
        public double X;
        public double Y;
        public double Z;

        public static Point3D operator /(Point3D p1, Point3D p2)
        {
            return new Point3D
            {
                X = (p1.X / p2.X),
                Y = (p1.Y / p2.Y),
                Z = (p1.Z / p2.Z),
            };
        }
        public static Point3D operator /(Point3D p1, int val)
        {
            return new Point3D
            {
                X = p1.X / val,
                Y = p1.Y / val,
                Z = p1.Z / val,
            };
        }
        public static Point3D operator /(Point3D p1, double val)
        {
            return new Point3D
            {
                X = p1.X / val,
                Y = p1.Y / val,
                Z = p1.Z / val,
            };
        }
        public static Point3D operator *(Point3D p1, Point3D p2)
        {
            return new Point3D
            {
                X = (p1.X * p2.X),
                Y = (p1.Y * p2.Y),
                Z = (p1.Z * p2.Z),
            };
        }
        public static Point3D operator *(int val, Point3D p1)
        {
            return new Point3D
            {
                X = p1.X * val,
                Y = p1.Y * val,
                Z = p1.Z * val,
            };
        }
        public static Point3D operator *(double val, Point3D p1)
        {
            return new Point3D
            {
                X = p1.X * val,
                Y = p1.Y * val,
                Z = p1.Z * val,
            };
        }
        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            return new Point3D
            {
                X = (p1.X + p2.X),
                Y = (p1.Y + p2.Y),
                Z = (p1.Z + p2.Z),
            };
        }
        public static Point3D operator +(Point3D p1, int val)
        {
            return new Point3D
            {
                X = p1.X + val,
                Y = p1.Y + val,
                Z = p1.Z + val,
            };
        }
        public static Point3D operator +(Point3D p1, double val)
        {
            return new Point3D
            {
                X = p1.X + val,
                Y = p1.Y + val,
                Z = p1.Z + val,
            };
        }
        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D
            {
                X = p1.X - p2.X,
                Y = p1.Y - p2.Y,
                Z = p1.Z - p2.Z,
            };
        }
        public static Point3D operator -(Point3D p1, int val)
        {
            return new Point3D
            {
                X = p1.X - val,
                Y = p1.Y - val,
                Z = p1.Z - val,
            };
        }
        public static Point3D operator -(Point3D p1, double val)
        {
            return new Point3D
            {
                X = p1.X - val,
                Y = p1.Y - val,
                Z = p1.Z - val,
            };
        }
    }

    // Contains virtual base class for object points, like hands and fingers
    public abstract class ObjectPoint
    {
        // maintain a stopwatch
        protected Stopwatch sw;

        // previous time used for calculating velocity and acceleration
        public long previousTime;

        // returns the elapsed Milliseconds since the last position was set
        public int elapsedMillis
        {
            get
            {
                return (int)(sw.ElapsedMilliseconds - previousTime);
            }
        }

        // contain precise vectors for position, velocity, acceleration - private
        protected Point3D _position_pixels = new Point3D { X = 0, Y = 0, Z = 0 };
        protected Point3D _velocity_pixels = new Point3D { X = 0, Y = 0, Z = 0 };

        // is the object being tracked?
        public bool tracked = false;

        // contain Pixel location for the X, Y, and Depth
        public short X
        {
            get
            {
                short temp = _position_pixels.X > 639 ? (short)639 : (short)_position_pixels.X;
                temp = temp < 0 ? (short)0 : temp;
                return Convert.ToInt16(temp);
            }
        }
        public short Y
        {
            get
            {
                short temp = _position_pixels.Y > 479 ? (short)479 : (short)_position_pixels.Y;
                temp = temp < 0 ? (short)0 : temp;
                return Convert.ToInt16(temp);
            }
        }
        public short Depth
        {
            get
            {
                short temp = _position_pixels.Z > Int16.MaxValue ? Int16.MaxValue : (short)_position_pixels.Z;
                temp = temp < 0 ? (short)0 : temp;
                return Convert.ToInt16(temp);
            }
        }

        // contain precise vectors for position, velocity, acceleration
        public virtual Point3D position_pixels_absolute
        {
            get
            {
                return _position_pixels;
            }
            set
            {
                int eTime = elapsedMillis;
                previousTime = sw.ElapsedMilliseconds;

                // If no time has elapsed, don't change
                if (eTime == 0)
                    return;

                // If position has not been set, set it
                if (_position_pixels == null)
                    _position_pixels = value;

                else
                {
                    // Store velocity temporarily
                    _velocity_pixels = (value - _position_pixels) / eTime;

                    tracked = true;
                    _position_pixels = value;
                }
            }
        }
        public Point3D velocity_pixels_absolute
        { 
            get
            {
                return _velocity_pixels;
            }
        }

        public Point3D position_meters_absolute
        {
            get
            {
                return new Point3D
                {
                    X = (_position_pixels.Z / 1000) * DetectionParameters.pixelsToMeters * _position_pixels.X,
                    Y = (_position_pixels.Z / 1000) * DetectionParameters.pixelsToMeters * (480 - _position_pixels.Y),
                    Z = _position_pixels.Z / 1000
                };
            }
        }

        // Constructor
        public ObjectPoint()
        {
            sw = Stopwatch.StartNew();
            previousTime = sw.ElapsedMilliseconds;
        }

        public static Point3D PixelsToMeters(Point3D pixels)
        {
            return new Point3D
            {
                X = (pixels.Z / 1000) * DetectionParameters.pixelsToMeters * pixels.X,
                Y = (pixels.Z / 1000) * DetectionParameters.pixelsToMeters * pixels.Y,
                Z = pixels.Z / 1000
            };
        }
    }

    // Derived hand class
    public class HandPoint : ObjectPoint
    {
        public enum HANDS { LEFT, RIGHT };
        public FingerPoint[] fingers = new FingerPoint[5];
        public Point3D angle_from_elbow_degrees = new Point3D() { X = 270 };
        
        public void InitializeFingers(HandPoint h)
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i] = new FingerPoint(h);
            }
        }
    }

    // Derived finger class
    public class FingerPoint : ObjectPoint
    {
        public enum FINGERS { THUMB, INDEX, MIDDLE, RING, PINKY };

        HandPoint _containging_hand;

        public Point3D position_pixels_from_hand = new Point3D();
        public Point3D velocity_pixels_from_hand = new Point3D();

        // X = Roll; Y = Pitch; Z = Yaw
        public Point3D angle_from_hand_degrees = new Point3D();

        // Tunable elements for kinematic kalman filter
        static double ACCEL_SIGMA_SQUARED = 3; // 2 pixels per second..
        static double MEASUREMENT_SIGMA_SQUARED = 0.008; // measurement error
        // ^ Make dependant on match coefficient??

        // Initialize covarience with large [0,0] and [1,1]
        double P11 = 100;
        double P12 = 0;
        double P21 = 0;
        double P22 = 100;

        public FingerPoint ( HandPoint h)
        {
            _containging_hand = h;
        }

        public FingerPoint(){}

        // To Do: provide input that influences based on better match correlations.
        public void updateKalman(Point3D z)
        {
            // Get time elapsed and update
            double dt = elapsedMillis/1000.0;
            double dt_squared = dt * dt;
            previousTime = sw.ElapsedMilliseconds;

            tracked = true;

            // Update predicted position
            position_pixels_from_hand += dt * velocity_pixels_from_hand;

            // Update covarience
            P11 += dt * P21 + dt * P12 + dt_squared * P22 + ACCEL_SIGMA_SQUARED * dt_squared * dt_squared / 4;
            P12 += dt * P22 + ACCEL_SIGMA_SQUARED * dt_squared * dt / 2;
            P21 += dt * P22 + ACCEL_SIGMA_SQUARED * dt_squared * dt / 2;
            P22 += dt_squared;

            // Update kalman gain
            double K1 = P11 / (P11 + MEASUREMENT_SIGMA_SQUARED);
            double K2 = P21 / (P11 + MEASUREMENT_SIGMA_SQUARED);

            // Update state variables
            position_pixels_from_hand += K1 * (z - position_pixels_from_hand);
            velocity_pixels_from_hand = K2 * velocity_pixels_from_hand;

            // post update covarience
            P11 -= K1 * P11;
            P12 -= K1 * P12;
            P21 -= K1 * P21;
            P22 -= K1 * P22;

            this._position_pixels = position_pixels_from_hand + _containging_hand.position_pixels_absolute;// +_containging_hand.elapsedMillis * _containging_hand.velocity_pixels_absolute; // <-- causes drift
        }
    }
}
