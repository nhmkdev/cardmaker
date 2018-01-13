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
using CardMaker.XML;

namespace CardMaker.Card.Render
{
    class BorderElementRenderProcessor : IElementRenderProcessor
    {
        public string Render(Graphics zGraphics, ProjectLayoutElement zElement, Deck zDeck, string sInput, int nX, int nY, bool bExport)
        {
            if (0 != zElement.borderthickness)
            {
                // note that the border is inclusive in the width/height consuming 2 pixels (0 to total-1)
                zGraphics.DrawRectangle(
                    255 == zElement.opacity
                        ? new Pen(zElement.GetElementBorderColor(), zElement.borderthickness)
                        : new Pen(Color.FromArgb(zElement.opacity, zElement.GetElementBorderColor()), zElement.borderthickness),
                    0,
                    0,
                    zElement.width - 1,
                    zElement.height - 1);
            }

            return sInput;
        }
    }
}
