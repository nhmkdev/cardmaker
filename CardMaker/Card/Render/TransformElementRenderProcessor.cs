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
using CardMaker.Data;
using CardMaker.XML;

namespace CardMaker.Card.Render
{
    class TransformElementRenderProcessor : IElementRenderProcessor
    {
        public string Render(Graphics zGraphics, ProjectLayoutElement zElement, Deck zDeck, string sInput, int nX, int nY, bool bExport)
        {
            if (0 != zElement.rotation)
            {
                // center the internal element then rotate and restore
                zGraphics.TranslateTransform(zElement.x + nX + (zElement.width >> 1), zElement.y + nY + (zElement.height >> 1));
                zGraphics.RotateTransform(zElement.rotation);
                zGraphics.TranslateTransform(-(zElement.width >> 1), -(zElement.height >> 1));
                if (CardMakerInstance.DrawElementBorder && CardMakerInstance.DrawSelectedElementRotationBounds && !bExport)
                {
                    zGraphics.DrawRectangle(Pens.LightGreen, 0, 0, zElement.width - 1, zElement.height - 1);
                }
            }
            else
            {
                zGraphics.TranslateTransform(zElement.x + nX, zElement.y + nY);
            }

            return sInput;
        }
    }
}
