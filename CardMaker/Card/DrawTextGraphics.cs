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
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Card
{
    public class DrawTextGraphics : IDrawText
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
            var zFormat = new StringFormat
            {
                LineAlignment = zElement.GetVerticalAlignment(),
                Alignment = zElement.GetHorizontalAlignment()
            };

            var zBrush = 255 == zElement.opacity
                ? new SolidBrush(colorFont)
                : new SolidBrush(Color.FromArgb(zElement.opacity, colorFont));

            if (zElement.autoscalefont)
            {
                SizeF zSize = zGraphics.MeasureString(sInput, zFont, new SizeF(zElement.width, int.MaxValue), zFormat);

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

                    var scaledFont = FontLoader.GetFont(zFont.FontFamily, newSizeRatio * zFont.Size, zFont.Style);
                    //Support.IO.Logger.AddLogLine(scaledFont.Size + " was [" + zFont.Size + "]");
                    zFont = scaledFont;

#if true            // the preprocessing above will get the font size close but not perfect, the slow code below further refines the size
                    // slow mode - but perfect precision (well arguably with the Graphics.MeasureString)
                    bool bUpscaled = false;
                    if (0 < sInput.Trim().Length)
                    {
                        while (true)
                        {
                            zSize = zGraphics.MeasureString(sInput, zFont, new SizeF(zElement.width, int.MaxValue), zFormat);
                            if (zSize.Height > zElement.height || zSize.Width > zElement.width)
                            {
                                if (zFont.Size == 1)
                                    break;
                                zFont = FontLoader.GetFont(zFont.FontFamily, zFont.Size - 1, zFont.Style);
                                if (bUpscaled)
                                    break;
                            }
                            else
                            {
                                zFont = FontLoader.GetFont(zFont.FontFamily, zFont.Size + 1, zFont.Style);
                                bUpscaled = true;
                            }
                        }
                        //Support.IO.Logger.AddLogLine("[" + zFont.Size + "]");
                    }
#endif
                }
                // else -- font size is fine for this element
            }
            else
            {
                zFormat.Trimming = StringTrimming.EllipsisCharacter;
            }

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

            if (0 == zElement.outlinethickness)
            {
                try
                {
                    zGraphics.DrawString(sInput, zFont, zBrush,
                        new RectangleF(0, 0, zElement.width, zElement.height), zFormat);
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
                    zPath.AddString(sInput, zFont.FontFamily, (int)zFont.Style, fEmSize, new RectangleF(0, 0, zElement.width, zElement.height), zFormat);
                    CardRenderer.DrawPathOutline(zElement, zGraphics, zPath);
                }
                catch (Exception)
                {
                    Logger.AddLogLine("Unable to render text (font issue?)");
                }

                // fill in the outline
                zGraphics.FillPath(zBrush, zPath);
            }
        }
    }
}
