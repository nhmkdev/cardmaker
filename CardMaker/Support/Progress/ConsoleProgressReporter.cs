////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Support.Progress
{
    /// <summary>
    /// Command Line progress reporter (console based wait dialog)
    /// </summary>
    class ConsoleProgressReporter : IProgressReporter
    {
        public bool WriteToConsole { get; set; }

        private const int PROGRESS_WIDTH = 30;
        private readonly ThreadStart m_zThreadStart;
        private readonly ParameterizedThreadStart m_zParameterizedThreadStart;
        private readonly object m_zParamObject;
        private readonly string m_sTitle;
        private object m_zStepLockObject = new object();
        private object m_zRenderLockObject = new object();
        private List<ProgressLine> m_listProgressLines;
        private List<string> m_listIssues = new List<string>();
        private Dictionary<string, int> m_dictionaryProgressIndex = new Dictionary<string, int>();
        private bool m_bRendered = false;
        private int m_nConsoleRenderLine;
        private StringBuilder m_zOutputBuilder = new StringBuilder();

        public ConsoleProgressReporter(string sTitle, string[] arrayDescriptions, ThreadStart zThreadStart)
        {
            m_sTitle = sTitle;
            m_zThreadStart = zThreadStart;
            InitializeProgressLines(arrayDescriptions);
        }

        public ConsoleProgressReporter(string sTitle, string[] arrayDescriptions, ParameterizedThreadStart zThreadStart, object zParamObject)
        {
            m_sTitle = sTitle;
            m_zParameterizedThreadStart = zThreadStart;
            m_zParamObject = zParamObject;
            InitializeProgressLines(arrayDescriptions);
        }

        public bool ThreadSuccess { get; set; }

        public bool Canceled { get; set; }

        public bool CancelButtonVisible { get; set; }

        public int GetProgressCount()
        {
            return m_listProgressLines.Count;
        }

        public int GetProgressIndex(string sProgressName)
        {
            int nIdx;
            return m_dictionaryProgressIndex.TryGetValue(sProgressName, out nIdx) ? nIdx : -1;
        }

        public void SetStatusText(string sText)
        {
           Console.WriteLine(sText);
        }

        public void ProgressReset(int nProgressBar, int nMin, int nMax, int nStartVal)
        {
            m_listProgressLines[nProgressBar].Min = nMin;
            m_listProgressLines[nProgressBar].Max = nMax;
            m_listProgressLines[nProgressBar].Value = nStartVal;
            Render();
        }

        public void ProgressSet(int nProgressBar, int nValue)
        {
            m_listProgressLines[nProgressBar].Value = nValue;
            Render();
        }

        public void ProgressStep(int nProgressBar)
        {
            lock (m_zStepLockObject)
            {
                m_listProgressLines[nProgressBar].Value = Math.Min(m_listProgressLines[nProgressBar].Max,
                    m_listProgressLines[nProgressBar].Value + 1);
            }

            Render();
        }

        public int ProgressGet(int nProgressBar)
        {
            return m_listProgressLines[nProgressBar].Value;
        }

        public void StartProcessing(object initializationObject)
        {
            if (m_zThreadStart != null)
            {
                m_zThreadStart();
            }
            else if (m_zParameterizedThreadStart != null)
            {
                m_zParameterizedThreadStart(m_zParamObject);
            }
        }

        public void AddIssue(string sIssue)
        {
            m_listIssues.Add(sIssue);
        }

        public void Shutdown()
        {
            if (m_listIssues.Count > 0)
            {
                Console.WriteLine("Issues:");
                m_listIssues.ForEach(i => Console.WriteLine(i));
            }
        }

        /// <summary>
        /// Initializes the ProgressLine objects based on the passed in array of descriptions
        /// </summary>
        /// <param name="arrayDescriptions">The descriptions to apply to each line</param>
        private void InitializeProgressLines(string[] arrayDescriptions)
        {
            m_listProgressLines = new List<ProgressLine>(arrayDescriptions.Length);
            for(var nIdx = 0; nIdx< arrayDescriptions.Length; nIdx++)
            {
                m_dictionaryProgressIndex[arrayDescriptions[nIdx]] = nIdx;
                m_listProgressLines.Add(new ProgressLine(arrayDescriptions[nIdx]));
            }
        }

        /// <summary>
        /// Renders the current state of the reporter (has to jump the cursor around the console)
        /// </summary>
        private void Render()
        {
            if(!WriteToConsole) return;
            lock (m_zRenderLockObject)
            {
                m_zOutputBuilder.Clear();
                Console.CursorVisible = false;
                if (m_bRendered)
                {
#if false // this introduced flicker (don't need that!)
                    // clear all the lines starting with the render point
                    for (var nConsoleLine = m_nConsoleRenderLine;
                        nConsoleLine < m_listProgressLines.Count + 1;
                        nConsoleLine++)
                    {
                        Console.SetCursorPosition(0, nConsoleLine);
                        Console.Write("".PadRight(Console.WindowWidth));
                    }
#endif
                    Console.SetCursorPosition(0, m_nConsoleRenderLine);
                }
                else
                {
                    m_nConsoleRenderLine = Console.CursorTop;
                    m_bRendered = true;
                }

                m_zOutputBuilder.AppendLine(m_sTitle.PadRight(Console.WindowWidth, ' '));

                m_listProgressLines.ForEach(zLine =>
                {
                    var nScaledValue = (int) ((((float) zLine.Value - (float) zLine.Min) / (float) zLine.Max) *
                                              (float) PROGRESS_WIDTH);
                    m_zOutputBuilder.AppendLine("[" + ("".PadRight(nScaledValue, '+')).PadRight(PROGRESS_WIDTH, ' ') + "] " +
                                                zLine.Description);
                });
                Console.WriteLine(m_zOutputBuilder.ToString());
                //Thread.Sleep(500);
                Console.CursorVisible = true;
            }
        }
    }

    /// <summary>
    /// Tracks the state of an individual sub progress status
    /// </summary>
    internal class ProgressLine
    {
        public string Description { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Value { get; set; }

        public ProgressLine(string sDescription) : this(sDescription, 0, 100, 0)
        {

        }

        public ProgressLine(string sDescription, int nMin, int nMax, int nInitialValue)
        {
            Description = sDescription;
            Min = nMin;
            Max = nMax;
            Value = nInitialValue;
        }
    }
}
