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

//#define LOG_CACHE_MISSES
#define LOG_CACHE_ACTIONS

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CardMaker.Data;
using CardMaker.Data.Serialization;
using CardMaker.Events.Managers;
using CardMaker.XML;
#if !MONO_BUILD
using SkiaSharp;
using SkiaSharp.Views.Desktop;
#endif
using Support.IO;

namespace CardMaker.Card.ImageLoad
{
    public static class ImageCache
    {
        private const int IMAGE_CACHE_MAX = 500;
        private const int IMAGE_CUSTOM_CACHE_MAX = 1000;
        // cache of plain images (no adjustments)
        private static readonly ConcurrentDictionary<string, BitmapCacheEntry> s_dictionaryImages = new ConcurrentDictionary<string, BitmapCacheEntry>();
        // cache of images with in-memory tweaks
        private static readonly ConcurrentDictionary<string, BitmapCacheEntry> s_dictionaryCustomImages = new ConcurrentDictionary<string, BitmapCacheEntry>();
        private static readonly List<IImageLoader> ImageLoaders = new List<IImageLoader>
        {
            new ImageUrlLoader(),
            new ImageFileLoader() // default (always valid)
        };

        private static bool RunUrlDownloadThread = true;
        private static readonly ConcurrentQueue<string> UrlDownloadQueue = new ConcurrentQueue<string>();

        public static bool ExportMode { get; set; } = false;

        static ImageCache()
        {
            Task.Run(LoadBitmapUrlBackgroundThread);
        }

        public static void ClearImageCaches()
        {
            DumpImagesFromDictionary(s_dictionaryImages);
            DumpImagesFromDictionary(s_dictionaryCustomImages);
        }

        public static Bitmap LoadCustomImageFromCache(string sFile, ProjectLayoutElement zElement,
            int nTargetWidth = -1, int nTargetHeight = -1)
        {
            return LoadCustomImageFromCache(
                sFile,
                zElement,
                zElement.GetElementColor(),
                zElement.GetColorMatrix(),
                (ElementColorType)zElement.colortype,
                nTargetWidth, 
                nTargetHeight, 
                zElement.GetMirrorType());
        }

        public static Bitmap LoadCustomImageFromCache(
            string sFile, 
            ProjectLayoutElement zElement, 
            Color colorOverride,
            ColorMatrix zColorMatrix,
            ElementColorType eColorType,
            int nTargetWidth = -1, 
            int nTargetHeight = -1, 
            MirrorType eMirrorType = MirrorType.None
        )
        {
#warning move key gen to another function
            var sColorMatrixKeyComponent = ColorMatrixSerializer.SerializeToString(zColorMatrix);
            var sKey = sFile.ToLower() +
                       ":" + zElement.opacity +
                       ":" + ProjectLayoutElement.GetElementColorString(colorOverride) +
                       ":" + sColorMatrixKeyComponent +
                       ":" + (int)eColorType +
                       ":" + nTargetWidth +
                       ":" + nTargetHeight +
                       ":" + eMirrorType;
#if LOG_CACHE_ACTIONS
            Logger.AddLogLine($"Attempting custom image cache lookup: {sKey}");
#endif

            if (GetCacheEntry(s_dictionaryCustomImages, sKey, sFile, out var zCacheEntry))
            {
                return zCacheEntry.Bitmap;
            }

            var eElementType = EnumUtil.GetElementType(zElement.type);

            // note: default loader will be the file based one
            var zImageLoader = GetImageLoader(sFile);

            var zBaseImageCacheEntry = LoadImageFromCache(zImageLoader, sFile);
            if (null == zBaseImageCacheEntry)
            {
                return null;
            }

            var zSourceBitmap = zBaseImageCacheEntry.Bitmap;
            // if the desired width/height/opacity match the 'plain' cached copy just return it (or special color handling for certain element types)
            // TODO: make a method for this just to shrink all this logic down
            if (
                (-1 == nTargetWidth || zSourceBitmap.Width == nTargetWidth)
                && (-1 == nTargetHeight || zSourceBitmap.Height == nTargetHeight)
                && 255 == zElement.opacity
            )
            {
                switch (eElementType)
                {
                    case ElementType.FormattedText:
                    case ElementType.Graphic:
                        if (!IsColorSet(colorOverride, eColorType))
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
            if (s_dictionaryCustomImages.Count > IMAGE_CUSTOM_CACHE_MAX)
            {
                DumpImagesFromDictionary(s_dictionaryCustomImages);
            }

            var zImageAttributes = new ImageAttributes();
            zImageAttributes.SetColorMatrix(GenerateColorMatrix(zElement, eElementType, eColorType, colorOverride, zColorMatrix));

            nTargetWidth = nTargetWidth == -1 ? zSourceBitmap.Width : nTargetWidth;
            nTargetHeight = nTargetHeight == -1 ? zSourceBitmap.Height : nTargetHeight;

            var zDestinationBitmap = new Bitmap(nTargetWidth, nTargetHeight); // target image
            var zGraphics = Graphics.FromImage(zDestinationBitmap);

            MirrorRender.MirrorElementGraphicTransform(zGraphics, zElement, eMirrorType, nTargetWidth, nTargetHeight);

            // draw the source image into the destination with the desired opacity
            zGraphics.DrawImage(zSourceBitmap, new Rectangle(0, 0, nTargetWidth, nTargetHeight), 0, 0, zSourceBitmap.Width, zSourceBitmap.Height, GraphicsUnit.Pixel,
                zImageAttributes);
            Logger.AddLogLine($"Add custom image key to cache: {sKey}");
            // CUSTOM IMAGE CACHE
            CacheImage(s_dictionaryCustomImages, zImageLoader, sKey, sFile, zDestinationBitmap, zBaseImageCacheEntry.EntryType);
            zBaseImageCacheEntry.AddDependentCustomCacheKey(sKey);
            return zDestinationBitmap;
        }

        private static BitmapCacheEntry LoadImageFromCache(IImageLoader zImageLoader, string sFile)
        {
            var sKey = sFile.ToLower();
#if LOG_CACHE_ACTIONS
            Logger.AddLogLine($"Attempting base image cache lookup [{zImageLoader.GetType().Name}]: {sKey}");
#endif

            if (GetCacheEntry(s_dictionaryImages, sKey, sFile, out var zExistingCacheEntry))
            {
                return zExistingCacheEntry;
            }
            
            if (s_dictionaryImages.Count > IMAGE_CACHE_MAX)
            {
#warning TODO: this is a terrible eviction strategy
                DumpImagesFromDictionary(s_dictionaryImages);
            }

            Bitmap zSourceBitmap;
            if (!ExportMode && zImageLoader.UseBackgroundThread)
            {
                zSourceBitmap = CreateInfoBitmap($"Downloading {sFile}", Brushes.ForestGreen, Brushes.Black);
            }
            else
            {
#if LOG_CACHE_ACTIONS
                Logger.AddLogLine($"Attempting image load[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                zSourceBitmap = zImageLoader.LoadBitmap(sFile);
                if (zSourceBitmap == null)
                {
                    return null;
                }
            }

            var zDestinationBitmap = zImageLoader.UseMemoryCopy ? CopyAndDisposeBitmap(zSourceBitmap) : zSourceBitmap;

            // BASE IMAGE CACHE
            var zCacheEntry = CacheImage(s_dictionaryImages, zImageLoader, sKey, sFile, zDestinationBitmap, zImageLoader.CacheEntryType, zImageLoader.UseBackgroundThread);
            if (zImageLoader.UseBackgroundThread)
            {
#if LOG_CACHE_ACTIONS
                Logger.AddLogLine($"Queuing image load[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                // queue up the image load, ONLY after the placeholder is cached
                UrlDownloadQueue.Enqueue(sFile);
            }
            return zCacheEntry;
        }

        private static void LoadBitmapUrlBackgroundThread()
        {
            while (RunUrlDownloadThread)
            {
                if (!UrlDownloadQueue.TryDequeue(out var sFile))
                {
                    Thread.Sleep(500);
                    continue;
                }
                var zImageLoader = GetImageLoader(sFile);
                if (zImageLoader == null)
                {
                    continue;
                }
                var sKey = sFile.ToLower();
#if LOG_CACHE_ACTIONS
                Logger.AddLogLine($"Background: Checking base image cache[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                if (GetCacheEntry(s_dictionaryImages, sKey, sFile, out var zCacheEntry) && !zCacheEntry.PlaceHolder)
                {
#if LOG_CACHE_ACTIONS
                    Logger.AddLogLine($"Background: Load ignore, already cached[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                    // do not re-download
                    continue;
                }

#if LOG_CACHE_ACTIONS
                Logger.AddLogLine($"Background: Load started[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                try
                {
                    var zSourceBitmap = zImageLoader.LoadBitmap(sFile);
                    Bitmap zDestinationBitmap;
                    if (zSourceBitmap == null)
                    {
#if LOG_CACHE_ACTIONS
                        Logger.AddLogLine($"Background: Load failed[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                        zDestinationBitmap =
                            CreateInfoBitmap($"Load failed for {sFile}", Brushes.LightCoral, Brushes.Black);
                    }
                    else
                    {
                        zDestinationBitmap = zImageLoader.UseMemoryCopy
                            ? CopyAndDisposeBitmap(zSourceBitmap)
                            : zSourceBitmap;
                    }

                    // BASE IMAGE CACHE (replacing placeholder)
                    CacheImage(s_dictionaryImages, zImageLoader, sKey, sFile, zDestinationBitmap,
                        ImageCacheEntryType.Url);
#if LOG_CACHE_ACTIONS
                    Logger.AddLogLine($"Background: Load completed[{zImageLoader.GetType().Name}]: {sFile}");
#endif
                    LayoutManager.Instance.FireLayoutRenderUpdatedEvent();
                }
                catch (Exception ex)
                {
#if LOG_CACHE_ACTIONS
                    Logger.AddLogLine($"Background: Load threw exception[{zImageLoader.GetType().Name}]: {sFile} - {ex}");
#endif
                }
            }
        }

        /// <summary>
        /// Makes an in-memory copy of the bitmap so the source can be disposed (primarily for File sources so they can be externally edited)
        /// </summary>
        /// <param name="zSourceBitmap"></param>
        /// <returns></returns>
        private static Bitmap CopyAndDisposeBitmap(Bitmap zSourceBitmap)
        {
            // copy the contents into a new image (to allow the source to be disposed)
            var zDestinationBitmap = new Bitmap(zSourceBitmap.Width, zSourceBitmap.Height);
            var zGraphics = Graphics.FromImage(zDestinationBitmap);
            zGraphics.DrawImage(zSourceBitmap,
                new Rectangle(0, 0, zDestinationBitmap.Width, zDestinationBitmap.Height),
                0, 0, zDestinationBitmap.Width, zDestinationBitmap.Height, GraphicsUnit.Pixel);

            // duping the image into a memory copy allows the file to change (not locked by the application)
            zSourceBitmap.Dispose();
            return zDestinationBitmap;
        }

        private static Bitmap CreateInfoBitmap(string sMessage, Brush brushBackground, Brush brushText)
        {
            var zPlaceholderBitmap = new Bitmap(300, 300);
            var zGraphics = Graphics.FromImage(zPlaceholderBitmap);
            zGraphics.FillRectangle(brushBackground, new Rectangle(0, 0, zPlaceholderBitmap.Width, zPlaceholderBitmap.Height));
            zGraphics.DrawString(sMessage, FontLoader.DefaultFont, brushText, new RectangleF(0, 0, zPlaceholderBitmap.Width, zPlaceholderBitmap.Height));
            return zPlaceholderBitmap;
        }

        private static bool GetCacheEntry(IDictionary<string, BitmapCacheEntry> dictionaryImageCache, string sKey, string sFile, out BitmapCacheEntry zCacheEntry)
        {
            zCacheEntry = null;
            return dictionaryImageCache.TryGetValue(sKey, out zCacheEntry) && zCacheEntry.IsCacheEntryValid(sFile);
        }

        public static void CacheInMemoryImage(string sFile, Bitmap zBitmap)
        {
            var sKey = Path.GetFileNameWithoutExtension(sFile).ToLower();
            UpdateImageCache(s_dictionaryImages, sKey, zBitmap, DateTime.UtcNow, ImageCacheEntryType.InMemory, null, false);
        }

        private static BitmapCacheEntry CacheImage(
            ConcurrentDictionary<string, BitmapCacheEntry> dictionaryCache, 
            IImageLoader zImageLoader,
            string sKey, 
            string sFile, 
            Bitmap zBitmap, 
            ImageCacheEntryType eEntryType,
            bool bPlaceholder = false)
        {
            var dtLastWriteTime = DateTime.UtcNow;
            if (ImageCacheEntryType.InMemory == eEntryType)
            {
                // HACK: the IImageLoader at this point is an ImageFileLoader, but in-memory images don't invalidate (ex: from sub-layouts)
                // no further checks needed
            }
            else
            {
                if (zImageLoader.ShouldCacheFile(sFile))
                {
                    dtLastWriteTime = zImageLoader.GetLastWriteTime(sFile);
                }
                else
                {
                    return null;
                }
            }
            return UpdateImageCache(dictionaryCache, sKey, zBitmap, dtLastWriteTime, eEntryType, sFile, bPlaceholder);
        }

        private static BitmapCacheEntry UpdateImageCache(
            ConcurrentDictionary<string, BitmapCacheEntry> dictionaryCache,
            string sKey,
            Bitmap zBitmap,
            DateTime dtLastWriteTime,
            ImageCacheEntryType eEntryType,
            string sFile,
            bool bPlaceholder)
        {
#if LOG_CACHE_ACTIONS
            Logger.AddLogLine($"Add key to cache: {sKey}");
#endif
            ApplyBitmapTags(zBitmap);
            var zCacheEntry = new BitmapCacheEntry(zBitmap, dtLastWriteTime, eEntryType, sFile, bPlaceholder);
            if (dictionaryCache.ContainsKey(sKey))
            {
                RemoveCacheEntry(dictionaryCache, sKey);
            }
            dictionaryCache[sKey] = zCacheEntry;
            return zCacheEntry;
        }

        private static void ApplyBitmapTags(Bitmap zBitmap)
        {
            // preserve the aspect ratio on the tag
            zBitmap.Tag = (float)zBitmap.Width / (float)zBitmap.Height;
        }

        /// <summary>
        /// Recursive cache removal (clears any dependents)
        /// </summary>
        /// <param name="dictionaryImages">Dictionary to remove the entry from</param>
        /// <param name="sKey">Key to remove</param>
        private static void RemoveCacheEntry(IDictionary<string, BitmapCacheEntry> dictionaryImages,
            string sKey)
        {
            if (!dictionaryImages.TryGetValue(sKey, out var zEntry))
            {
                return;
            }
            dictionaryImages.Remove(sKey);
            DisposeEntry(zEntry);
            foreach (var sDependentKey in zEntry.DependentCustomCacheKeys)
            {
                // Note: this specifically removes from custom images only
                RemoveCacheEntry(s_dictionaryCustomImages, sDependentKey);
            }
        
        }

        private static IImageLoader GetImageLoader(string sFile)
        {
            return ImageLoaders.FirstOrDefault(x => x.CanAttemptLoad(sFile));
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

        private static ColorMatrix GenerateColorMatrix(
            ProjectLayoutElement zElement, 
            ElementType eElementType,
            ElementColorType eColorType,
            Color colorOverride,
            ColorMatrix colorMatrixOverride)
        {
            if(ElementColorType.Matrix == eColorType)
            {
                return colorMatrixOverride ?? zElement.GetColorMatrix() ?? ColorMatrixSerializer.GetIdentityColorMatrix();
            }

            var zColorMatrix = new ColorMatrix();
            if (255 != zElement.opacity || 255 != colorOverride.A)
            {
                zColorMatrix.Matrix33 =
                    ((float)zElement.opacity / 255.0f) *
                    ((float)colorOverride.A / 255.0f);
            }
            // special color handling for certain element types
            if (!IsColorSet(colorOverride, eColorType))
            {
                return zColorMatrix;
            }
            switch (eElementType)
            {
                case ElementType.FormattedText:
                case ElementType.Graphic:
                    switch (eColorType)
                    {
                        case ElementColorType.Add:
                            zColorMatrix.Matrix40 = (float)colorOverride.R / 255.0f;
                            zColorMatrix.Matrix41 = (float)colorOverride.G / 255.0f;
                            zColorMatrix.Matrix42 = (float)colorOverride.B / 255.0f;
                            break;
                        case ElementColorType.Multiply:
                            zColorMatrix.Matrix00 = (float)colorOverride.R / 255.0f;
                            zColorMatrix.Matrix11 = (float)colorOverride.G / 255.0f;
                            zColorMatrix.Matrix22 = (float)colorOverride.B / 255.0f;
                            break;
                    }
                    break;
            }
            
            return zColorMatrix;
        }

        private static bool IsColorSet(Color color, ElementColorType eColorType)
        {
            return ElementColorType.Matrix == eColorType
                   || color.ToArgb() != Color.Black.ToArgb();
        }
    }
}
