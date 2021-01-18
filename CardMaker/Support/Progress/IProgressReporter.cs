////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2021 Tim Stair
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
    public interface IProgressReporter
    {
        /// <summary>
        /// Flag indicating the success/failure of the associated thread.
        /// </summary>
        bool ThreadSuccess { get; set; }

        /// <summary>
        /// Flag indicating the cancel state of the reporter
        /// </summary>
        bool Canceled { get; set; }

        /// <summary>
        /// Controls the visibility of the cancel button
        /// </summary>
        bool CancelButtonVisible { get; set; }

        /// <summary>
        /// Gets the number of progress sub statuses
        /// </summary>
        /// <returns></returns>
        int GetProgressCount();

        /// <summary>
        /// Gets the index of the given progress
        /// </summary>
        /// <param name="sProgressName">The name to lookup</param>
        /// <returns>The index or -1 if not found</returns>
        int GetProgressIndex(string sProgressName);

        /// <summary>
        /// Sets the status text
        /// </summary>
        /// <param name="sText"></param>
        void SetStatusText(string sText);

        /// <summary>
        /// Resets the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to reset (0 based)</param>
        /// <param name="nMin">The minimum value to set on the progress bar</param>
        /// <param name="nMax">The maximum value to set on the progress bar</param>
        /// <param name="nStartVal">The starting value to set on the progress bar</param>
        void ProgressReset(int nProgressBar, int nMin, int nMax, int nStartVal);

        /// <summary>
        /// Sets the value on the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to set (0 based)</param>
        /// <param name="nValue">The value to set</param>
        void ProgressSet(int nProgressBar, int nValue);

        /// <summary>
        /// Steps the value on the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to step (0 based)</param>
        void ProgressStep(int nProgressBar);

        /// <summary>
        /// Gets the value of the specified progress bar.
        /// </summary>
        /// <param name="nProgressBar">The progress bar to get (0 based)</param>
        /// <returns></returns>
        int ProgressGet(int nProgressBar);

        /// <summary>
        /// Initiates the underlying processing
        /// </summary>
        /// <param name="initializationObject">The object to pass to the underlying reporter</param>
        void StartProcessing(object initializationObject);

        /// <summary>
        /// Adds an issue for the reporter to process as necessary.
        /// </summary>
        /// <param name="sIssue"></param>
        void AddIssue(string sIssue);

        /// <summary>
        /// Shuts down the reporter as necessary.
        /// </summary>
        void Shutdown();
    }
}
