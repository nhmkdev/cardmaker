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

using System.Drawing;
using System.Text;

namespace CardMaker.Data
{
    public static class CardMakerConstants
    {
        public const string GOOGLE_REFERENCE = "google";
        public const char GOOGLE_REFERENCE_SPLIT_CHAR = ';';
        public static readonly Encoding XML_ENCODING = Encoding.UTF8;
        public const string VISIBLE_SETTING = ".visible";
        public const char CHAR_FILE_SPLIT = '|';
        public const int MAX_RECENT_PROJECTS = 10;
        public const string GOOGLE_CACHE_FILE = ".CardMakerGoogleCache.dat";
        public static readonly Color NoColor = Color.FromArgb(0, 0, 0, 0);
    }
}
