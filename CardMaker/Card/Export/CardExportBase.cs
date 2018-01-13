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
using CardMaker.Events.Managers;
using CardMaker.XML;

namespace CardMaker.Card.Export
{
    public abstract class CardExportBase
    {
        protected Bitmap m_zExportCardBuffer;
        protected ProjectLayout m_zLastLayout = null;

        protected int ExportLayoutStartIndex { get; private set; }
        protected int ExportLayoutEndIndex { get; private set; }
        
        protected CardRenderer CardRenderer { get; }

        protected CardExportBase(int nLayoutStartIndex, int nLayoutEndIndex)
        {
            ExportLayoutStartIndex = nLayoutStartIndex;
            ExportLayoutEndIndex = nLayoutEndIndex;
            CardRenderer = new CardRenderer
            {
                CurrentDeck = new Deck()
            };
        }

        /// <summary>
        /// Changes the layout to the specified index in the project
        /// </summary>
        /// <param name="nIdx"></param>
        protected void ChangeExportLayoutIndex(int nIdx)
        {
            // based on the currently loaded project get the layout based on the index
            var zLayout = ProjectManager.Instance.LoadedProject.Layout[nIdx];
            CurrentDeck.SetAndLoadLayout(zLayout ?? CurrentDeck.CardLayout, true);
        }

        protected Deck CurrentDeck => CardRenderer.CurrentDeck;

        ~CardExportBase()
        {
            m_zExportCardBuffer?.Dispose();
        }

        /// <summary>
        /// Updates the existing buffer image if necessary
        /// </summary>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="zGraphics"></param>
        protected virtual void UpdateBufferBitmap(int nWidth, int nHeight, Graphics zGraphics = null)
        {
            if (null == m_zExportCardBuffer ||
                nWidth != m_zExportCardBuffer.Width ||
                nHeight != m_zExportCardBuffer.Height)
            {
                m_zExportCardBuffer?.Dispose();
                m_zExportCardBuffer = null == zGraphics
                    ? new Bitmap(nWidth, nHeight)
                    : new Bitmap(nWidth, nHeight, zGraphics);
            }
        }

        /// <summary>
        /// Rotates the export buffer based on the Layout exportRotation setting
        /// </summary>
        /// <param name="zBuffer"></param>
        /// <param name="zLayout"></param>
        /// <param name="postTransition"></param>
        protected void ProcessRotateExport(Bitmap zBuffer, ProjectLayout zLayout, bool postTransition)
        {
            switch (zLayout.exportRotation)
            {
                case 90:
                    zBuffer.RotateFlip(postTransition ? RotateFlipType.Rotate270FlipNone : RotateFlipType.Rotate90FlipNone);
                    break;
                case -90:
                    zBuffer.RotateFlip(postTransition ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone);
                    break;
            }
        }
    }
}
