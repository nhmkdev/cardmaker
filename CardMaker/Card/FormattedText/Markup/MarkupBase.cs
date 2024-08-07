﻿////////////////////////////////////////////////////////////////////////////////
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
using System.Drawing;
using CardMaker.XML;

namespace CardMaker.Card.FormattedText.Markup
{
    public abstract class MarkupBase
    {
        protected MarkupBase()
        {
            TargetRect = RectangleF.Empty;
        }

        public int LineNumber { get; set; }

        /// <summary>
        /// Indicates whether this type of markup aligns
        /// </summary>
        public virtual bool Aligns => false;

        public StringAlignment StringAlignment { get; private set; }

        public virtual bool Appendable => false;

        public RectangleF TargetRect { get; set; }

        /// <summary>
        /// Processes the markup to determine the markup stack information (font settings, rectangle sizes/settings)
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="zData"></param>
        /// <param name="zProcessData"></param>
        /// <param name="zGraphics"></param>
        /// <returns>true if this markup is to be further processed</returns>
        public bool ProcessMarkup(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            if (Aligns)
            {
                StringAlignment = zProcessData.CurrentStringAlignment;
            }
            return ProcessMarkupHandler(zElement, zData, zProcessData, zGraphics);
        }

        /// <summary>
        /// Attempts to append the markup to a prior markup.
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="zData"></param>
        /// <param name="zProcessData"></param>
        /// <param name="zGraphics"></param>
        /// <param name="zMarkupToAppend"></param>
        /// <returns>True on success, false otherwise</returns>
        public virtual bool TryAppendMarkup(ProjectLayoutElement zElement, FormattedTextData zData,
            FormattedTextProcessData zProcessData, Graphics zGraphics, MarkupBase zMarkupToAppend)
        {
            return false;
        }

        /// <summary>
        /// Processes the markup to determine the markup stack information (font settings, rectangle sizes/settings)
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="zData"></param>
        /// <param name="zProcessData"></param>
        /// <param name="zGraphics"></param>
        /// <returns>true if this markup is to be further processed</returns>
        protected virtual bool ProcessMarkupHandler(ProjectLayoutElement zElement, FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics)
        {
            return false;
        }


        /// <summary>
        /// Second pass after rectangles are configured
        /// </summary>
        /// <param name="zElement"></param>
        /// <param name="listAllMarkups"></param>
        /// <param name="nMarkup"></param>
        /// <returns>true if this markup is to be cached</returns>
        public virtual bool PostProcessMarkupRectangle(ProjectLayoutElement zElement, List<MarkupBase> listAllMarkups, int nMarkup)
        {
            return false;
        }

        public virtual void CloseMarkup(FormattedTextData zData, FormattedTextProcessData zProcessData, Graphics zGraphics) { }

        /// <summary>
        /// Renders the given markup
        /// </summary>
        /// <param name="zElement">The element being rendered</param>
        /// <param name="zGraphics">The Graphics to draw to</param>
        /// <returns>true if the markup was rendered, false if not (due to space or other issues)</returns>
        public virtual bool Render(ProjectLayoutElement zElement, Graphics zGraphics)
        {
            return true;
        }

        public static MarkupBase GetMarkup(string sInput)
        {
            // check for the value based tags
            var arraySplit = sInput.Split(new char[] { '=' });

            try
            {
                if (2 == arraySplit.Length)
                {
                    var typeMarkup = MarkupUtil.GetMarkupType(arraySplit[0]);
                    if (null != typeMarkup)
                    {
                        return (MarkupBase) Activator.CreateInstance(typeMarkup, new object[] {arraySplit[1]});
                    }
                }
                else
                {
                    // this check is after the value based tag check because some tags may optionally use values
                    var typeMarkup = MarkupUtil.GetMarkupType(sInput);
                    if (null != typeMarkup)
                    {
                        return (MarkupBase) Activator.CreateInstance(typeMarkup);
                    }
                }
            }
            catch (Exception)
            {}
            return null;
        }    
    }
}