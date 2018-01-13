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
using CardMaker.Card.Shapes;
using CardMaker.XML;

namespace CardMaker.Card.Render
{
    class InlineElementRenderProcessor : IElementRenderProcessor
    {
        private static readonly IInlineBackgroundElementProcessor s_zInlineBackgroundElementProcessor = new InlineBackgroundElementProcessor();
        private readonly IDrawGraphic m_zDrawGraphic;
        private readonly IShapeRenderer m_zShapeRenderer;

        private InlineElementRenderProcessor() { }

        public InlineElementRenderProcessor(IShapeRenderer zShapeRenderer, IDrawGraphic zDrawGraphic)
        {
            m_zShapeRenderer = zShapeRenderer;
            m_zDrawGraphic = zDrawGraphic;
        }

        public string Render(Graphics zGraphics, ProjectLayoutElement zElement, Deck zDeck, string sInput, int nX, int nY, bool bExport)
        {
            // TODO: this should just be a sequence of processors 1) transform 2) background 3) background shape 4) render 5) blah
            // render any inline shape (max of 1)
            sInput = s_zInlineBackgroundElementProcessor.ProcessInlineShape(m_zShapeRenderer, zGraphics, zElement, sInput);

            // render any inline background image (max of 1)
            sInput = s_zInlineBackgroundElementProcessor.ProcessInlineBackgroundGraphic(m_zDrawGraphic, zGraphics, zElement, sInput);

            return sInput;
        }
    }
}
