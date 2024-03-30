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

using CardMaker.Data;
using CardMaker.XML;
using Support.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CardMaker.Card.Render.Gradient
{
    public static class GradientProcessor
    {
        private static readonly string[] PARAM_SEPARATOR = new string[] { CardMakerConstants.GRADIENT_PARAM_SEPARATOR };

        enum GradientIndex
        {
            Style = 0,
            End,
        }

        enum GradientStyle
        {
            lefttoright,
            points,
            None,
        }

        // TODO: probably break into subprocessor classes and dramatically reduce duplication/logic

        enum GradientStyleIndexesLeftToRight
        {
            Direction = GradientIndex.End,
            DestinationColor,
            End
        }

        enum GradientStyleIndexesPoints
        {
            DestinationColor = GradientIndex.End,
            StartX,
            StartY,
            EndX,
            EndY,
            End
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
            }

            return null;
        }

        public static GradientDefinition TranslateLeftToRightGradient(ProjectLayoutElement zElement, string[] arrayParameters)
        {
            if (arrayParameters.Length < (int)GradientStyleIndexesLeftToRight.End)
            {
                return null;
            }

            double gradientDirection;
            if (!ParseUtil.ParseDouble(arrayParameters[(int)GradientStyleIndexesLeftToRight.Direction],
                    out gradientDirection))
            {
                return null;
            }

            bool bColorParsed;
            var gradientDestinationColor = ProjectLayoutElement.TranslateColorString(
                arrayParameters[(int)GradientStyleIndexesLeftToRight.DestinationColor], 255, out bColorParsed);
            if(!bColorParsed)
            {
                return null;
            }

            // Convert to radians
            var direction = gradientDirection * Math.PI / -180.0;
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
            
            bool bColorParsed;
            var gradientDestinationColor = ProjectLayoutElement.TranslateColorString(
                arrayParameters[(int)GradientStyleIndexesPoints.DestinationColor], 255, out bColorParsed);
            if (!bColorParsed)
            {
                return null;
            }

            Point pointStart;
            Point pointEnd;
            if (!ParsePoint((int)GradientStyleIndexesPoints.StartX, arrayParameters, out pointStart)
                || !ParsePoint((int)GradientStyleIndexesPoints.EndX, arrayParameters, out pointEnd))
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

        private static bool ParsePoint(int nIdx, string[] arrayParameters, out Point point)
        {
            point = Point.Empty;
            if (nIdx + 2 > arrayParameters.Length)
            {
                return false;
            }

            double dX;
            double dY;
            if (ParseUtil.ParseDouble(arrayParameters[nIdx], out dX)
                && ParseUtil.ParseDouble(arrayParameters[nIdx+1], out dY))
            {
                point = new Point((int)dX, (int)dY);
                return true;
            }
            return false;

        }
    }
}
