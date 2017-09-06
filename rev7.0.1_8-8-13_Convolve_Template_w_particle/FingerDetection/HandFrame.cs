using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerDetection
{
    public class HandFrame
    {
        public class Center
        {
            public int X;
            public int Y;
        }

        public short[,] abs;
        public float[,] dx;
        public float[,] dy;
        public bool[,] evaluated;

        public int width;
        public int height;

        public int radius;

        public Center center = new Center();

        private int xShift;
        private int yShift; 

        // Constructor
        public HandFrame(int DepthFrameWidth, int DepthFrameHeight, int handX, int handY, int r)
        {
            this.center.X = handX;
            this.center.Y = handY;
            this.radius = r;

            this.width = getOutMatrixWidth(DepthFrameWidth, handX, r, out xShift);
            this.height = getOutMatrixHeight(DepthFrameHeight, handY, r, out yShift);

            // get rid of + 1
            abs = new short[height,width];
            dx = new float[height,width];
            dy = new float[height,width];
            evaluated = new bool[height, width];
        }

        /**
         * Returns the width, in pixels, of the hand frame matrix
        **/
        private static short getOutMatrixWidth(int w, int x, int r, out int shift)
        {
            int rightSide = x + r;
            int leftSide = x - r;

            // set x shift based on overflow from left and right edge of Depth Image
            shift = 0;
            shift = leftSide < 0 ? leftSide : shift;
            shift = rightSide > w - 1 ? rightSide - w + 1 : shift;

            return (short)(2 * r - Math.Abs(shift) + 1);
        }

        /**
         * Returns the height, in pixels, of the hand frame matrix
        **/
        private static short getOutMatrixHeight(int h, int y, int r, out int shift)
        {
            int bottom = y + r;
            int top = y - r;

            // set y shift based on overflow from top and bottom of Depth Image
            shift = 0;
            shift = top < 0 ? top : shift;
            shift = bottom > h - 1 ? bottom - h + 1 : shift;

            return (short)(2 * r - Math.Abs(shift) + 1);
        }
    }
}
