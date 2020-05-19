////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2020 Tim Stair
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

namespace Support.Progress
{
    /// <summary>
    /// Wrapper for a ProgressReporter narrowed to a single Progress Index.
    /// Allows for nested ProgressReporting
    /// </summary>
    public class ProgressReporterProxy
    {
        public int ProgressIndex { get; set; }
        public IProgressReporter ProgressReporter { get; set; }
        public bool ProxyOwnsReporter { get; set; }

        public void ProgressReset(int nMin, int nMax, int nStartVal)
        {
            ProgressReporter.ProgressReset(ProgressIndex, nMin, nMax, nStartVal);
        }

        public void AddIssue(string sIssue)
        {
            ProgressReporter.AddIssue(sIssue);
        }

        public void ProgressStep()
        {
            ProgressReporter.ProgressStep(ProgressIndex);
        }

        /// <summary>
        /// Shuts down the underlying ProgressReporter if this is the owner
        /// </summary>
        public void Shutdown()
        {
            if (ProxyOwnsReporter)
            {
                ProgressReporter.Shutdown();
            }
        }
    }
}
