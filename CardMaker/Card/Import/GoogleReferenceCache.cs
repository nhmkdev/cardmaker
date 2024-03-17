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

using System;
using System.Collections.Generic;
using System.IO;
using CardMaker.Data;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Import
{
    public static class GoogleReferenceCache
    {
        // public required to serialize
        public class GoogleCacheItem
        {
            public string Reference { get; set; }
            public List<List<string>> Data { get; set; }
        }

        private const string GOOGLE_CACHE_FILE = ".CardMakerGoogleCache.dat";

        private static readonly Dictionary<string, List<List<string>>> s_dictionaryDataCache = new Dictionary<string, List<List<string>>>();
        private static bool s_bDirty = false;


        public static string GetCacheFilePath()
        {
            return Path.Combine(CardMakerInstance.StartupPath, GOOGLE_CACHE_FILE);
        }

        /// <summary>
        /// Updates the cache entry
        /// </summary>
        /// <param name="sKey">key to update</param>
        /// <param name="listData">data to associate with entry</param>
        public static void UpdateCacheEntry(string sKey, List<List<string>> listData)
        {
            s_bDirty = true;
            s_dictionaryDataCache[sKey] = listData;
        }

        /// <summary>
        /// Gets the cache entry associated with the key
        /// </summary>
        /// <param name="sKey">Key to lookup</param>
        /// <param name="listCachedData">list to write to</param>
        /// <returns>true if it exists, false otherwise</returns>
        public static bool GetCacheEntry(string sKey, out List<List<string>> listCachedData)
        {
            return s_dictionaryDataCache.TryGetValue(sKey, out listCachedData);
        }

        /// <summary>
        /// Determines if the key is in the cache
        /// </summary>
        /// <param name="sKey">key to lookup</param>
        /// <returns>true if it exists, false otherwise</returns>
        public static bool IsEntryInCache(string sKey)
        {
            return s_dictionaryDataCache.ContainsKey(sKey);
        }

        /// <summary>
        /// Initializes the cache
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        public static bool ReadFromDisk()
        {
            s_dictionaryDataCache.Clear();
            List<GoogleCacheItem> listCacheItems = null;
            if (SerializationUtils.DeserializeFromXmlFile(
                    GetCacheFilePath(),
                    CardMakerConstants.XML_ENCODING,
                    ref listCacheItems))
            {
                foreach (var zCacheItem in listCacheItems)
                {
                    UpdateCacheEntry(zCacheItem.Reference, zCacheItem.Data);
                }
                s_bDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Flushes the cache to disk if dirty
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        public static bool FlushToDisk()
        {
            if (!s_bDirty)
            {
                return true;
            }
            var listCacheItems = new List<GoogleCacheItem>();
            foreach (var zPair in s_dictionaryDataCache)
            {
                listCacheItems.Add(new GoogleCacheItem()
                {
                    Reference = zPair.Key,
                    Data = zPair.Value
                });
            }

            if (SerializationUtils.SerializeToXmlFile(
                    GetCacheFilePath(),
                    listCacheItems,
                    CardMakerConstants.XML_ENCODING))
            {
                s_bDirty = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes and clears the local Google Sheets cache
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        public static bool DeleteGoogleCache()
        {
            try
            {
                File.Delete(GetCacheFilePath());
                s_dictionaryDataCache.Clear();
                Logger.AddLogLine("Cleared Google Cache");
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Failed to delete Google Cache File: {0} - {1}".FormatString(GetCacheFilePath(), ex.Message));
            }
            return false;
        }
    }
}
