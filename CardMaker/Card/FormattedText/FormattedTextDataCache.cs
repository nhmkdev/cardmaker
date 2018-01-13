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

using System;
using System.Collections.Generic;
using System.Drawing;
using CardMaker.Card.FormattedText.Markup;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText
{
    public class FormattedTextDataCache
    {
        private static readonly Type[] s_zTypeRenderOrder =
        {
            typeof(BackgroundColorMarkup),
            typeof(BackgroundImageMarkup),
            typeof(ImageMarkup),
            typeof(SpaceMarkup),
            typeof(TextMarkup)
        };

        private readonly Dictionary<Type, List<MarkupBase>> m_dictionaryTypeMarkupList;

        public FormattedTextDataCache()
        {
            m_dictionaryTypeMarkupList = new Dictionary<Type, List<MarkupBase>>();
            foreach (var renderType in s_zTypeRenderOrder)
            {
                m_dictionaryTypeMarkupList.Add(renderType, new List<MarkupBase>());
            }
        }

        public void AddMarkup(MarkupBase zMarkup)
        {
            if (m_dictionaryTypeMarkupList.ContainsKey(zMarkup.GetType()))
            {
                m_dictionaryTypeMarkupList[zMarkup.GetType()].Add(zMarkup);
            }
        }

        public void Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            foreach (var zRenderType in s_zTypeRenderOrder)
            {
                foreach (var zMarkup in m_dictionaryTypeMarkupList[zRenderType])
                {
                    if (!zMarkup.Render(zElement, zGraphics))
                    {
                        RenderLackOfSpaceWarning(zElement, zGraphics);
                    }
                }
            }         
        }

        private static void RenderLackOfSpaceWarning(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            zGraphics.DrawLine(Pens.Red, 0, zElement.height - 4, zElement.width, zElement.height - 4);
            zGraphics.DrawLine(Pens.Purple, 0, zElement.height - 8, zElement.width, zElement.height - 8);
            zGraphics.DrawLine(Pens.Blue, 0, zElement.height - 12, zElement.width, zElement.height - 12);            
        }
    }
}