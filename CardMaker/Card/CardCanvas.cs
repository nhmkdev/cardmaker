////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using CardMaker.Forms;
using CardMaker.XML;

namespace CardMaker.Card
{
    public class CardCanvas : UserControl
    {
        private bool m_bPreviewUpdate;
        private readonly CardRenderer m_zCardRenderer;

        public Deck ActiveDeck
        {
            get
            {
                return m_zCardRenderer.CurrentDeck;
            }
        }

        public CardRenderer CardRenderer 
        {
            get
            {
                return m_zCardRenderer;
            }
        }

        public CardCanvas()
        {
            m_zCardRenderer = new CardRenderer
            {
                ZoomLevel = 1.0f,
                DrawElementBorder = true,
                DrawFormattedTextBorder = false,
                CurrentDeck = new Deck()
            };
            // double buffer and optimize!
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Opaque, true);
        }

        ~CardCanvas()
        {
            DrawItem.DumpImages();
            DrawItem.DumpOpacityImages();
        }

        public void UpdatePreview()
        {
            m_bPreviewUpdate = true;
            Invalidate();
        }

        public void SetCardLayout(ProjectLayout zCardLayout)
        {
            ActiveDeck.SetAndLoadLayout(zCardLayout ?? ActiveDeck.CardLayout, false);
            Size = new Size(ActiveDeck.CardLayout.width, ActiveDeck.CardLayout.height);
        }

        public void UpdateSize()
        {
            if (null != ActiveDeck.CardLayout)
            {
                Size = new Size((int)((float)ActiveDeck.CardLayout.width * m_zCardRenderer.ZoomLevel), (int)((float)ActiveDeck.CardLayout.height * m_zCardRenderer.ZoomLevel));
            }
        }

        #region paint overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!m_bPreviewUpdate)
            {
                if (-1 != ActiveDeck.CardIndex && ActiveDeck.CardIndex < ActiveDeck.CardCount)
                {
                    m_zCardRenderer.DrawCard(0, 0, e.Graphics, ActiveDeck.CurrentLine, false, true);
                }
            }
            else // capture a grab by using a backbuffer to draw onto (preview only)
            {
                var zBackBuffer = new Bitmap(ClientSize.Width, ClientSize.Height);

                Graphics zGraphics = Graphics.FromImage(zBackBuffer);

                if (-1 != ActiveDeck.CardIndex)
                {
                    m_zCardRenderer.DrawCard(0, 0, zGraphics, ActiveDeck.CurrentLine, false, true);
                }

                zGraphics.Dispose();

                //Copy the back buffer to the screen
                e.Graphics.DrawImageUnscaled(zBackBuffer, 0, 0);

                MDIPreview.Instance.SetImage(zBackBuffer);

                m_bPreviewUpdate = false;
            }
        }

        #endregion
    }
}
