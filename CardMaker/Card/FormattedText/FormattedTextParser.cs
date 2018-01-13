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

using System.Collections.Generic;
using System.Text;
using CardMaker.Card.FormattedText.Markup;
using Support.IO;

namespace CardMaker.Card.FormattedText
{
    public static class FormattedTextParser
    {
        /// <summary>
        /// Markup parser
        /// </summary>
        /// <param name="sInput"></param>
        /// <returns></returns>
        public static List<MarkupBase> GetMarkups(string sInput)
        {
            var listMarkups = new List<MarkupBase>();

            var nIdx = 0;
            var bEscapeNext = false;
            var bInTag = false;
            var bCloseTag = false;

            var sTagName = string.Empty;
            var sBuilder = new StringBuilder();

            while (sInput.Length > nIdx)
            {
                if (bEscapeNext)
                {
                    // NOTE: this also is gated by !bInTag (when bEscapeNext is set)
                    bEscapeNext = false;
                    switch (sInput[nIdx])
                    {
                        case '<':
                        case '>':
                        case '\\':
                            sBuilder.Append(sInput[nIdx]);
                            nIdx++;
                            continue;
                        case 'n':
                            if (sBuilder.Length > 0)
                            {
                                // add any existing text to a text markup
                                listMarkups.Add(new TextMarkup(sBuilder.ToString()));
                                sBuilder = new StringBuilder();
                            }
                            listMarkups.Add(new NewlineMarkup());
                            nIdx++;
                            continue;
                        default:
                            // append the missing \
                            sBuilder.Append("\\");
                            break;
                    }
                }

                var cCurrent = sInput[nIdx];
                var cLast = nIdx > 0 ? sInput[nIdx - 1] : ' ';

                switch (cCurrent)
                {
// no newline should ever make it from a reference to here without conversion
                    case '\n':
                        Logger.AddLogLine("Newline found in formatted text input. This is a bug! Report it!");
                        break;
                    case '\\':
                        if (!bInTag)
                        {
                            bEscapeNext = true;
                        }
                        else
                        {
                            sTagName += "\\";
                        }
                        break;
                    case '<':
                        if (!bInTag && sBuilder.Length > 0)
                        {
                            listMarkups.Add(new TextMarkup(sBuilder.ToString()));
                        }
                        bInTag = true;
                        sTagName = string.Empty;
                        sBuilder = new StringBuilder();
                        break;
                    case '>':
                        bInTag = false;
                        if (bCloseTag)
                        {
                            // find the tag to actually close
                            var zMarkupTypeToSeek = MarkupBase.GetMarkupType(sTagName);
                            if (null != zMarkupTypeToSeek)
                            {
                                for (int nMarkup = listMarkups.Count - 1; nMarkup > -1; nMarkup--)
                                {
                                    if (listMarkups[nMarkup].GetType() == zMarkupTypeToSeek)
                                    {
                                        sTagName = string.Empty;
                                        listMarkups.Add(new CloseTagMarkup(listMarkups[nMarkup]));
                                        break;
                                    }
                                }
                            }
                            bCloseTag = false;
                            // otherwise it is a junk markup, toss it out
                        }
                        else
                        {
                            var zMarkup = MarkupBase.GetMarkup(sTagName);
                            if (null != zMarkup)
                            {
                                // prevent continued adds of the same tag
                                sTagName = string.Empty;
                                listMarkups.Add(zMarkup);
                            }
                        }
                        break;
                    case ' ':
                        if (!bInTag && sBuilder.Length > 0)
                        {
                            listMarkups.Add(new TextMarkup(sBuilder.ToString()));
                            sBuilder = new StringBuilder();
                        }
                        if (bInTag)
                        {
                            sTagName += cCurrent;
                        }
                        else
                        {

                            if (listMarkups.Count == 0)
                            {
                                break;
                            }
                            var zMarkupBase = listMarkups[listMarkups.Count - 1];
                            if (!(zMarkupBase is SpaceMarkup))
                            {
                                // create a trimmable space (can be trimmed from end of line)
                                listMarkups.Add(new SpaceMarkup(true));
                            }
                        }
                        break;
                    default:
                        if (bInTag)
                        {
                            // this is just lazy, not checking if this is actually after the <
                            if ('/' == cCurrent && cLast == '<')
                            {
                                bCloseTag = true;
                            }
                            else
                            {
                                sTagName += cCurrent;
                            }
                        }
                        else
                        {
                            sBuilder.Append(cCurrent);
                        }
                        break;
                }
                nIdx++;
            }

            // trailing escape code char
            if (bEscapeNext)
                sBuilder.Append("\\");

            if (!bInTag && sBuilder.Length > 0)
            {
                listMarkups.Add(new TextMarkup(sBuilder.ToString()));
            }
            return listMarkups;
        }
    }
}
