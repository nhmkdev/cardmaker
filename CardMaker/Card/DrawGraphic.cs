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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;
using Support.Util;

namespace CardMaker.Card
{
    public class DrawGraphic : IDrawGraphic
    {
        private static Bitmap m_zBufferMaskBitmap = null;
        //                                                          1    2  3
        private static readonly Regex regexImageTile = new Regex(@"(.+?)(x)(.+)", RegexOptions.Compiled);

        public void DrawGraphicFile(GraphicsContext zGraphicsContext, string sFile, ProjectLayoutElement zElement, int nXGraphicOffset = 0, int nYGraphicOffset = 0)
        {
            var zGraphics = zGraphicsContext.Graphics;
            var sPath = sFile;
            if (string.IsNullOrEmpty(sPath)
                || sPath.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var zBmp = ImageCache.LoadCustomImageFromCache(sPath, zElement);
            if (zBmp == null)
            {
                IssueManager.Instance.FireAddIssueEvent("Image file not found: " + sPath);
                return;
            }

            if (zElement.imagemasksurface)
            {
                RenderAsMask(zGraphicsContext, zBmp, zElement);
                return;
            }

            if (zElement.centerimageonorigin)
            {
                if (zElement.keeporiginalsize)
                {
                    DrawUncroppedCenteredGraphicOriginalSize(zGraphics, zBmp, zElement, nXGraphicOffset, nYGraphicOffset);
                }
                else
                {
                    DrawUncroppedCenteredGraphicScaledToElementSize(zGraphics, zBmp, zElement, nXGraphicOffset,
                        nYGraphicOffset);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(zElement.tilesize) 
                && zElement.tilesize.Trim() != "-")
            {
                DrawGraphicTiled(zGraphics, zBmp, zElement, sPath, nXGraphicOffset, nYGraphicOffset);
                return;
            }

            if (zElement.keeporiginalsize)
            {
                DrawGraphicOriginalSize(zGraphics, zBmp, zElement);
                return;
            }

            var nWidth = zElement.width;
            var nHeight = zElement.height;

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

        private static Bitmap GetMaskBufferBitmap(int nWidth, int nHeight)
        {
            if (m_zBufferMaskBitmap == null
                || m_zBufferMaskBitmap.Width != nWidth
                || m_zBufferMaskBitmap.Height != nHeight)
            {
                Logger.AddLogLine("creating new buffer mask");
                m_zBufferMaskBitmap?.Dispose();
                m_zBufferMaskBitmap = new Bitmap(nWidth, nHeight);
            }

            return m_zBufferMaskBitmap;
        }

        private static void RenderAsMask(GraphicsContext zGraphicsContext, Bitmap zBitmapMask, ProjectLayoutElement zElement)
        {
            var zBitmapSurface = zGraphicsContext.Bitmap;

            try
            {
                // create a full size mask to match the destination surface (simplifies logic around transforms)
                var zBitmapFullMask =
                    GetMaskBufferBitmap(zGraphicsContext.Bitmap.Width, zGraphicsContext.Bitmap.Height);

                var zMaskGraphics = Graphics.FromImage(zBitmapFullMask);
                zMaskGraphics.Clear(Color.Transparent);

                // mark everything outside the element region (mask) as a white pixel to indicate no masking
                var zRegion = new Region();
                // NOTE: this is a bit strange as one might assume the region is empty to start with...
                // exclude the element space and fill the rest with white (so only the element space is impacted)
                zRegion.Exclude(new Rectangle(0, 0, zElement.width, zElement.height));
                zMaskGraphics.Transform = zGraphicsContext.Graphics.Transform;
                zMaskGraphics.FillRegion(Brushes.White, zRegion);

                // render the mask into the element region
                zMaskGraphics.DrawImage(zBitmapMask, new Rectangle(0,0, zElement.width, zElement.height));

                // lock bits (full images)
                var bitsSurface = zBitmapSurface.LockBits(
                    new Rectangle(0, 0, zBitmapSurface.Width, zBitmapSurface.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                var bitsMask = zBitmapFullMask.LockBits(
                    new Rectangle(0, 0, zBitmapFullMask.Width, zBitmapFullMask.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                var arraySize = zBitmapSurface.Width * zBitmapSurface.Height * 4;
                var arraySurface = new byte[arraySize];
                var arrayMask = new byte[arraySize];
                Marshal.Copy(bitsSurface.Scan0, arraySurface, 0, arraySurface.Length);
                Marshal.Copy(bitsMask.Scan0, arrayMask, 0, arrayMask.Length);
                // cheap optimization -- just get byte offset of the the first and (after) last row
                var nStartIdx = Math.Max(0, (int)((int)zMaskGraphics.Transform.OffsetY * bitsSurface.Stride));
                var nEndIdx = Math.Min(
                    arraySurface.Length, 
                    nStartIdx + bitsSurface.Stride * (int)(zMaskGraphics.Transform.Elements[0] * zElement.height + 1));

                // TODO: optimization (will have to be near approximations with scaling)
                // 1) jump directly to the first pixel of the element region
                // 2) end after the last pixel of the element region
                // 3) jump into each row the at necessary offset (ties into #1 above)

                // iterate over all color channel bytes and multiple them
                for (var nIdx = nStartIdx; nIdx < nEndIdx; nIdx += 4)
                {
                    for (var nComponent = 0; nComponent < 4; nComponent++)
                    {
                        var nArrayIdx = nIdx + nComponent;
                        var fMaskR = arrayMask[nArrayIdx] / 255f;
                        var fSurfaceR = arraySurface[nArrayIdx] / 255f;
                        arraySurface[nArrayIdx] = (byte)((fMaskR * fSurfaceR) * 255);
                    }
                }
                // copy the resulting array back to the surface
                Marshal.Copy(arraySurface, 0, bitsSurface.Scan0, arraySurface.Length);
                zBitmapFullMask.UnlockBits(bitsMask);
                zBitmapSurface.UnlockBits(bitsSurface);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine($"Error processing mask: {ex.ToString()}");
            }
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
        /// Draws the image uncropped and centered on the element origin.
        /// The image is always drawn in proper aspect ratio by this method.
        /// </summary>
        /// <param name="zGraphics"></param>
        /// <param name="zBmp"></param>
        /// <param name="zElement"></param>
        private static void DrawUncroppedCenteredGraphicOriginalSize(Graphics zGraphics, Bitmap zBmp, ProjectLayoutElement zElement, int nXGraphicOffset, int nYGraphicOffset)
        {
            var nCenterX = zBmp.Width / 2;
            var nCenterY = zBmp.Height / 2;
            nXGraphicOffset -= nCenterX;
            nYGraphicOffset -= nCenterY;

            zGraphics.DrawImage(zBmp, new Point(nXGraphicOffset, nYGraphicOffset));
        }

        /// <summary>
        /// Draws the image uncropped and centered on the element origin.
        /// The image is drawn in the proper aspect ratio if the element is set to.
        /// </summary>
        /// <param name="zGraphics"></param>
        /// <param name="zBmp"></param>
        /// <param name="zElement"></param>
        private static void DrawUncroppedCenteredGraphicScaledToElementSize(Graphics zGraphics, Bitmap zBmp, ProjectLayoutElement zElement, int nXGraphicOffset, int nYGraphicOffset)
        {
            var nWidth = zElement.width;
            var nHeight = zElement.height;
            if (zElement.lockaspect)
            {
                GetSizeFromAspectRatio((float)zBmp.Tag, nWidth, nHeight, out nWidth, out nHeight);
            }
            var nCenterX = nWidth / 2;
            var nCenterY = nHeight / 2;
            nXGraphicOffset -= nCenterX;
            nYGraphicOffset -= nCenterY;
            zGraphics.DrawImage(zBmp, new Rectangle(nXGraphicOffset, nYGraphicOffset, nWidth, nHeight));
        }

        private static void DrawGraphicTiled(Graphics zGraphics, Bitmap zBmp, ProjectLayoutElement zElement, string sPath, int nXGraphicOffset, int nYGraphicOffset)
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

                zBmp = ImageCache.LoadCustomImageFromCache(sPath, zElement, nTileWidth, nTileHeight);
            }
            using (var zTextureBrush = new TextureBrush(zBmp, WrapMode.Tile))
            {
                // backup the transform
                var zOriginalTransform = zGraphics.Transform;
                // need to translate so the tiling starts with a full image if offset
                zGraphics.TranslateTransform(nXGraphicOffset, nYGraphicOffset);
                zGraphics.FillRectangle(zTextureBrush, 0, 0, zElement.width, zElement.height);
                zGraphics.Transform = zOriginalTransform;
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
        public static void GetAspectRatioHeight(Bitmap zBmp, int nDesiredWidth, int nDesiredHeight, out int nWidth, out int nHeight)
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
