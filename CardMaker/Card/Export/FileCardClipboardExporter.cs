////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CardMaker.Data;
using Support.IO;
using Support.Progress;

namespace CardMaker.Card.Export
{
    public class FileCardClipboardExporter : CardExportBase
    {
        private readonly int m_nImageExportIndex;

        public FileCardClipboardExporter(int nLayoutStartIndex, int nLayoutEndIdx, int nImageExportIndex)
            : base(nLayoutStartIndex, nLayoutEndIdx)
        {
            m_nImageExportIndex = nImageExportIndex;
        }

        public override void ExportThread()
        {
            var progressLayoutIdx = ProgressReporter.GetProgressIndex(ProgressName.LAYOUT);
            var progressCardIdx = ProgressReporter.GetProgressIndex(ProgressName.CARD);

            ProgressReporter.ProgressReset(progressLayoutIdx, 0, ExportLayoutIndices.Length, 0);
            ChangeExportLayoutIndex(ExportLayoutIndices[0]);
            ProgressReporter.ProgressReset(progressCardIdx, 0, CurrentDeck.CardCount, 0);

            UpdateBufferBitmap(CurrentDeck.CardLayout.width, CurrentDeck.CardLayout.height);

            var zGraphics = Graphics.FromImage(m_zExportCardBuffer);
            var nCardIdx = m_nImageExportIndex;
            zGraphics.Clear(CurrentDeck.CardLayout.exportTransparentBackground ?
                CardMakerConstants.NoColor :
                Color.White);
            CurrentDeck.ResetDeckCache();
            CurrentDeck.CardPrintIndex = nCardIdx++;

            // special case for clipboard exports... need to write files somewhere (need to document this)
            ProcessSubLayoutExports(CardMakerInstance.StartupPath);

            CardRenderer.DrawPrintLineToGraphics(zGraphics, 0, 0, !CurrentDeck.CardLayout.exportTransparentBackground);
            m_zExportCardBuffer.SetResolution(CurrentDeck.CardLayout.dpi, CurrentDeck.CardLayout.dpi);

            ProgressReporter.ProgressStep(progressLayoutIdx);

            try
            {
                ProcessRotateExport(m_zExportCardBuffer, CurrentDeck.CardLayout, false);
                var thread = new Thread(() => Clipboard.SetImage(m_zExportCardBuffer));
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join(); //Wait for the thread to end  
            }
            catch (Exception)
            {
                Logger.AddLogLine("Error copying layout image to clipboard.");
                ProgressReporter.ThreadSuccess = false;
                ProgressReporter.Shutdown();
                return;
            }

            ProgressReporter.ProgressStep(progressCardIdx);

            ProgressReporter.ThreadSuccess = true;
            ProgressReporter.Shutdown();
        }
    }
}
