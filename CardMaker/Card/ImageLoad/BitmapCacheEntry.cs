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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Support.IO;

namespace CardMaker.Card.ImageLoad
{
    public enum ImageCacheEntryType
    {
        File,
        Url,
        InMemory,
        Missing,
    }

    internal class BitmapCacheEntry
    {
        public Bitmap Bitmap { get; }
        public string FullFilePath { get; } // note: if more custom fields are required just create an abstract cache entry
        public DateTime LastWriteTimestamp { get; }
        public ImageCacheEntryType EntryType { get; }
        public HashSet<string> DependentCustomCacheKeys { get; } = new HashSet<string>();
        public bool PlaceHolder { get; }

        public BitmapCacheEntry(Bitmap zBitmap, DateTime dtLastWriteTimestamp, ImageCacheEntryType eEntryType, string sFile, bool bPlaceHolder = false)
        {
            Bitmap = zBitmap;
            FullFilePath = eEntryType == ImageCacheEntryType.File ? ImagePathUtil.GetExistingFilePath(sFile) : null;
            LastWriteTimestamp = dtLastWriteTimestamp;
            EntryType = eEntryType;
            PlaceHolder = bPlaceHolder;
        }

        public void AddDependentCustomCacheKey(string sKey)
        {
            DependentCustomCacheKeys.Add(sKey);
        }

        public bool IsCacheEntryValid(string sFile)
        {
            // NOTE: this entire method is very file specific
            if (EntryType != ImageCacheEntryType.File)
            {
                return true;
            }
            try
            {
                if (FullFilePath == null || !File.Exists(FullFilePath))
                {
                    Logger.AddLogLine($"cached associated file doesn't exist: {FullFilePath}");
                    return false;
                }

                var dateTimeLastWriteTime = File.GetLastWriteTime(FullFilePath);
                if (dateTimeLastWriteTime > LastWriteTimestamp)
                {
                    Logger.AddLogLine($"file lastWriteTime: {FullFilePath} {dateTimeLastWriteTime} > {LastWriteTimestamp}");
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                // not this method's problem
                return false;
            }
        }
    }
}
