////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using System.Drawing;
using System.IO;
using CardMaker.Forms;
using CardMaker.XML;

namespace CardMaker.Card
{
    static public partial class DrawItem
    {
        private static void DrawGraphic(Graphics zGraphics, string sFile, ProjectLayoutElement zElement)
        {
            string sPath = sFile;
            if (sPath.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            if (!File.Exists(sPath))
            {
                sPath = CardMakerMDI.ProjectPath + sFile;
            }
            if (File.Exists(sPath))
            {
                var zBmp = 255 != zElement.opacity
                    ? LoadOpacityImageFromCache(sPath, zElement)
                    : LoadImageFromCache(sPath);
                
                int nWidth = zElement.width;
                int nHeight = zElement.height;

                if (zElement.keeporiginalsize)
                {
                    DrawGraphicOriginalSize(zGraphics, zBmp, zElement);
                    return;
                }

                if (zElement.lockaspect)
                {
                    var fAspect = (float)zBmp.Tag;

                    var nTargetHeight = (int)((float)nWidth / fAspect);
                    if (nTargetHeight < nHeight)
                    {
                        nHeight = (int)((float)nWidth / fAspect);
                    }
                    else
                    {
                        nWidth = (int)((float)nHeight * fAspect);
                    }
                }

                int nX = 0;
                int nY = 0;

                // standard alignment adjustment
                UpdateAlignmentValue(zElement.horizontalalign, ref nX, zElement.width, nWidth);
                UpdateAlignmentValue(zElement.verticalalign, ref nY, zElement.height, nHeight);

                zGraphics.DrawImage(zBmp, nX, nY, nWidth, nHeight);

            }
            else
            {
                MDIIssues.Instance.AddIssue("Image file not found: " + sFile);
            }
            // draw nothing
        }

        /// <summary>
        /// Draws the image cropped based on alignment. The image is always drawn in proper aspect by this method
        /// </summary>
        /// <param name="zGraphics"></param>
        /// <param name="zBmp"></param>
        /// <param name="zElement"></param>
        private static void DrawGraphicOriginalSize(Graphics zGraphics, Bitmap zBmp, ProjectLayoutElement zElement)
        {
            int nSourceX = 0;
            int nSourceY = 0;

            int nX = 0;
            int nY = 0;

            // determine if the update is needed for drawing source X or target X
            if (zBmp.Width > zElement.width)
            {
                UpdateAlignmentValue(zElement.horizontalalign, ref nSourceX, zBmp.Width, zElement.width);
            }
            else
            {
                UpdateAlignmentValue(zElement.horizontalalign, ref nX, zElement.width, zBmp.Width);
            }
            // determine if the update is needed for drawing source Y or target Y
            if (zBmp.Height > zElement.height)
            {
                UpdateAlignmentValue(zElement.verticalalign, ref nSourceY, zBmp.Height, zElement.height);
            }
            else
            {
                UpdateAlignmentValue(zElement.verticalalign, ref nY, zElement.height, zBmp.Height);
            }
            zGraphics.DrawImage(zBmp, nX, nY, new Rectangle(nSourceX, nSourceY, zElement.width, zElement.height), GraphicsUnit.Pixel);
        }

        private static void UpdateAlignmentValue(int nAlignment, ref int nResult, int nLarge, int nSmall)
        {
            switch ((StringAlignment)nAlignment)
            {
                case StringAlignment.Center:
                    nResult = (nLarge - nSmall) >> 1;
                    break;
                case StringAlignment.Far:
                    nResult = nLarge - nSmall;
                    break;
            }            
        }
    }
}
