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

using System.Collections.Generic;

namespace CardMaker.Card.Import
{
    public class ReferenceCache
    {
        private static object s_lock = new object();
        private static bool m_bEnabled;

        public static bool Enabled
        {
            get => m_bEnabled;
            set
            {
                m_bEnabled = value;
#if false
                Logger.AddLogLine($"Reference cache {(m_bEnabled ? "enabled" : "disabled")}.");
#endif
                if (m_bEnabled)
                {
                    Clear();
                }
            }
        }

        // this is a static cache as no files will clash
        private static Dictionary<string, List<ReferenceLine>> s_dictionaryReferenceCache =
            new Dictionary<string, List<ReferenceLine>>();

        public static bool TryGetCachedReference(string sKey, out List<ReferenceLine> listReferenceLines)
        {
            if (Enabled)
            {
                lock (s_lock)
                {
                    if (s_dictionaryReferenceCache.TryGetValue(sKey, out var listCachedReferenceLines))
                    {
#warning DeckReader trashes these lists (so dupe them) - Doesn't affect google cache due to List<ReferenceLine> create
                        listReferenceLines = new List<ReferenceLine>(listCachedReferenceLines);
                        return true;
                    }
                }
            }
            listReferenceLines = null;
            return false;
        }

        public static void CacheReference(string sKey, List<ReferenceLine> listCachedReferenceLines)
        {
            if (!Enabled)
            {
                return;
            }
            lock (s_lock)
            {
#warning DeckReader trashes these lists (so dupe them) - Doesn't affect google cache due to List<ReferenceLine> create
                s_dictionaryReferenceCache[sKey] = new List<ReferenceLine>(listCachedReferenceLines);
            }
        }

        public static void Clear()
        {
            lock (s_lock)
            {
                s_dictionaryReferenceCache.Clear();
            }
        }
    }
}
