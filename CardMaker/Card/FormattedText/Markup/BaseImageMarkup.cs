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
using System.Drawing;
using CardMaker.Data;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public abstract class BaseImageMarkup : MarkupValueBase
    {
        protected string m_sImageFile;
        protected Color m_colorImage = Color.Black;
        protected MirrorType m_eMirrorType = MirrorType.None;

        protected BaseImageMarkup(string sVariable) : base(sVariable) { }

        public override bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            return true;
        }

        protected Bitmap LoadImage(ProjectLayoutElement zElement)
        {
            return ImageCache.LoadCustomImageFromCache(m_sImageFile, zElement, m_colorImage, -1, -1, m_eMirrorType);
        }
    }
}
