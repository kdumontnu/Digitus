using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FingerDetection
{
    public static class ReadXML
    {
        public static int getIntValue(string file, string param)
        {
            XmlReader xmlRead = XmlReader.Create(file);

            while (xmlRead.Read())
            {
                if (xmlRead.Name == param)
                {
                    if (xmlRead.GetAttribute(0) == "int")
                    {
                        while (xmlRead.NodeType != XmlNodeType.EndElement)
                        {
                            xmlRead.Read();
                            if (xmlRead.Name == "value")
                            {
                                while (xmlRead.NodeType != XmlNodeType.EndElement)
                                {
                                    xmlRead.Read();
                                    if (xmlRead.NodeType == XmlNodeType.Text)
                                        return Int16.Parse(xmlRead.Value);
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public static double[] getDoubleArray(string file, string param)
        {
            XmlReader xmlRead = XmlReader.Create(file);

            while (xmlRead.Read())
            {
                if (xmlRead.Name == param)
                {
                    if (xmlRead.GetAttribute("type") == "array" && xmlRead.GetAttribute("dataType") == "double")
                    {
                        int length = Convert.ToInt32(xmlRead.GetAttribute("length"));
                        double[] output = new double[length];

                        int i = 0;

                        while (xmlRead.NodeType != XmlNodeType.EndElement)
                        {
                            xmlRead.Read();
                            if (xmlRead.Name == "value")
                            {
                                while (xmlRead.NodeType != XmlNodeType.EndElement)
                                {
                                    xmlRead.Read();
                                    if (xmlRead.NodeType == XmlNodeType.Text)
                                    {
                                        output[i] = Convert.ToDouble(xmlRead.Value);
                                        i++;
                                    } // end if
                                } // end while
                                xmlRead.Read();
                            } // end if
                        } // end while
                        return output;
                    } // end if    
                } // end if
            } // end while
            return null;
        }

        public static short[] getShortArray(string file, string param)
        {
            XmlReader xmlRead = XmlReader.Create(file);

            while (xmlRead.Read())
            {
                if (xmlRead.Name == param)
                {
                    if (xmlRead.AttributeCount == 3 && xmlRead.GetAttribute("type") == "array" && xmlRead.GetAttribute("dataType") == "short")
                    {
                        int length = Convert.ToInt32(xmlRead.GetAttribute("length"));
                        short[] output = new short[length];

                        int i = 0;

                        while (xmlRead.NodeType != XmlNodeType.EndElement)
                        {
                            xmlRead.Read();
                            if (xmlRead.Name == "value")
                            {
                                while (xmlRead.NodeType != XmlNodeType.EndElement)
                                {
                                    xmlRead.Read();
                                    if (xmlRead.NodeType == XmlNodeType.Text)
                                    {
                                        output[i] = Convert.ToInt16(xmlRead.Value);

                                        i++;
                                    } // end if
                                } // end while
                                xmlRead.Read();
                            } // end if
                        } // end while
                        return output;
                    } // end if    
                } // end if
            } // end while
            return null;
        }

        public static double[,] getDoubleMatrix(string file, string param)
        {
            XmlReader xmlRead = XmlReader.Create(file);

            while (xmlRead.Read())
            {
                if (xmlRead.Name == param)
                {
                    if (xmlRead.GetAttribute("type") == "matrix" && xmlRead.GetAttribute("dataType") == "double")
                    {
                        int width = Convert.ToInt16(xmlRead.GetAttribute("width"));
                        int height = Convert.ToInt16(xmlRead.GetAttribute("height"));
                        double[,] output = new double[height, width];

                        int y = 0;
                        int x = 0;

                        while (xmlRead.NodeType != XmlNodeType.EndElement)
                        {
                            xmlRead.Read();
                            if (xmlRead.Name == "value")
                            {
                                while (xmlRead.NodeType != XmlNodeType.EndElement)
                                {
                                    xmlRead.Read();
                                    if (xmlRead.NodeType == XmlNodeType.Text)
                                    {
                                        output[y, x] = Convert.ToDouble(xmlRead.Value);

                                        x++;
                                        if ((x % width) == 0)
                                        {
                                            x = 0;
                                            y++;
                                        }
                                    }
                                }

                                xmlRead.Read();
                            }
                        }
                        return output;
                    } // end if    
                } // end if
            } // end while
            return null;
        }
    }
}
