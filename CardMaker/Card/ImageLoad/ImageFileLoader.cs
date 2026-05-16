////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2026 Tim Stair
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
using PhotoshopFile;
#if !MONO_BUILD
using SkiaSharp;
using SkiaSharp.Views.Desktop;
#endif
using Support.IO;
using Support.UI;

namespace CardMaker.Card.ImageLoad
{
    internal class ImageFileLoader : IImageLoader
    {
        public bool UseMemoryCopy => true;
        public bool UseBackgroundThread => false;
        public ImageCacheEntryType CacheEntryType => ImageCacheEntryType.File;

        public bool ShouldCacheFile(string sFile)
        {
            // do not cache missing files
            return ImagePathUtil.GetExistingFilePath(sFile) != null;
        }

        public DateTime GetLastWriteTime(string sFile)
        {
            var sFilePath = ImagePathUtil.GetExistingFilePath(sFile);
            return sFilePath == null ? DateTime.MinValue : File.GetLastWriteTimeUtc(sFilePath);
        }

        public bool CanAttemptLoad(string sFile)
        {
            return true;
        }

        public Bitmap LoadBitmap(string sFile)
        {
            sFile = ImagePathUtil.GetExistingFilePath(sFile);
            if (sFile == null)
            {
                return null;
            }
            try
            {
                switch (Path.GetExtension(sFile).ToLower())
                {
                    case ".psd":
                    {
                        var zFile = new PsdFile();
                        zFile.Load(sFile);
                        return ImageDecoder.DecodeImage(zFile);
                    }
#if !MONO_BUILD
                    case ".webp":
                        using (var zStream = SKFileStream.OpenStream(sFile))
                        {
                            return SKBitmap.Decode(zStream).ToBitmap();
                        }
#endif
                    default:
                        return new Bitmap(sFile);
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Unable to load image: {0} - {1}".FormatString(sFile, ex.ToString()));
                return null;
            }
        }
    }
}
