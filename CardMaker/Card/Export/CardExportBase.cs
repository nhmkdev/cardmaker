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
using CardMaker.Events.Managers;
using CardMaker.XML;

namespace CardMaker.Card.Export
{
    public abstract class CardExportBase
    {
        protected int m_nExportLayoutStartIndex;
        protected int m_nExportLayoutEndIndex;
        protected Bitmap m_zExportCardBuffer = null;
        protected ProjectLayout m_zLastLayout = null;
        
        public CardRenderer CardRenderer
        {
            get;
            private set;
        }

        protected CardExportBase(int nLayoutStartIndex, int nLayoutEndIndex)
        {
            m_nExportLayoutStartIndex = nLayoutStartIndex;
            m_nExportLayoutEndIndex = nLayoutEndIndex;
            CardRenderer = new CardRenderer
            {
                CurrentDeck = new Deck()
            };
        }

        protected void ChangePrintCardCanvas(int nIdx)
        {
            // based on the currently loaded project get the layout based on the index
            ProjectLayout zLayout = ProjectManager.Instance.LoadedProject.Layout[nIdx];
            SetCardLayout(zLayout);
        }

#warning consider removing this extra step...
        public Deck CurrentDeck 
        {
            get
            {
                return CardRenderer.CurrentDeck;
            }
        }

        ~CardExportBase()
        {
            if (m_zExportCardBuffer != null)
            {
                m_zExportCardBuffer.Dispose();
            }
        }

        public bool SetCardLayout(ProjectLayout zCardLayout)
        {
            // the setter does more than set
            return CurrentDeck.SetAndLoadLayout(zCardLayout ?? CurrentDeck.CardLayout, true);
        }

        public virtual void UpdateBufferBitmap(int nWidth, int nHeight, Graphics zGraphics = null)
        {
            if (null == m_zExportCardBuffer ||
                nWidth != m_zExportCardBuffer.Width ||
                nHeight != m_zExportCardBuffer.Height)
            {
                if (null != m_zExportCardBuffer)
                {
                    m_zExportCardBuffer.Dispose();
                }
                m_zExportCardBuffer = null == zGraphics
                    ? new Bitmap(nWidth, nHeight)
                    : new Bitmap(nWidth, nHeight, zGraphics);
            }
        }
    }
}
