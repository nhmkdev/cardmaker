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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CardMaker.Card.Shapes.Definitions
{
    class GridShape : AbstractShape
    {
        private int m_nAllowPartialGrid = 1;
        private int m_nGridRectWidth = 10;
        private int m_nGridRectHeight = 10;

        private enum GridShapeVariables
        {
            AllowPartialGrid = ShapeInformationIndex.BasicShapeInformation + 1,
            GridRectWidth,
            GridRectHeight,
            VariablesLength = GridRectHeight
        }

        public GridShape()
        {
            m_sName = "grid";
        }

        [Description("Allow Incomplete Grid")]
        public int AllowPartialGrid
        {
            get { return m_nAllowPartialGrid; }
            set { m_nAllowPartialGrid = value; }
        }

        [Description("Grid Rectangle Width")]
        public int GridRectWidth
        {
            get { return m_nGridRectWidth; }
            set { m_nGridRectWidth = value; }
        }

        [Description("Grid Rectangle Height")]
        public int GridRectHeight
        {
            get { return m_nGridRectHeight; }
            set { m_nGridRectHeight = value; }
        }

        public override void InitializeItem(string sInput)
        {
            string[] arraySettings = InitializeVariableArray(sInput);
            if (null != arraySettings && (int)GridShapeVariables.VariablesLength < arraySettings.Length)
            {
                int.TryParse(arraySettings[(int)GridShapeVariables.AllowPartialGrid], out m_nAllowPartialGrid);
                int.TryParse(arraySettings[(int)GridShapeVariables.GridRectWidth], out m_nGridRectWidth);
                int.TryParse(arraySettings[(int)GridShapeVariables.GridRectHeight], out m_nGridRectHeight);
            }
        }

        public override string ToCardMakerString()
        {
            return ToCardMakerString(
                m_nAllowPartialGrid + ";" +
                m_nGridRectWidth + ";" +
                m_nGridRectHeight);
        }

        public override bool DrawShape(GraphicsPath zPath, Rectangle zTargetRect, ShapeInfo zInfo)
        {
            int nTotalWidth = zTargetRect.Width;
            int nTotalHeight = zTargetRect.Height;

            if ((int)GridShapeVariables.VariablesLength > zInfo.Arguments.Length)
                return false;

            int nGridRectWidth;
            int nGridRectHeight;

            var bParse = true;
            var bAllowPartialGrid = zInfo.Arguments[(int)GridShapeVariables.AllowPartialGrid].Equals("1");
            bParse &= int.TryParse(zInfo.Arguments[(int)GridShapeVariables.GridRectWidth], out nGridRectWidth);
            bParse &= int.TryParse(zInfo.Arguments[(int)GridShapeVariables.GridRectHeight], out nGridRectHeight);

            if (!bParse)
                return false;

            if (0 == nGridRectWidth || 0 == nGridRectHeight)
                return false;

            if (!bAllowPartialGrid)
            {
                nTotalWidth = zTargetRect.Width - (zTargetRect.Width % nGridRectWidth);
                nTotalHeight = zTargetRect.Height - (zTargetRect.Height % nGridRectHeight);
            }

            int nVerticalLineOffset = 0;
            int nHorizontalLineOffset = 0;

            // draw the horizontal lines
            while (nHorizontalLineOffset < zTargetRect.Height)
            {
                zPath.AddLine(
                    new Point(zTargetRect.X, nHorizontalLineOffset),
                    new Point(zTargetRect.X + nTotalWidth, nHorizontalLineOffset));
                zPath.CloseFigure();
                nHorizontalLineOffset += nGridRectHeight;
            }
            // draw the vertical lines
            while (nVerticalLineOffset < zTargetRect.Width)
            {
                zPath.AddLine(
                    new Point(nVerticalLineOffset, zTargetRect.Y),
                    new Point(nVerticalLineOffset, zTargetRect.Y + nTotalHeight));
                zPath.CloseFigure();
                nVerticalLineOffset += nGridRectWidth;
            }

            return true;
        }
    }
}
