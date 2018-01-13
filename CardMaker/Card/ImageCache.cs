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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CardMaker.Events.Managers;
using CardMaker.XML;
using PhotoshopFile;

namespace CardMaker.Card
{
#warning make an interface out of this
    public static class ImageCache
    {
        private const int IMAGE_CACHE_MAX = 100;
        private static readonly Dictionary<string, Bitmap> s_dictionaryImages = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Bitmap> s_dictionaryCustomImages = new Dictionary<string, Bitmap>();


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
                    Matrix33 = (float)zElement.opacity / 255.0f
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
                                var zFile = new PsdFile();
                                zFile.Load(sFile);
                                zSourceImage = ImageDecoder.DecodeImage(zFile);
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
    }
}
