using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FingerDetection
{
    public static class DetectionParameters
    {
        // Detection Parameters are automatically loaded from an XML file
        public static string version = "6.1.1 - " + System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString();
        internal static double[,] dxTemplate = ReadXML.getDoubleMatrix("XML_Files/DetectionParameters.xml", "dxTemplate");
        internal static double[,] dyTemplate = ReadXML.getDoubleMatrix("XML_Files/DetectionParameters.xml", "dyTemplate");
        internal static int dxThreshold = ReadXML.getIntValue("XML_Files/DetectionParameters.xml", "dxThreshold");
        internal static int dyThreshold = ReadXML.getIntValue("XML_Files/DetectionParameters.xml", "dyThreshold");
        internal static int radiusFromHandPixel = ReadXML.getIntValue("XML_Files/DetectionParameters.xml", "radiusFromHandPixel");
        internal static double[] seperableKernal = ReadXML.getDoubleArray("XML_Files/DetectionParameters.xml", "seperableKernal");
        internal static double[] gaussianArray = ReadXML.getDoubleArray("XML_Files/DetectionParameters.xml", "gaussianMatrix");
        internal static double[] StandardGaussianArray = ReadXML.getDoubleArray("XML_Files/DetectionParameters.xml", "StandardGaussianMatrix");
        internal static double pixelsToMeters = 1.765/1000;

        /// <summary>
        /// Returns a Normalized gaussian value, given a distance and threshold, which is set to 0.5
        /// Sigma is 34.5
        /// </summary>
        /// <param name="d">A double representing the distance away</param>
        /// <param name="r">An Integer which will be normalized to the index returning 0.5. Usually, the radius of a finger</param>
        public static double gaussian(double d, int r)
        {
            // Index of 40 is 0.5
            if (d < 0)
                return 0;

            int i = Convert.ToInt32(40 * d / r);
            // Clips the index at the max value.
            return gaussianArray[i >= gaussianArray.Length ? gaussianArray.Length - 1 : i];
        }

        /// <summary>
        /// Returns a Normalized ( standardGaussian(0) = 1 ) standard gaussian value, given a distance
        /// </summary>
        /// <param name="x">A double representing the index</param>
        /// <param name="sigma">a double representing the sigma value</param>
        public static double StandardGaussian(double x, double sigma)
        {
            // Normalize index to the standard with sigma of 10 (I hope...)
            int i = Convert.ToInt32(x/(sigma*sigma/100));
            // Clips the index at the max value.
            return StandardGaussianArray[i >= StandardGaussianArray.Length ? StandardGaussianArray.Length - 1 : i];
        }
    }
}
