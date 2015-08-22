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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using CardMaker.Forms;
using CardMaker.Card.Shapes;
using CardMaker.XML;

namespace CardMaker.Card
{
    static public partial class DrawItem
    {
        private const int IMAGE_CACHE_MAX = 50;

        private static readonly Pen s_zPenDebugBorder = new Pen(Color.FromArgb(196, Color.Red), 1);
        private static readonly Font s_zDefaultFont = new Font("Arial", 12);
        private static readonly Pen m_zPenElementSelect = Pens.ForestGreen;

        private static readonly Dictionary<string, ElementType> s_dictionaryStringElementType = new Dictionary<string, ElementType>();
        private static readonly Dictionary<string, Bitmap> s_dictionaryImages = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Bitmap> s_dictionaryOpacityImages = new Dictionary<string, Bitmap>();

        public const float OutlineFontScale = 395f / 300f;

        static DrawItem()
        {
            for (int nIdx = 0; nIdx < (int)ElementType.End; nIdx++)
            {
                s_dictionaryStringElementType.Add(((ElementType)nIdx).ToString(), (ElementType)nIdx);
            }
        }

        public static Font DefaultFont
        {
            get
            {
                return s_zDefaultFont;
            }
        }

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
            zGraphics.DrawRectangle(s_zPenDebugBorder, zElement.x, zElement.y, zElement.width - 1, zElement.height - 1);
            if (bSelected)
            {
                zGraphics.DrawRectangle(m_zPenElementSelect, zElement.x - 2, zElement.y - 2, zElement.width + 3, zElement.height + 3);
            }
        }

        public static void DrawElement(Graphics zGraphics, Deck zDeck, ProjectLayoutElement zElement, ElementType eType, int nX, int nY, string sInput)
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
            }
            else
            {
                zGraphics.TranslateTransform(zElement.x + nX, zElement.y + nY);
            }
            // TODO: an interface for all these would be more appropriate

            // Draw
            switch (eType)
            {   
                case ElementType.Text:
                    DrawText(zGraphics, zElement, sInput, zBrush, zFont, colorFont);
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

        private static Bitmap LoadOpacityImageFromCache(string sFile, ProjectLayoutElement zElement)
        {
            Bitmap zBitmap;
            string sKey = sFile.ToLower() + ":" + zElement.opacity;
            if (!s_dictionaryOpacityImages.TryGetValue(sKey, out zBitmap))
            {
                // only buffer 20 opacity images max (make this an option?)
                if (s_dictionaryOpacityImages.Count > IMAGE_CACHE_MAX)
                {
                    DumpOpacityImages();
                }
                var zColor = new ColorMatrix
                {
                    Matrix33 = (float) zElement.opacity/255.0f
                };
                var zAttrib = new ImageAttributes();
                zAttrib.SetColorMatrix(zColor);
                var zSourceImage = LoadImageFromCache(sFile);
                zBitmap = new Bitmap(zSourceImage.Width, zSourceImage.Height); // target image
                var zGraphics = Graphics.FromImage(zBitmap);
                // draw the source image into the destination with the desired opacity
                zGraphics.DrawImage(zSourceImage, new Rectangle(0, 0, zBitmap.Width, zBitmap.Height), 0, 0, zBitmap.Width, zBitmap.Height, GraphicsUnit.Pixel,
                    zAttrib);
                zBitmap.Tag = (float)zBitmap.Width / (float)zBitmap.Height; // backup the aspect ratio
                // cache it!
                s_dictionaryOpacityImages.Add(sKey, zBitmap);
            }
            return zBitmap;
        }

        public static Bitmap LoadImageFromCache(string sFile)
        {
            Bitmap zBitmap;
            string sKey = sFile.ToLower();
            if (!s_dictionaryImages.TryGetValue(sKey, out zBitmap))
            {
                // only buffer 20 images max (make this an option?)
                if (s_dictionaryImages.Count > IMAGE_CACHE_MAX)
                {
                    DumpImages();
                }
                if (!File.Exists(sFile))
                {
                    sFile = CardMakerMDI.ProjectPath + sFile;
                    if (!File.Exists(sFile))
                        return null;
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

                zSourceImage.Dispose(); // allow the file to change
                zBitmap.Tag = (float)zBitmap.Width / (float)zBitmap.Height; // backup the aspect ratio
                // cache it!
                s_dictionaryImages.Add(sKey, zBitmap);
            }
            return zBitmap;
        }

        public static void DumpImages()
        {
            DumpImagesFromDictionary(s_dictionaryImages);
        }

        public static void DumpOpacityImages()
        {
            DumpImagesFromDictionary(s_dictionaryOpacityImages);
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
                var outlinePen = new Pen(Color.FromArgb(zElement.opacity, zElement.GetElementOutlineColor()), zElement.outlinethickness);
                zGraphics.DrawPath(outlinePen, zPath);
            }
        }

    }
}
