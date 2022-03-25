////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Card
{
    /// <summary>
    /// This is an UNUSED renderer
    /// </summary>
    public class DrawTextTextRenderer : IDrawText
    {
        public void DrawText(Graphics zGraphics, ProjectLayoutElement zElement, string sInput)
        {
            var zFont = zElement.GetElementFont();
            var colorFont = zElement.GetElementColor();

            if (null == zFont) // default to something!
            {
                // font will show up in red if it's not yet set
                zFont = FontLoader.DefaultFont;
            }

            var zBrush = 255 == zElement.opacity
                ? new SolidBrush(colorFont)
                : new SolidBrush(Color.FromArgb(zElement.opacity, colorFont));

            TextFormatFlags zFormatFlags =
                TextFormatFlags.WordBreak
                | TextFormatFlags.NoClipping;

            switch (zElement.GetVerticalAlignment())
            {
                case StringAlignment.Center:
                    zFormatFlags |= TextFormatFlags.VerticalCenter;
                    break;
                case StringAlignment.Far:
                    zFormatFlags |= TextFormatFlags.Bottom;
                    break;
            }

            switch (zElement.GetHorizontalAlignment())
            {
                case StringAlignment.Center:
                    zFormatFlags |= TextFormatFlags.HorizontalCenter;
                    break;
                case StringAlignment.Far:
                    zFormatFlags |= TextFormatFlags.Right;
                    break;
            }

//#warning TextRenderer apparently does not support opactiy?!
            Bitmap zOpacityBitmap = null;
            if (255 != zElement.opacity)
            {
                zOpacityBitmap = new Bitmap(zElement.width, zElement.height, PixelFormat.Format32bppArgb);
                zOpacityBitmap.SetResolution(zGraphics.DpiY, zGraphics.DpiY);
            }

            if (zElement.autoscalefont)
            {

                var zSize = TextRenderer.MeasureText(sInput, zFont, new Size(zElement.width, int.MaxValue), zFormatFlags);

                if (zSize.Height > zElement.height || zSize.Width > zElement.width)
                {
                    float newSizeRatio;
                    if ((zSize.Height - zElement.height) > (zSize.Width - zElement.width))
                    {
                        newSizeRatio = (float)zElement.height / (float)zSize.Height;
                    }
                    else
                    {
                        newSizeRatio = (float)zElement.width / (float)zSize.Width;
                    }

                    //var scaledFont = new Font(zFont.FontFamily, newSizeRatio * zFont.Size, zFont.Style);
                    var scaledFont = new Font(zFont.FontFamily, newSizeRatio * zFont.Size, zFont.Style);
                    //Logger.AddLogLine(scaledFont.Size + " was [" + zFont.Size + "]");
                    zFont = scaledFont;

#if true            // the preprocessing above will get the font size close but not perfect, the slow code below further refines the size
                    // slow mode - but perfect precision (well arguably with the Graphics.MeasureString)
                    bool bUpscaled = false;
                    const float FONT_SCALE_ADJUST = 0.25f;
                    if (0 < sInput.Trim().Length)
                    {
                        while (true)
                        {
                            zSize = TextRenderer.MeasureText(sInput, zFont, new Size(zElement.width, int.MaxValue), zFormatFlags);
                            if (zSize.Height > zElement.height || zSize.Width > zElement.width)
                            {
                                if (zFont.Size <= 1)
                                {
                                    break;
                                }
                                zFont = new Font(zFont.FontFamily, zFont.Size - FONT_SCALE_ADJUST, zFont.Style);
                                //Logger.AddLogLine("ADJ A [" + zFont.Size + "]");
                                if (bUpscaled)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                zFont = new Font(zFont.FontFamily, zFont.Size + FONT_SCALE_ADJUST, zFont.Style);
                                //Logger.AddLogLine("ADJ B [" + zFont.Size + "]");
                                bUpscaled = true;
                            }
                        }
                    }
#endif
                }
                // else -- font size is fine for this element
            }
            else
            {
                zFormatFlags |= TextFormatFlags.EndEllipsis;
            }

            Logger.AddLogLine("[" + zFont.Size + "]");

            var arrayDrawLines = new string[] { sInput };
            var nLineOffset = 0;

            var fEmSize = zFont.Size;

            switch (zFont.Unit)
            {
                case GraphicsUnit.Point:
                    fEmSize = zGraphics.DpiY * (zFont.Size / 72f);
                    break;
                default:
                    Logger.AddLogLine("This font is using the Unit: {0} (not currently supported)".FormatString(zFont.Unit.ToString()));
                    break;
            }

            foreach (var sLine in arrayDrawLines)
            {
                if (0 == zElement.outlinethickness)
                {
                    try
                    {
                        // https://stackoverflow.com/questions/849531/textrenderer-drawtext-in-bitmap-vs-onpaintbackground/1578056#1578056
                        if (null != zOpacityBitmap)
                        {
                            zFont = new Font("SkyScrappers Regular", zFont.Size);

                            // TODO: https://stackoverflow.com/questions/18838037/drawing-text-to-a-bitmap-with-textrenderer

#if false
// too bad this makes the font look terrible
                            var image = new Bitmap(zElement.width, zElement.height, PixelFormat.Format32bppArgb);

                            // create memory buffer from desktop handle that supports alpha channel
                            IntPtr dib;
                            var memoryHdc = CreateMemoryHdc(IntPtr.Zero, image.Width, image.Height, out dib);
                            try
                            {
                                // create memory buffer graphics to use for HTML rendering
                                using (var memoryGraphics = Graphics.FromHdc(memoryHdc))
                                {
                                    // must not be transparent background 
                                    memoryGraphics.Clear(Color.White);

                                    TextRenderer.DrawText(memoryGraphics, sLine, zFont, new Rectangle(0, 0, zElement.width, zElement.height), colorFont, zFormatFlags);
                                }

                                // copy from memory buffer to image
                                using (var imageGraphics = Graphics.FromImage(image))
                                {
                                    var imgHdc = imageGraphics.GetHdc();
                                    BitBlt(imgHdc, 0, 0, image.Width, image.Height, memoryHdc, 0, 0, 0x00CC0020);
                                    imageGraphics.ReleaseHdc(imgHdc);
                                }
                            }
                            finally
                            {
                                // release memory buffer
                                DeleteObject(dib);
                                DeleteDC(memoryHdc);
                            }
                            zGraphics.DrawImageUnscaled(image, 0, 0);

#else
#if false
                            using (Bitmap buffer = new Bitmap(zElement.width, zElement.height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                            {
                                using (Graphics graphics = Graphics.FromImage(buffer))
                                {
                                    graphics.FillRectangle(Brushes.Transparent, 0, 0, zElement.width, zElement.height);
                                    // Produces the result below
                                    //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                                    // Produces clean text, but I'd really like ClearType!
                                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                                    TextRenderer.DrawText(graphics, sLine, zFont, new Rectangle(0, 0, zElement.width, zElement.height), colorFont, zFormatFlags);
                                }
                                zGraphics.DrawImageUnscaled(buffer, 0, 0);
                            }
#else
                            var zGraphicsTemp = Graphics.FromImage(zOpacityBitmap);
                            zGraphicsTemp.SmoothingMode = SmoothingMode.AntiAlias;
                            zGraphicsTemp.TextRenderingHint = TextRenderingHint.AntiAlias;

                            TextRenderer.DrawText(zGraphicsTemp, sLine, zFont, new Rectangle(0, 0, zElement.width, zElement.height), colorFont, zFormatFlags);

                            var zColor = new ColorMatrix
                            {
                                Matrix33 = (float)zElement.opacity / 255.0f
                            };
                            var zAttrib = new ImageAttributes();
                            zAttrib.SetColorMatrix(zColor);
                            var zBitmap = new Bitmap(zOpacityBitmap.Width, zOpacityBitmap.Height); // target image
                            var zGraphicsX = Graphics.FromImage(zBitmap);
                            // draw the source image into the destination with the desired opacity
                            zGraphicsX.DrawImage(zOpacityBitmap, new Rectangle(0, 0, zBitmap.Width, zBitmap.Height), 0, 0, zBitmap.Width, zBitmap.Height, GraphicsUnit.Pixel,
                                zAttrib);
                            /**
                             * 
                             *             var zImageAttributes = new ImageAttributes();
            if (255 != zElement.opacity)
            {
                var zColor = new ColorMatrix
                {
                    Matrix33 = (float)zElement.opacity / 255.0f
                };
                zImageAttributes.SetColorMatrix(zColor);
            }

            zDestinationBitmap = new Bitmap(nTargetWidth, nTargetHeight); // target image
            var zGraphics = Graphics.FromImage(zDestinationBitmap);
            // draw the source image into the destination with the desired opacity
            zGraphics.DrawImage(zSourceBitmap, new Rectangle(0, 0, nTargetWidth, nTargetHeight), 0, 0, zSourceBitmap.Width, zSourceBitmap.Height, GraphicsUnit.Pixel,
                zImageAttributes);
                             * */


                            zGraphics.DrawImageUnscaled(zBitmap, 0, 0);
#endif
#endif
                        }
                        else
                        {
                            TextRenderer.DrawText(zGraphics, sLine, zFont, new Rectangle((int)zGraphics.Transform.OffsetX, (int)zGraphics.Transform.OffsetY, zElement.width,
                                    zElement.height), colorFont, zFormatFlags);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.AddLogLine("Unable to render text (font issue?)");
                    }
                }
                else
                {
                    // prepare to draw text
                    var zPath = new GraphicsPath();

                    try
                    {
//#warning is there a path based text renderer thing to use?
                        var zFormat = new StringFormat
                        {
                            LineAlignment = zElement.GetVerticalAlignment(),
                            Alignment = zElement.GetHorizontalAlignment(),
                            Trimming = StringTrimming.None,
                            FormatFlags = StringFormatFlags.NoClip
                        };

                        zPath.AddString(sLine, zFont.FontFamily, (int)zFont.Style, fEmSize, new RectangleF(0, nLineOffset, zElement.width, zElement.height), zFormat);
                        //CardRenderer.DrawElementPath(zElement, zGraphics, zPath);
                    }
                    catch (Exception)
                    {
                        Logger.AddLogLine("Unable to render text (font issue?)");
                    }

                    // fill in the outline
                    zGraphics.FillPath(zBrush, zPath);
                }
                nLineOffset += zElement.lineheight;
            }
        }

        private static IntPtr CreateMemoryHdc(IntPtr hdc, int width, int height, out IntPtr dib)
        {
            // Create a memory DC so we can work off-screen
            IntPtr memoryHdc = CreateCompatibleDC(hdc);
            SetBkMode(memoryHdc, 1);

            // Create a device-independent bitmap and select it into our DC
            var info = new BitMapInfo();
            info.biSize = Marshal.SizeOf(info);
            info.biWidth = width;
            info.biHeight = -height;
            info.biPlanes = 1;
            info.biBitCount = 32;
            info.biCompression = 0; // BI_RGB
            IntPtr ppvBits;
            dib = CreateDIBSection(hdc, ref info, 0, out ppvBits, IntPtr.Zero, 0);
            SelectObject(memoryHdc, dib);

            return memoryHdc;
        }

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int mode);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BitMapInfo pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        [DllImport("gdi32.dll")]
        public static extern int SelectObject(IntPtr hdc, IntPtr hgdiObj);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [StructLayout(LayoutKind.Sequential)]
        internal struct BitMapInfo
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
            public byte bmiColors_rgbBlue;
            public byte bmiColors_rgbGreen;
            public byte bmiColors_rgbRed;
            public byte bmiColors_rgbReserved;
        }

    }
}
