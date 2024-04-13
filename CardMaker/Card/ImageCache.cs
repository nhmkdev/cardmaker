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

//#define LOG_CACHE_MISSES

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using PhotoshopFile;
#if !MONO_BUILD
using SkiaSharp;
using SkiaSharp.Views.Desktop;
#endif
using Support.IO;
using Support.UI;

namespace CardMaker.Card
{
    public static class ImageCache
    {
        public enum ImageCacheEntryType
        {
            File,
            InMemory,
            Missing,
        }

        // NOTE: this entire cache is not thread safe (not critical at this point)

        // If this gets any more complex split it into implementations (in memory, file)
        private class BitmapCacheEntry
        {
            public DateTime LastWriteTimestamp { get; }
            public Bitmap Bitmap { get; }
            public ImageCacheEntryType EntryType { get; }
            public HashSet<string> DependentCustomCacheKeys { get; } = new HashSet<string>();

            public BitmapCacheEntry(Bitmap zBitmap, DateTime dtLastWriteTimestamp, ImageCacheEntryType eEntryType)
            {
                LastWriteTimestamp = dtLastWriteTimestamp;
                Bitmap = zBitmap;
                EntryType = eEntryType;
            }

            public void AddDependentCustomCacheKey(string sKey)
            {
                DependentCustomCacheKeys.Add(sKey);
            }
        }

        private const int IMAGE_CACHE_MAX = 500;
        // cache of plain images (no adjustments)
        private static readonly Dictionary<string, BitmapCacheEntry> s_dictionaryImages = new Dictionary<string, BitmapCacheEntry>();
        // cache of images with in-memory tweaks
        private static readonly Dictionary<string, BitmapCacheEntry> s_dictionaryCustomImages = new Dictionary<string, BitmapCacheEntry>();

        public static void ClearImageCaches()
        {
            DumpImagesFromDictionary(s_dictionaryImages);
            DumpImagesFromDictionary(s_dictionaryCustomImages);
        }

        public static Bitmap LoadCustomImageFromCache(string sFile, ProjectLayoutElement zElement,
            int nTargetWidth = -1, int nTargetHeight = -1)
        {
            return LoadCustomImageFromCache(sFile, zElement, zElement.GetElementColor(), nTargetWidth, nTargetHeight, zElement.GetMirrorType());
        }

        public static Bitmap LoadCustomImageFromCache(string sFile, ProjectLayoutElement zElement, Color colorOverride, int nTargetWidth = -1, int nTargetHeight = -1, MirrorType eMirrorType = MirrorType.None)
        {
            var sKey = sFile.ToLower() + ":" + zElement.opacity + ":" + nTargetWidth + ":" + nTargetHeight + ProjectLayoutElement.GetElementColorString(colorOverride) + 
                       ":" + eMirrorType;

            if (GetCacheEntry(s_dictionaryCustomImages, sKey, sFile, out var zCacheEntry))
            {
                return zCacheEntry.Bitmap;
            }

            var zElementType = EnumUtil.GetElementType(zElement.type);

            var zBaseImageCacheEntry = LoadImageFromCache(sFile);
            if (null == zBaseImageCacheEntry)
            {
                return null;
            }

            var zSourceBitmap = zBaseImageCacheEntry.Bitmap;
            // if the desired width/height/opacity match the 'plain' cached copy just return it (or special color handling for certain element types)
            // TODO: make a method for this just to shrink all this logic down
            if (
                (
                    (-1 == nTargetWidth || zSourceBitmap.Width == nTargetWidth)
                    && (-1 == nTargetHeight || zSourceBitmap.Height == nTargetHeight)
                    && 255 == zElement.opacity
                )
            )
            {
                switch (zElementType)
                {
                    case ElementType.FormattedText:
                    case ElementType.Graphic:
                        if (colorOverride == Color.Black)
                        {
                            return zSourceBitmap;
                        }
                        break;
                    default:
                        return zSourceBitmap;
                }
                
            }
            // TODO: should this be handled in a shared way?
            // TODO: this is a terrible eviction strategy
            if (s_dictionaryCustomImages.Count > IMAGE_CACHE_MAX)
            {
                DumpImagesFromDictionary(s_dictionaryCustomImages);
            }

            var zImageAttributes = new ImageAttributes();
            var zColor = new ColorMatrix();
            if (255 != zElement.opacity)
            {
                zColor.Matrix33 = (float) zElement.opacity / 255.0f;
            }
            // special color handling for certain element types
            if (colorOverride != Color.Black)
            {
                switch (zElementType)
                {
                    case ElementType.FormattedText:
                    case ElementType.Graphic:
                        zColor.Matrix40 = (float)colorOverride.R / 255.0f;
                        zColor.Matrix41 = (float)colorOverride.G / 255.0f;
                        zColor.Matrix42 = (float)colorOverride.B / 255.0f;
                        break;
                }
            }
            zImageAttributes.SetColorMatrix(zColor);

            nTargetWidth = nTargetWidth == -1 ? zSourceBitmap.Width : nTargetWidth;
            nTargetHeight = nTargetHeight == -1 ? zSourceBitmap.Height : nTargetHeight;

            var zDestinationBitmap = new Bitmap(nTargetWidth, nTargetHeight); // target image
            var zGraphics = Graphics.FromImage(zDestinationBitmap);

            MirrorRender.MirrorElementGraphicTransform(zGraphics, zElement, eMirrorType, nTargetWidth, nTargetHeight);

            // draw the source image into the destination with the desired opacity
            zGraphics.DrawImage(zSourceBitmap, new Rectangle(0, 0, nTargetWidth, nTargetHeight), 0, 0, zSourceBitmap.Width, zSourceBitmap.Height, GraphicsUnit.Pixel,
                zImageAttributes);
            CacheImage(s_dictionaryCustomImages, sKey, sFile, zDestinationBitmap, zBaseImageCacheEntry.EntryType);
            zBaseImageCacheEntry.AddDependentCustomCacheKey(sKey);
            return zDestinationBitmap;
        }

        private static BitmapCacheEntry LoadImageFromCache(string sFile)
        {
            var sKey = sFile.ToLower();
            if (GetCacheEntry(s_dictionaryImages, sKey, sFile, out var zCacheEntry))
            {
                return zCacheEntry;
            }
            
            if (s_dictionaryImages.Count > IMAGE_CACHE_MAX)
            {
                // TODO: this is a terrible eviction strategy
                DumpImagesFromDictionary(s_dictionaryImages);
            }

            sFile = GetExistingFilePath(sFile);
            if(sFile == null)
            {
                return null;
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
#if !MONO_BUILD
                    case ".webp":
                        using (var zStream = SKFileStream.OpenStream(sFile))
                        {
                            zSourceImage = SKBitmap.Decode(zStream).ToBitmap();
                        }
                        break;
#endif
                    default:
                        zSourceImage = new Bitmap(sFile);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Unable to load image: {0} - {1}".FormatString(sFile, ex.ToString()));
                return null;
            }

            var zDestinationBitmap = new Bitmap(zSourceImage.Width, zSourceImage.Height);

            // copy the contents into the image
            var zGraphics = Graphics.FromImage(zDestinationBitmap);
            zGraphics.DrawImage(zSourceImage, new Rectangle(0, 0, zDestinationBitmap.Width, zDestinationBitmap.Height), 
                0, 0, zDestinationBitmap.Width, zDestinationBitmap.Height, GraphicsUnit.Pixel);

            // duping the image into a memory copy allows the file to change (not locked by the application)
            zSourceImage.Dispose();
            return CacheImage(s_dictionaryImages, sKey, sFile, zDestinationBitmap, ImageCacheEntryType.File);
        }

        private static bool GetCacheEntry(IDictionary<string, BitmapCacheEntry> dictionaryImageCache, string sKey, string sFile, out BitmapCacheEntry zCacheEntry)
        {
            zCacheEntry = null;
            if (dictionaryImageCache.TryGetValue(sKey, out zCacheEntry))
            {
                if (zCacheEntry.EntryType == ImageCacheEntryType.InMemory)
                {
                    return true;
                }

                try
                {
                    sFile = GetExistingFilePath(sFile);
                    if (File.GetLastWriteTimeUtc(sFile) == zCacheEntry.LastWriteTimestamp)
                    {
                        return true;
                    }
#if LOG_CACHE_MISSES
                    Logger.AddLogLine($"Image Cache Miss[timestamp]: {sFile}");
#endif
                    RemoveCacheEntry(dictionaryImageCache, sKey);
                    return false;
                }
                catch (Exception)
                {
                    // not this method's problem
                }
            }
#if LOG_CACHE_MISSES
            Logger.AddLogLine($"Image Cache Miss: {sFile}");
#endif

            return false;
        }

        public static void AddInMemoryImageToCache(string sFile, Bitmap zBitmap)
        {
            var sKey = Path.GetFileNameWithoutExtension(sFile).ToLower();
            if (s_dictionaryImages.ContainsKey(sKey))
            {
                RemoveCacheEntry(s_dictionaryImages, sKey);
            }
            ApplyBitmapTags(zBitmap);
            s_dictionaryImages[sKey] = new BitmapCacheEntry(zBitmap, DateTime.UtcNow, ImageCacheEntryType.InMemory);
        }

        private static BitmapCacheEntry CacheImage(Dictionary<string, BitmapCacheEntry> dictionaryCache, string sKey, string sFile, Bitmap zBitmap, ImageCacheEntryType eEntryType)
        {
            switch (eEntryType)
            {
                case ImageCacheEntryType.InMemory:
                    break;
                case ImageCacheEntryType.File:
                    var sExistingFilePath = GetExistingFilePath(sFile);
                    if (sExistingFilePath == null)
                    {
                        // do not cache missing files
                        return null;
                    }
                    break;
                default:
                    return null;
            }

            ApplyBitmapTags(zBitmap);
            var zCacheEntry = new BitmapCacheEntry(zBitmap, File.GetLastWriteTimeUtc(sFile), eEntryType);
            if (dictionaryCache.ContainsKey(sKey))
            {
                RemoveCacheEntry(dictionaryCache, sKey);
            }
            dictionaryCache[sKey] = zCacheEntry;
            return zCacheEntry;
        }

        private static string GetExistingFilePath(string sFile)
        {
            if (!File.Exists(sFile))
            {
                sFile = Path.Combine(ProjectManager.Instance.ProjectPath,sFile);
                if (!File.Exists(sFile))
                {
                    if (sFile.Length > 1)
                    {
                        switch (sFile[0])
                        {
                            case '/':
                            case '\\':
                                // last ditch effort (support for files like this: "/file.png"
                                sFile = Path.Combine(ProjectManager.Instance.ProjectPath, sFile.Substring(1));
                                if (File.Exists(sFile))
                                {
                                    return sFile;
                                }
                                break;
                            default:
                                return null;
                        }
                    }
                    return null;
                }
            }

            return sFile;
        }

        private static void RemoveCacheEntry(IDictionary<string, BitmapCacheEntry> dictionaryImages,
            string sKey)
        {
            if (dictionaryImages.TryGetValue(sKey, out var zEntry))
            {
                dictionaryImages.Remove(sKey);
                DisposeEntry(zEntry);
                foreach (var sDependentKey in zEntry.DependentCustomCacheKeys)
                {
                    RemoveCacheEntry(s_dictionaryCustomImages, sDependentKey);
                }
            }
        }

        private static void ApplyBitmapTags(Bitmap zBitmap)
        {
            // preserve the aspect ratio on the tag
            zBitmap.Tag = (float)zBitmap.Width / (float)zBitmap.Height;
        }

        private static void DumpImagesFromDictionary(IDictionary<string, BitmapCacheEntry> dictionaryImages)
        {
            foreach (var zCacheEntry in dictionaryImages.Values)
            {
                DisposeEntry(zCacheEntry);
            }
            dictionaryImages.Clear();
        }

        private static void DisposeEntry(BitmapCacheEntry zCacheEntry)
        {
            try
            {
                zCacheEntry.Bitmap.Dispose();
            }
            catch (Exception)
            {
                // do not care... (how bad is this?)
            }
        }
    }
}
