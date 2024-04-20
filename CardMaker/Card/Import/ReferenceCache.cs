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

        public static bool TryGetCachedReference(string sKey, out List<ReferenceLine> listCachedReferenceLines)
        {
            if (!Enabled)
            {
                listCachedReferenceLines = null;
                return false;
            }
            lock (s_lock)
            {
                return s_dictionaryReferenceCache.TryGetValue(sKey, out listCachedReferenceLines);
            }
        }

        public static void CacheReference(string sKey, List<ReferenceLine> listCachedReferenceLines)
        {
            if (!Enabled)
            {
                return;
            }
            lock (s_lock)
            {
                s_dictionaryReferenceCache[sKey] = listCachedReferenceLines;
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
