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
using System.Globalization;

namespace CardMaker.Card.Shapes.Definitions
{
    class TriangleShape : AbstractShape
    {
        private int m_nCorner;

        private enum TriangleVariables
        {
            Corner = ShapeInformationIndex.BasicShapeInformation + 1,
            VariablesLength = Corner,
        }

        [Description("The corner to draw the triangle from")]
        public int Corner
        {
            get { return m_nCorner; }
            set { m_nCorner = value; }
        }

        public TriangleShape()
        {
            m_sName = "triangle";
        }

        public override void InitializeItem(string sInput)
        {
            string[] arraySettings = InitializeVariableArray(sInput);
            if (null != arraySettings && (int)TriangleVariables.VariablesLength < arraySettings.Length)
            {
                int.TryParse(arraySettings[(int)TriangleVariables.Corner], out m_nCorner);
            }
        }

        public override string ToCardMakerString()
        {
            return ToCardMakerString(m_nCorner.ToString(CultureInfo.InvariantCulture));
        }

        public override bool DrawShape(GraphicsPath zPath, Rectangle zTargetRect, ShapeInfo zInfo)
        {
            var arrayPoints = new PointF[3];
            switch (Corner)
            {
                default:
                    arrayPoints[0] = new PointF(zTargetRect.X, zTargetRect.Y);
                    arrayPoints[1] = new PointF(zTargetRect.Right, zTargetRect.Y);
                    arrayPoints[2] = new PointF(zTargetRect.X, zTargetRect.Bottom);
                    break;
                case 1:
                    arrayPoints[0] = new PointF(zTargetRect.X, zTargetRect.Y);
                    arrayPoints[1] = new PointF(zTargetRect.Right, zTargetRect.Y);
                    arrayPoints[2] = new PointF(zTargetRect.Right, zTargetRect.Bottom);
                    break;
                case 2:
                    arrayPoints[0] = new PointF(zTargetRect.Right, zTargetRect.Y);
                    arrayPoints[1] = new PointF(zTargetRect.Right, zTargetRect.Bottom); 
                    arrayPoints[2] = new PointF(zTargetRect.X, zTargetRect.Bottom);
                    break;
                case 3:
                    arrayPoints[0] = new PointF(zTargetRect.Right, zTargetRect.Bottom); 
                    arrayPoints[1] = new PointF(zTargetRect.X, zTargetRect.Bottom);
                    arrayPoints[2] = new PointF(zTargetRect.X, zTargetRect.Y);
                    break;
            }
            zPath.AddPolygon(arrayPoints);
            return true;
        }
    }
}
