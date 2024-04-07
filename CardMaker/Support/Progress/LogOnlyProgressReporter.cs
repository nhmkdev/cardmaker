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

using Support.IO;

namespace Support.Progress
{
    class LogOnlyProgressReporter : IProgressReporter
    {
        public bool ThreadSuccess { get; set; }

        public bool Canceled { get; set; }

        public bool CancelButtonVisible { get; set; }

        public int GetProgressCount()
        {
            return 0;
        }


        public int GetProgressIndex(string sProgressName)
        {
            return 0;
        }


        public void SetStatusText(string sText)
        {
        }


        public void ProgressReset(int nProgressBar, int nMin, int nMax, int nStartVal)
        {
        }


        public void ProgressSet(int nProgressBar, int nValue)
        {
        }


        public void ProgressStep(int nProgressBar)
        {
        }


        public int ProgressGet(int nProgressBar)
        {
            return 0;
        }


        public void StartProcessing(object initializationObject)
        {

        }


        public void AddIssue(string sIssue)
        {
            Logger.AddLogLine(sIssue);
        }


        public void Shutdown()
        {
        }
    }
}
