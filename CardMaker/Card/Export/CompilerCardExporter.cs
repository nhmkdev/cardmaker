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
using Support.UI;

namespace CardMaker.Card.Export
{
    /// <summary>
    /// This is an exporter used to scan the project for errors
    /// </summary>
    public class CompilerCardExporter : CardExportBase, ICardExporter
    {
        public CompilerCardExporter(int nLayoutStartIndex, int nLayoutEndIndex)
            : base(nLayoutStartIndex, nLayoutEndIndex)
        {
        }

        public void ExportThread()
        {
            WaitDialog zWait = WaitDialog.Instance;
            for (int nIdx = ExportLayoutStartIndex; nIdx < ExportLayoutEndIndex; nIdx++)
            {
                IssueManager.Instance.FireChangeCardInfoEvent(nIdx, 1);
                IssueManager.Instance.FireChangeElementEvent(string.Empty);
                ChangeExportLayoutIndex(nIdx);
                zWait.ProgressReset(1, 0, CurrentDeck.CardCount, 0);

                UpdateBufferBitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);
                var zGraphics = Graphics.FromImage(m_zExportCardBuffer);

                for (var nCardIdx = 0; nCardIdx < CurrentDeck.CardCount; nCardIdx++)
                {
                    IssueManager.Instance.FireChangeCardInfoEvent(nIdx, nCardIdx + 1);
                    CurrentDeck.CardPrintIndex = nCardIdx;
                    CardRenderer.DrawPrintLineToGraphics(zGraphics);
                    zWait.ProgressStep(1);
                }
                zWait.ProgressStep(0);
            }
            zWait.ThreadSuccess = true;
            zWait.CloseWaitDialog();
        }
    }
}
