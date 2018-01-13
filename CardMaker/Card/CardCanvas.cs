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
using System.Windows.Forms;

namespace CardMaker.Card
{
    public class CardCanvas : UserControl
    {
        private readonly CardRenderer m_zCardRenderer;

        private Deck ActiveDeck => m_zCardRenderer.CurrentDeck;

        public CardRenderer CardRenderer => m_zCardRenderer;

        private readonly Size m_zDefaultNoLayoutSize = new Size(320, 80);

        public CardCanvas()
        {
            m_zCardRenderer = new CardRenderer
            {
                ZoomLevel = 1.0f,
            };
            // double buffer and optimize!
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Opaque, true);
            Reset(null);
        }

        public void Reset(Deck zDeck)
        {
            m_zCardRenderer.CurrentDeck = zDeck;
            UpdateSize();
            Invalidate();
        }

        ~CardCanvas()
        {
            ImageCache.ClearImageCaches();
        }

        public void UpdateSize()
        {
            // TODO: may want to compare current and previous size if this is costly
            Size = ActiveDeck?.CardLayout != null 
                ? new Size((int)((float)ActiveDeck.CardLayout.width * m_zCardRenderer.ZoomLevel), (int)((float)ActiveDeck.CardLayout.height * m_zCardRenderer.ZoomLevel)) 
                : m_zDefaultNoLayoutSize;
        }

        #region paint overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            if (ActiveDeck?.CardLayout == null)
            {
                e.Graphics.FillRectangle(Brushes.White, 0, 0, Size.Width, Size.Height);
                e.Graphics.DrawString("Select a Layout in the Project Window", new Font(DefaultFont.FontFamily, 20), Brushes.Red, new RectangleF(10, 10, Size.Width - 10, Size.Height - 10));
                return;
            }
            
            if (-1 != ActiveDeck.CardIndex && ActiveDeck.CardIndex < ActiveDeck.CardCount)
            {
                m_zCardRenderer.DrawCard(0, 0, e.Graphics, ActiveDeck.CurrentLine, false, true);
            }
        }

        #endregion
    }
}
