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

namespace CardMaker.Events.Managers
{
    /// <summary>
    /// Manages issue tracking across the application
    /// </summary>
    public class IssueManager
    {
        private static IssueManager m_zIssueManager;

        /// <summary>
        /// Fired when the card information has changed
        /// </summary>
        public event IssueCardInfoChanged CardInfoChanged;

        /// <summary>
        /// Fired when the element has changed
        /// </summary>
        public event IssueElementChanged ElementChanged;

        /// <summary>
        /// Fired when a new issue has been added
        /// </summary>
        public event IssueAdded IssueAdded;

        public static IssueManager Instance
        {
            get
            {
                if (null == m_zIssueManager)
                {
                    m_zIssueManager = new IssueManager();
                }
                return m_zIssueManager;
            }
        }

        /// <summary>
        /// Adjusts the issue tracking to the specified details
        /// </summary>
        /// <param name="nLayout"></param>
        /// <param name="nCard"></param>
        public void ChangeCardInfo(int nLayout, int nCard)
        {
            if (null != CardInfoChanged)
            {
                CardInfoChanged(this, new IssueCardInfoEventArgs(nLayout, nCard));
            }
        }

        /// <summary>
        /// Adjusts the issue track to the given element name
        /// </summary>
        /// <param name="sName"></param>
        public void ChangeElement(string sName)
        {
            if (null != ElementChanged)
            {
                ElementChanged(this, new IssueElementEventArgs(sName));
            }            
        }

        /// <summary>
        /// Adds an issue
        /// </summary>
        /// <param name="sIssue">The issue message</param>
        public void AddIssue(string sIssue)
        {
            if (null != IssueAdded)
            {
                IssueAdded(this, new IssueMessageEventArgs(sIssue));
            }
        }
    }
}
