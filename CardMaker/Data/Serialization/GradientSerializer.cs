////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System.Drawing.Drawing2D;
using System.Drawing;
using System;
using CardMaker.XML;
using Support.Util;
using CardMaker.Card.Render.Gradient;

namespace CardMaker.Data.Serialization
{
    public abstract class GradientSettingsBase
    {
        public Color DestinationColor { get; set; }
    }

    public class GradientSettingsAngle : GradientSettingsBase
    {
        public float Angle { get; set; }
    }

    public class GradientSettingsPoints : GradientSettingsBase
    {
        public PointF Source { get; set; }
        public PointF Destination { get; set; }
    }


    public static class GradientSerializer
    {
        private static readonly string[] PARAM_SEPARATOR = new string[] { CardMakerConstants.GRADIENT_PARAM_SEPARATOR };

        public enum GradientIndex
        {
            Style = 0,
            End,
        }

        public enum GradientStyleIndexesLeftToRight
        {
            Direction = GradientIndex.End,
            DestinationColor,
            End
        }

        public enum GradientStyleIndexesPoints
        {
            DestinationColor = GradientIndex.End,
            StartX,
            StartY,
            DestinationX,
            DestinationY,
            End
        }

        public static GradientStyle GetGradientStyle(string sGradient)
        {
            if (string.IsNullOrWhiteSpace(sGradient))
            {
                return GradientStyle.None;
            }
            var arrayParameters = sGradient.Split(PARAM_SEPARATOR, StringSplitOptions.None);
            if (!Enum.TryParse(arrayParameters[(int)GradientIndex.Style].ToLower(), out GradientStyle eStyle))
            {
                return GradientStyle.None;
            }
            return eStyle;
        }

        public static GradientSettingsAngle GetAngleGradient(string sGradient)
        {
            if (string.IsNullOrWhiteSpace(sGradient))
            {
                return null;
            }
            var arrayParameters = sGradient.Split(PARAM_SEPARATOR, StringSplitOptions.None);
            if (arrayParameters.Length < (int)GradientStyleIndexesLeftToRight.End)
            {
                return null;
            }

            return new GradientSettingsAngle()
            {
                DestinationColor = ProjectLayoutElement.TranslateColorString(
                    arrayParameters[(int)GradientStyleIndexesLeftToRight.DestinationColor]),
                Angle = ParseUtil.ParseDefault(arrayParameters[(int)GradientStyleIndexesLeftToRight.Direction], 0)

            };
        }

        public static GradientSettingsPoints GetPointGradient(string sGradient)
        {
            if (string.IsNullOrWhiteSpace(sGradient))
            {
                return null;
            }
            var arrayParameters = sGradient.Split(PARAM_SEPARATOR, StringSplitOptions.None);
            if (arrayParameters.Length < (int)GradientStyleIndexesPoints.End)
            {
                return null;
            }

            if(false == (
                ParsePoint((int)GradientStyleIndexesPoints.StartX, arrayParameters, out var pointSource) 
                && ParsePoint((int)GradientStyleIndexesPoints.DestinationX, arrayParameters, out var pointDestination)))
            {
                return null;
            }

            return new GradientSettingsPoints()
            {
                DestinationColor = ProjectLayoutElement.TranslateColorString(
                    arrayParameters[(int)GradientStyleIndexesPoints.DestinationColor]),
                Source = pointSource,
                Destination = pointDestination
            };
        }

        public static string SerializeToString(GradientSettingsAngle zGradientSettingsAngle)
        {
            return
                $"lefttoright;{zGradientSettingsAngle.Angle}" +
                $";{ProjectLayoutElement.GetElementColorString(zGradientSettingsAngle.DestinationColor)}";
            }

        public static string SerializeToString(GradientSettingsPoints zGradientSettingsPoints, GradientStyle eGradientStyle)
        {
            return
                $"{eGradientStyle.ToString()};{ProjectLayoutElement.GetElementColorString(zGradientSettingsPoints.DestinationColor)}" +
                $";{zGradientSettingsPoints.Source.X};{zGradientSettingsPoints.Source.Y}" +
                $";{zGradientSettingsPoints.Destination.X};{zGradientSettingsPoints.Destination.Y}";
            }

        public static GradientDefinition ProcessGradientStringToBrush(ProjectLayoutElement zElement)
        {
            var arrayParameters = zElement.gradient.Split(PARAM_SEPARATOR, StringSplitOptions.None);
            GradientStyle eStyle;
            if (!Enum.TryParse(arrayParameters[(int)GradientIndex.Style].ToLower(), out eStyle))
            {
                return null;
            }
            switch (eStyle)
            {
                case GradientStyle.lefttoright:
                    return TranslateLeftToRightGradient(zElement, arrayParameters);
                case GradientStyle.points:
                    return TranslatePointsGradient(zElement, arrayParameters);
                case GradientStyle.pointsnormalized:
                    return TranslatePointsNormalizedGradient(zElement, arrayParameters);
            }

            return null;
        }

        public static GradientDefinition TranslateLeftToRightGradient(ProjectLayoutElement zElement, string[] arrayParameters)
        {
            if (arrayParameters.Length < (int)GradientStyleIndexesLeftToRight.End)
            {
                return null;
            }

            if (!ParseUtil.ParseDouble(arrayParameters[(int)GradientStyleIndexesLeftToRight.Direction],
                    out var dGradientAngle))
            {
                return null;
            }

            var gradientDestinationColor = ProjectLayoutElement.TranslateColorString(
                arrayParameters[(int)GradientStyleIndexesLeftToRight.DestinationColor], 255, out var bColorParsed);
            if (!bColorParsed)
            {
                return null;
            }

            // Convert to radians
            var direction = dGradientAngle * Math.PI / -180.0;
            var max_length = Math.Sqrt(zElement.width * zElement.width + zElement.height * zElement.height);

            // Get the proportional direction and size of the gradient
            var x0 = 0.0;
            var y0 = 0.0;

            var x1 = Math.Cos(direction) * max_length;
            var y1 = Math.Sin(direction) * max_length;

            // Ensure we have the correct angle
            if (y0 > y1) y0 = zElement.height;

            // Define the points so we can reuse them in both cases
            var pointStart = new Point((int)x0, (int)y0);
            var pointEnd = new Point((int)x1, (int)y1);

            return new GradientDefinition()
            {
                Brush = 255 == zElement.opacity
                    ? new LinearGradientBrush(pointStart, pointEnd, zElement.GetElementColor(), gradientDestinationColor)
                    : new LinearGradientBrush(pointStart, pointEnd, Color.FromArgb(zElement.opacity, zElement.GetElementColor()),
                        gradientDestinationColor),
                Start = pointStart,
                End = pointEnd,
            };
        }

        public static GradientDefinition TranslatePointsGradient(ProjectLayoutElement zElement,
            string[] arrayParameters)
        {
            if (arrayParameters.Length < (int)GradientStyleIndexesPoints.End)
            {
                return null;
            }

            var gradientDestinationColor = ProjectLayoutElement.TranslateColorString(
                arrayParameters[(int)GradientStyleIndexesPoints.DestinationColor], 255, out var bColorParsed);
            if (!bColorParsed)
            {
                return null;
            }

            if (!ParsePoint((int)GradientStyleIndexesPoints.StartX, arrayParameters, out var pointStart)
                || !ParsePoint((int)GradientStyleIndexesPoints.DestinationX, arrayParameters, out var pointEnd))
            {
                return null;
            }

            return new GradientDefinition()
            {
                Brush = 255 == zElement.opacity
                    ? new LinearGradientBrush(pointStart, pointEnd, zElement.GetElementColor(), gradientDestinationColor)
                    : new LinearGradientBrush(pointStart, pointEnd, Color.FromArgb(zElement.opacity, zElement.GetElementColor()),
                        gradientDestinationColor),
                Start = pointStart,
                End = pointEnd,
            };
        }

        public static GradientDefinition TranslatePointsNormalizedGradient(ProjectLayoutElement zElement,
            string[] arrayParameters)
        {
            if (arrayParameters.Length < (int)GradientStyleIndexesPoints.End)
            {
                return null;
            }

            var gradientDestinationColor = ProjectLayoutElement.TranslateColorString(
                arrayParameters[(int)GradientStyleIndexesPoints.DestinationColor], 255, out var bColorParsed);
            if (!bColorParsed)
            {
                return null;
            }

            if (!ParsePoint((int)GradientStyleIndexesPoints.StartX, arrayParameters, out var pointStart)
                || !ParsePoint((int)GradientStyleIndexesPoints.DestinationX, arrayParameters, out var pointEnd))
            {
                return null;
            }

            pointStart.X *= zElement.width;
            pointStart.Y *= zElement.height;
            pointEnd.X *= zElement.width;
            pointEnd.Y *= zElement.height;

            return new GradientDefinition()
            {
                Brush = 255 == zElement.opacity
                    ? new LinearGradientBrush(pointStart, pointEnd, zElement.GetElementColor(), gradientDestinationColor)
                    : new LinearGradientBrush(pointStart, pointEnd, Color.FromArgb(zElement.opacity, zElement.GetElementColor()),
                        gradientDestinationColor),
                Start = pointStart,
                End = pointEnd,
            };
        }

        private static bool ParsePoint(int nIdx, string[] arrayParameters, out PointF point)
        {
            point = PointF.Empty;
            if (nIdx + 2 > arrayParameters.Length)
            {
                return false;
            }

            if (ParseUtil.ParseDouble(arrayParameters[nIdx], out var dX)
                && ParseUtil.ParseDouble(arrayParameters[nIdx + 1], out var dY))
            {
                point = new PointF((float)dX, (float)dY);
                return true;
            }
            return false;

        }
    }
}
