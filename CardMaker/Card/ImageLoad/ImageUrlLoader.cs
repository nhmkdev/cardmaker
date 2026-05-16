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
using System.Net;
using PhotoshopFile;
#if !MONO_BUILD
using SkiaSharp;
using SkiaSharp.Views.Desktop;
#endif
using Support.IO;
using Support.UI;

namespace CardMaker.Card.ImageLoad
{
    public class ImageUrlLoader : IImageLoader
    {
        public bool UseMemoryCopy => false;
        public bool UseBackgroundThread => true;
        public ImageCacheEntryType CacheEntryType => ImageCacheEntryType.Url;

        public DateTime GetLastWriteTime(string sFile)
        {
            return DateTime.UtcNow;
        }

        public bool ShouldCacheFile(string sFile)
        {
            return true;
        }

        public bool CanAttemptLoad(string sFile)
        {
            sFile = sFile.ToLower();
            return sFile.StartsWith("http://") || sFile.StartsWith("https://");
        }

        public Bitmap LoadBitmap(string sFile)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent: Other");
                try
                {
                    var data = webClient.DownloadData(sFile);
                    using (var stream = new MemoryStream(data))
                    {
                        switch (Path.GetExtension(sFile).ToLower())
                        {
                            case ".psd":
                                {
                                    var zFile = new PsdFile();
                                    zFile.Load(stream);
                                    return ImageDecoder.DecodeImage(zFile);
                                }
#if !MONO_BUILD
                            case ".webp":
                                using (var zCodec = SKCodec.Create(stream))
                                {
                                    return SKBitmap.Decode(zCodec).ToBitmap();
                                }
#endif
                            default:
                                return new Bitmap(stream);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.AddLogLine("Unable to download image: {0} - {1}".FormatString(sFile, ex.ToString()));
                    return null;
                }
            }
        }
    }
}
