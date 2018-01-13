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
using CardMaker.Data;
using CardMaker.XML;

namespace CardMaker.Card.Render
{
    class TypeElementRenderProcessor : IElementRenderProcessor
    {
        private readonly IShapeRenderer m_zShapeRenderer;
        private readonly IDrawGraphic m_zDrawGraphic;
        private readonly IDrawFormattedText m_zDrawFormattedText;
        private readonly IDrawText m_zDrawText;

        private TypeElementRenderProcessor() { }

        public TypeElementRenderProcessor(IShapeRenderer zShapeRenderer, IDrawGraphic zDrawGraphic, IDrawFormattedText zDrawFormattedText, IDrawText zDrawText)
        {
            m_zShapeRenderer = zShapeRenderer;
            m_zDrawGraphic = zDrawGraphic;
            m_zDrawFormattedText = zDrawFormattedText;
            m_zDrawText = zDrawText;
        }

        public string Render(Graphics zGraphics, ProjectLayoutElement zElement, Deck zDeck, string sInput, int nX, int nY, bool bExport)
        {
            switch (EnumUtil.GetElementType(zElement.type))
            {
                case ElementType.Text:
                    m_zDrawText.DrawText(zGraphics, zElement, sInput);
                    break;
                case ElementType.FormattedText:
                    m_zDrawFormattedText.Draw(zGraphics, zDeck, zElement, sInput);
                    break;
                case ElementType.Graphic:
                    m_zDrawGraphic.DrawGraphicFile(zGraphics, sInput, zElement);
                    break;
                case ElementType.Shape:
                    m_zShapeRenderer.HandleShapeRender(zGraphics, sInput.ToLower(), zElement);
                    break;
            }
            return sInput;
        }
    }
}
