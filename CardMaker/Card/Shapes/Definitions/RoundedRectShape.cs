////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace CardMaker.Card.Shapes.Definitions
{
    class RoundedRectShape : AbstractShape
    {
        private int m_nRoundedCornerSize;

        private enum RoundedRectVariables
        {
            CornerSize = ShapeInformationIndex.BasicShapeInformation + 1,
            VariablesLength = CornerSize,
        }

        [Description("Pixels from corner to round out.")]
        public int EdgeSize
        {
            get { return m_nRoundedCornerSize; }
            set { m_nRoundedCornerSize = value; }
        }

        public RoundedRectShape()
        {
            m_sName = "roundedrect";
        }

        public override void InitializeItem(string sInput)
        {
            string[] arraySettings = InitializeVariableArray(sInput);
            if (null != arraySettings && (int)RoundedRectVariables.VariablesLength < arraySettings.Length)
            {
                int.TryParse(arraySettings[(int)RoundedRectVariables.CornerSize], out m_nRoundedCornerSize);
            }
        }

        public override string ToCardMakerString()
        {
            return ToCardMakerString(m_nRoundedCornerSize.ToString(CultureInfo.InvariantCulture));
        }

        public override bool DrawShape(GraphicsPath zPath, Rectangle zTargetRect, ShapeInfo zInfo)
        {
            int nWidth = zTargetRect.Width;
            int nHeight = zTargetRect.Height;
            int nDesiredCornerSize;

            if ((int)RoundedRectVariables.VariablesLength > zInfo.Arguments.Length)
                return false;

            if (!int.TryParse(zInfo.Arguments[(int)RoundedRectVariables.CornerSize], out nDesiredCornerSize))
                return false;

            int nSmallerSide = Math.Min(nWidth, nHeight);

            if (nDesiredCornerSize > (nSmallerSide >> 1))
            {
                nDesiredCornerSize = nSmallerSide >> 1;
            }

            if (0 >= nDesiredCornerSize)
            {
                return false; // get out!
            }

            int nArcSize = nDesiredCornerSize * 2;

            int nXSlide = nWidth - nArcSize;
            int nYSlide = nHeight - nArcSize;

            int nWidthLine = Math.Max(0, nWidth - (nDesiredCornerSize * 2));
            int nHeightLine = Math.Max(0, nHeight - (nDesiredCornerSize * 2));

            zPath.AddLine(nDesiredCornerSize, 0, nDesiredCornerSize + nWidthLine, 0);
            zPath.AddArc(nXSlide, 0, nArcSize, nArcSize, 270, 90);

            zPath.AddLine(nWidth, nDesiredCornerSize, nWidth, nHeightLine + nDesiredCornerSize);
            zPath.AddArc(nXSlide, nYSlide, nArcSize, nArcSize, 0, 90);

            zPath.AddLine(nDesiredCornerSize + nWidthLine, nHeight, nDesiredCornerSize, nHeight);
            zPath.AddArc(0, nYSlide, nArcSize, nArcSize, 90, 90);

            zPath.AddLine(0, nDesiredCornerSize + nHeightLine, 0, nDesiredCornerSize);
            zPath.AddArc(0, 0, nArcSize, nArcSize, 180, 90);
            zPath.CloseFigure();
            return true;
        }        
    }
}
