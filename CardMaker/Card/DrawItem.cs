////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2017 Tim Stair
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using CardMaker.Card.Shapes;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;

namespace CardMaker.Card
{
    public static partial class DrawItem
    {
        private const int IMAGE_CACHE_MAX = 100;

        private static readonly Pen s_zPenDebugBorder = new Pen(Color.FromArgb(196, Color.Red), 1);
        private static readonly Pen s_zPenDebugGuides = new Pen(Color.FromArgb(196, Color.LightPink), 1);
        private static readonly Font s_zDefaultFont = FontLoader.DefaultFont;
        private static readonly Pen m_zPenElementSelect = Pens.ForestGreen;

        private static readonly Dictionary<string, ElementType> s_dictionaryStringElementType = new Dictionary<string, ElementType>();
        private static readonly Dictionary<string, Bitmap> s_dictionaryImages = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Bitmap> s_dictionaryCustomImages = new Dictionary<string, Bitmap>();

        public const float OutlineFontScale = 395f / 300f;

        private static readonly IDrawText s_zDrawText =
#if false
            new DrawTextRenderer();
#else
            new DrawTextGraphics();
#endif

        static DrawItem()
        {
            for (int nIdx = 0; nIdx < (int)ElementType.End; nIdx++)
            {
                s_dictionaryStringElementType.Add(((ElementType)nIdx).ToString(), (ElementType)nIdx);
            }
        }

        public static Font DefaultFont => s_zDefaultFont;

        public static ElementType GetElementType(string sType)
        {
            ElementType eType;
            if (s_dictionaryStringElementType.TryGetValue(sType, out eType))
            {
                return eType;
            }
            return ElementType.End;
        }

        public static void DrawElementDebugBorder(Graphics zGraphics, ProjectLayoutElement zElement, int nX, int nY, bool bSelected)
        {
            // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
            zGraphics.TranslateTransform(nX, nY);
            if (bSelected && CardMakerInstance.DrawSelectedElementGuides)
            {
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(0, zElement.y), new PointF(zGraphics.ClipBounds.Width, zElement.y));
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(0, zElement.y + zElement.height - 1),
                    new PointF(zGraphics.ClipBounds.Width, zElement.y + zElement.height));
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(zElement.x, 0), new PointF(zElement.x, zGraphics.ClipBounds.Height));
                zGraphics.DrawLine(s_zPenDebugGuides, new PointF(zElement.x + zElement.width - 1, 0),
                    new PointF(zElement.x + zElement.width, zGraphics.ClipBounds.Height));
            }
            zGraphics.DrawRectangle(s_zPenDebugBorder, zElement.x, zElement.y, zElement.width - 1, zElement.height - 1);
            if (bSelected)
            {
                zGraphics.DrawRectangle(m_zPenElementSelect, zElement.x - 2, zElement.y - 2, zElement.width + 3, zElement.height + 3);
            }
        }

        public static void DrawElement(Graphics zGraphics, Deck zDeck, ProjectLayoutElement zElement, ElementType eType, int nX, int nY, string sInput, bool bExport)
        {
            switch (eType)
            {
                case ElementType.Graphic:
                case ElementType.Shape:
                    sInput = sInput.Trim();
                    break;
            }

            Font zFont = null;
            Brush zBrush = null;
            Pen zBorderPen = null;

            Color colorFont = Color.Black;

            if (0 != zElement.borderthickness)
            {
                zBorderPen = 255 != zElement.opacity
                    ? new Pen(Color.FromArgb(zElement.opacity, zElement.GetElementBorderColor()), zElement.borderthickness)
                    : new Pen(zElement.GetElementBorderColor(), zElement.borderthickness);
            }

            // Setup
            switch (eType)
            {
                case ElementType.Text:
                case ElementType.FormattedText:
                    zFont = zElement.GetElementFont();
                    colorFont = zElement.GetElementColor();
                    zBrush = new SolidBrush(colorFont);
                    break;
                case ElementType.Graphic:
                case ElementType.Shape:
                    break;
                default:
                    return;
            }

            // NOTE: this is the first transform
            if (0 != zElement.rotation)
            {
                // center the internal element then rotate and restore
                zGraphics.TranslateTransform(zElement.x + nX + (zElement.width >> 1), zElement.y + nY + (zElement.height >> 1));
                zGraphics.RotateTransform(zElement.rotation);
                zGraphics.TranslateTransform(-(zElement.width >> 1), -(zElement.height >> 1));
                if (CardMakerInstance.DrawElementBorder && CardMakerInstance.DrawSelectedElementRotationBounds && !bExport)
                {
                    zGraphics.DrawRectangle(Pens.LightGreen, 0, 0, zElement.width - 1, zElement.height - 1);
                }
            }
            else
            {
                zGraphics.TranslateTransform(zElement.x + nX, zElement.y + nY);
            }

            // render the background color
            if (zElement.GetElementBackgroundColor() != CardMakerConstants.NoColor)
            {
                var zBackgroundBrush = 255 != zElement.opacity
                    ? new SolidBrush(Color.FromArgb(zElement.opacity, zElement.GetElementBackgroundColor()))
                    : new SolidBrush(zElement.GetElementBackgroundColor());
                zGraphics.FillRectangle(zBackgroundBrush, 0, 0,
                    zElement.width, zElement.height);
            }

            // TODO: this should just be a sequence of processors 1) transform 2) background 3) background shape 4) render 5) blah
            // render any inline shape (max of 1)
            sInput = ShapeManager.ProcessInlineShape(zGraphics, zElement, sInput);

            // render any inline background image (max of 1)
            sInput = ProcessInlineBackgroundImage(zGraphics, zElement, sInput);


            // TODO: an interface for all these would be more appropriate
            // Draw
            switch (eType)
            {   
                case ElementType.Text:
                    s_zDrawText.DrawText(zGraphics, zElement, sInput, zBrush, zFont, colorFont);
                    break;
                case ElementType.FormattedText:
                    DrawFormattedText(zGraphics, zDeck, zElement, sInput, zBrush, zFont, colorFont);
                    break;
                case ElementType.Graphic:
                    DrawGraphic(zGraphics, sInput, zElement);
                    break;
                case ElementType.Shape:
                    ShapeManager.HandleShapeRender(zGraphics, sInput.ToLower(), zElement);
                    break;
            }

            if (null != zBorderPen)
            {
                // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
                zGraphics.DrawRectangle(zBorderPen, 0,0,zElement.width - 1, zElement.height - 1);
            }

            zGraphics.ResetTransform();
        }

#warning the callers of this should not be performing the opacity 255 check, that should be ina shared method
        public static Bitmap LoadCustomImageFromCache(string sFile, ProjectLayoutElement zElement, int nTargetWidth = -1, int nTargetHeight = -1)
        {
            Bitmap zDestinationBitmap;
            var sKey = sFile.ToLower() + ":" + zElement.opacity + ":" + nTargetWidth + ":" + nTargetHeight;
#warning the name of this cache is incorrect because it will include "customized" images (opacity, scale, whatever)
            if (s_dictionaryCustomImages.TryGetValue(sKey, out zDestinationBitmap))
            {
                return zDestinationBitmap;
            }

            var zSourceBitmap = LoadImageFromCache(sFile);
            // if the desired width/height/opcaity match the 'plain' cached copy just return it
            if (zSourceBitmap.Width == nTargetWidth
                && zSourceBitmap.Height == nTargetHeight
                && 255 == zElement.opacity)
            {
                return zSourceBitmap;
            }

            // TODO: should this be handled in a shared way?
            if (s_dictionaryCustomImages.Count > IMAGE_CACHE_MAX)
            {
                DumpImagesFromDictionary(s_dictionaryCustomImages);
            }

            var zAttrib = new ImageAttributes();
            if (255 != zElement.opacity)
            {
                var zColor = new ColorMatrix
                {
                    Matrix33 = (float) zElement.opacity / 255.0f
                };
                zAttrib.SetColorMatrix(zColor);
            }

            nTargetWidth = nTargetWidth == -1 ? zSourceBitmap.Width : nTargetWidth;
            nTargetHeight = nTargetHeight == -1 ? zSourceBitmap.Height : nTargetHeight;

            zDestinationBitmap = new Bitmap(nTargetWidth, nTargetHeight); // target image
            var zGraphics = Graphics.FromImage(zDestinationBitmap);
            // draw the source image into the destination with the desired opacity
            zGraphics.DrawImage(zSourceBitmap, new Rectangle(0, 0, nTargetWidth, nTargetHeight), 0, 0, zSourceBitmap.Width, zSourceBitmap.Height, GraphicsUnit.Pixel,
                zAttrib);
            CacheImage(s_dictionaryCustomImages, sKey, zDestinationBitmap);

            return zDestinationBitmap;
        }

        public static Bitmap LoadImageFromCache(string sFile)
        {
            Bitmap zBitmap;
            var sKey = sFile.ToLower();
            if (!s_dictionaryImages.TryGetValue(sKey, out zBitmap))
            {
                if (s_dictionaryImages.Count > IMAGE_CACHE_MAX)
                {
                    DumpImagesFromDictionary(s_dictionaryImages);
                }
                if (!File.Exists(sFile))
                {
                    sFile = ProjectManager.Instance.ProjectPath + sFile;
                    if (!File.Exists(sFile))
                    {
                        return null;
                    }
                }

                Bitmap zSourceImage;
                try
                {
                    switch (Path.GetExtension(sFile).ToLower())
                    {
                        case ".psd":
                            {
                                var zFile = new PhotoshopFile.PsdFile();
                                zFile.Load(sFile);
                                zSourceImage = PhotoshopFile.ImageDecoder.DecodeImage(zFile);
                            }
                            break;
                        default:
                            zSourceImage = new Bitmap(sFile);
                            break;
                    }
                }
                catch (Exception)
                {
                    // return a purple bitmap to indicate an error
                    zBitmap = new Bitmap(1, 1);
                    Graphics.FromImage(zBitmap).FillRectangle(Brushes.Purple, 0, 0, zBitmap.Width, zBitmap.Height);
                    return zBitmap;
                }
                 
                zBitmap = new Bitmap(zSourceImage.Width, zSourceImage.Height);
                
                // copy the contents into the image
                Graphics zGraphics = Graphics.FromImage(zBitmap);
                zGraphics.DrawImage(zSourceImage, new Rectangle(0, 0, zBitmap.Width, zBitmap.Height), 0, 0, zBitmap.Width, zBitmap.Height, GraphicsUnit.Pixel);

                // duping the image into a memory copy allows the file to change (not locked by the application)
                zSourceImage.Dispose();
                CacheImage(s_dictionaryImages, sKey, zBitmap);
            }
            return zBitmap;
        }

        public static void ClearImageCaches()
        {
            DumpImagesFromDictionary(s_dictionaryImages);
            DumpImagesFromDictionary(s_dictionaryCustomImages);
        }

        private static void CacheImage(IDictionary<string, Bitmap> dictionaryImageCache, string sKey, Bitmap zBitmap)
        {
            // preserve the aspect ratio on the tag
            zBitmap.Tag = (float)zBitmap.Width / (float)zBitmap.Height;
            dictionaryImageCache.Add(sKey, zBitmap);
        }

        private static void DumpImagesFromDictionary(Dictionary<string, Bitmap> dictionaryImages)
        {
            foreach (Bitmap zBitmap in dictionaryImages.Values)
            {
                zBitmap.Dispose();
            }
            dictionaryImages.Clear();
        }

        public static void DrawOutline(ProjectLayoutElement zElement, Graphics zGraphics, GraphicsPath zPath)
        {
            // draw the outline
            if (0 < zElement.outlinethickness)
            {
                var outlinePen = new Pen(Color.FromArgb(zElement.opacity, zElement.GetElementOutlineColor()),
                    zElement.outlinethickness)
                {
                    LineJoin = LineJoin.Round
                };
#warning This outline pen linejoin should be customizable (as it rounds the corners but corrects other issues!)
                zGraphics.DrawPath(outlinePen, zPath);
            }
        }

    }
}
