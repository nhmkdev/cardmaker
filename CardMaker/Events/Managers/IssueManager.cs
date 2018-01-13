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

using CardMaker.Events.Args;

namespace CardMaker.Events.Managers
{
    /// <summary>
    /// Manages issue tracking across the application
    /// </summary>
    public class IssueManager
    {
        private static IssueManager m_zInstance;

        /// <summary>
        /// Fired when the card information has changed
        /// </summary>
        public event IssueCardInfoChanged CardInfoChanged;

        /// <summary>
        /// Fired when the element has changed
        /// </summary>
        public event IssueElementChanged ElementChanged;

        /// <summary>
        /// Fired when an issue refresh is requested
        /// </summary>
        public event IssueRefreshRequest RefreshRequested;

        /// <summary>
        /// Fired when a new issue has been added
        /// </summary>
        public event IssueAdded IssueAdded;

        public static IssueManager Instance => m_zInstance ?? (m_zInstance = new IssueManager());

        #region Event Triggers

        /// <summary>
        /// Adjusts the issue tracking to the specified details
        /// </summary>
        /// <param name="nLayout"></param>
        /// <param name="nCard"></param>
        public void FireChangeCardInfoEvent(int nLayout, int nCard)
        {
            CardInfoChanged?.Invoke(this, new IssueCardInfoEventArgs(nLayout, nCard));
        }

        /// <summary>
        /// Adjusts the issue track to the given element name
        /// </summary>
        /// <param name="sName"></param>
        public void FireChangeElementEvent(string sName)
        {
            ElementChanged?.Invoke(this, new IssueElementEventArgs(sName));
        }

        /// <summary>
        /// Adds an issue
        /// </summary>
        /// <param name="sIssue">The issue message</param>
        public void FireAddIssueEvent(string sIssue)
        {
            IssueAdded?.Invoke(this, new IssueMessageEventArgs(sIssue));
        }

        /// <summary>
        /// Fires the request event to refresh the issues
        /// </summary>
        public void FireRefreshRequestedEvent()
        {
            RefreshRequested?.Invoke(this, new IssueRefreshEventArgs());
        }

        #endregion
    }
}
