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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card
{
    public class DrawGraphic : IDrawGraphic
    {
        //                                                          1    2  3
        private static readonly Regex regexImageTile = new Regex(@"(.+?)(x)(.+)", RegexOptions.Compiled);

        private const string APPLICATION_FOLDER_MARKER = "{appfolder}";

        public void DrawGraphicFile(Graphics zGraphics, string sFile, ProjectLayoutElement zElement, int nXGraphicOffset = 0, int nYGraphicOffset = 0)
        {
            var sPath = sFile;
            if (string.IsNullOrEmpty(sFile)
                || sPath.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            if (sPath.StartsWith(APPLICATION_FOLDER_MARKER))
            {
                sPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    sPath.Replace(APPLICATION_FOLDER_MARKER, string.Empty));
            }
            if (!File.Exists(sPath))
            {
                sPath = ProjectManager.Instance.ProjectPath + sFile;
            }
            if (!File.Exists(sPath))
            {
                IssueManager.Instance.FireAddIssueEvent("Image file not found: " + sFile);
                return;
            }

            var zBmp = 255 != zElement.opacity
                ? ImageCache.LoadCustomImageFromCache(sPath, zElement)
                : ImageCache.LoadImageFromCache(sPath);
                
            var nWidth = zElement.width;
            var nHeight = zElement.height;

            // TODO: sub processor methods (at a minimum)

            if (!string.IsNullOrWhiteSpace(zElement.tilesize) 
                && zElement.tilesize.Trim() != "-")
            {
                var zMatch = regexImageTile.Match(zElement.tilesize);
                if (zMatch.Success)
                {
                    var nTileWidth = Math.Max(-1, ParseUtil.ParseDefault(zMatch.Groups[1].Value, -1));
                    var nTileHeight = Math.Max(-1, ParseUtil.ParseDefault(zMatch.Groups[3].Value, -1));
                    GetAspectRatioHeight(zBmp, nTileWidth, nTileHeight, out nTileWidth, out nTileHeight);
                    // paranoia...
                    nTileWidth = Math.Max(1, nTileWidth);
                    nTileHeight = Math.Max(1, nTileHeight);

                    zBmp = ImageCache.LoadCustomImageFromCache(sFile, zElement, nTileWidth, nTileHeight);
                }
                using (var zTextureBrush = new TextureBrush(zBmp, WrapMode.Tile))
                {
                    // backup the transform
                    var zOriginalTransform = zGraphics.Transform;
                    // need to translate so the tiling starts with a full image if offset
                    zGraphics.TranslateTransform(nXGraphicOffset, nYGraphicOffset);
                    zGraphics.FillRectangle(zTextureBrush, 0, 0, nWidth, nHeight);
                    zGraphics.Transform = zOriginalTransform;
                }
                return;
            }

            if (zElement.keeporiginalsize)
            {
                DrawGraphicOriginalSize(zGraphics, zBmp, zElement);
                return;
            }

            if (zElement.lockaspect)
            {
                GetSizeFromAspectRatio((float) zBmp.Tag, nWidth, nHeight, out nWidth, out nHeight);
            }

            var nX = 0;
            var nY = 0;

            // standard alignment adjustment
            UpdateAlignmentValue(zElement.GetHorizontalAlignment(), ref nX, zElement.width, nWidth);
            UpdateAlignmentValue(zElement.GetVerticalAlignment(), ref nY, zElement.height, nHeight);
            zGraphics.DrawImage(zBmp, nX + nXGraphicOffset, nY + nYGraphicOffset, nWidth, nHeight);
        }

        private static void GetSizeFromAspectRatio(float fAspect, int nWidth, int nHeight, out int nDestWidth, out int nDestHeight)
        {
            var nTargetHeight = (int)((float)nWidth / fAspect);
            if (nTargetHeight < nHeight)
            {
                nDestWidth = nWidth;
                nDestHeight = (int)((float)nWidth / fAspect);
            }
            else
            {
                nDestWidth = (int)((float)nHeight * fAspect);
                nDestHeight = nHeight;
            }
        }

        /// <summary>
        /// Draws the image cropped based on alignment. The image is always drawn in proper aspect ratio by this method.
        /// </summary>
        /// <param name="zGraphics"></param>
        /// <param name="zBmp"></param>
        /// <param name="zElement"></param>
        private static void DrawGraphicOriginalSize(Graphics zGraphics, Bitmap zBmp, ProjectLayoutElement zElement)
        {
            var nSourceX = 0;
            var nSourceY = 0;

            var nX = 0;
            var nY = 0;

            // determine if the update is needed for drawing source X or target X
            if (zBmp.Width > zElement.width)
            {
                UpdateAlignmentValue(zElement.GetHorizontalAlignment(), ref nSourceX, zBmp.Width, zElement.width);
            }
            else
            {
                UpdateAlignmentValue(zElement.GetHorizontalAlignment(), ref nX, zElement.width, zBmp.Width);
            }
            // determine if the update is needed for drawing source Y or target Y
            if (zBmp.Height > zElement.height)
            {
                UpdateAlignmentValue(zElement.GetVerticalAlignment(), ref nSourceY, zBmp.Height, zElement.height);
            }
            else
            {
                UpdateAlignmentValue(zElement.GetVerticalAlignment(), ref nY, zElement.height, zBmp.Height);
            }
            zGraphics.DrawImage(zBmp, nX, nY, new Rectangle(nSourceX, nSourceY, zElement.width, zElement.height), GraphicsUnit.Pixel);
        }

        private static void UpdateAlignmentValue(StringAlignment eAlignment, ref int nResult, int nLarge, int nSmall)
        {
            switch (eAlignment)
            {
                case StringAlignment.Center:
                    nResult = (nLarge - nSmall) >> 1;
                    break;
                case StringAlignment.Far:
                    nResult = nLarge - nSmall;
                    break;
            }            
        }

#warning needs unit tests
        private static void GetAspectRatioHeight(Bitmap zBmp, int nDesiredWidth, int nDesiredHeight, out int nWidth, out int nHeight)
        {
            if (0 >= nDesiredWidth
                && 0 >= nDesiredHeight)
            {
                nWidth = zBmp.Width;
                nHeight = zBmp.Height;
            }
            else if (0 >= nDesiredWidth)
            {
                nWidth = (int)((float)nDesiredHeight * (float)zBmp.Tag);
                nHeight = nDesiredHeight;
            }
            else if (0 >= nDesiredHeight)
            {
                nWidth = nDesiredWidth;
                nHeight = (int) ((float)nDesiredWidth / (float) zBmp.Tag);
            }
            else
            {
                nWidth = nDesiredWidth;
                nHeight = nDesiredHeight;
            }
        }
    }
}
