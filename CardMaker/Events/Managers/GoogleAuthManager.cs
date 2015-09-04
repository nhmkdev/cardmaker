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

using System;
using CardMaker.Events.Args;

namespace CardMaker.Events.Managers
{
    public class GoogleAuthManager
    {
        private static GoogleAuthManager m_zInstance;

        public GoogleAuthUpdateRequested GoogleAuthUpdateRequested;

        public GoogleAuthCredentialsError GoogleAuthCredentialsError;

        public static GoogleAuthManager Instance
        {
            get
            {
                if (m_zInstance == null)
                {
                    m_zInstance = new GoogleAuthManager();
                }
                return m_zInstance;
            }
        }

        private GoogleAuthManager()
        {
            
        }

        #region Event Triggers

        /// <summary>
        /// Fires the GoogleAuthUpdateRequested event
        /// </summary>
        /// <param name="zSuccessAction">The action to perform on success</param>
        /// <param name="zCancelAction">The action to perform on cancel</param>
        public void FireGoogleAuthUpdateRequestedEvent(Action zSuccessAction = null, Action zCancelAction = null)
        {
            if (null != GoogleAuthUpdateRequested)
            {
                GoogleAuthUpdateRequested(this, new GoogleAuthEventArgs(zSuccessAction, zCancelAction));
            }
        }

        /// <summary>
        /// Fires the GoogleAuthCredentialsError event
        /// </summary>
        /// <param name="zSuccessAction">The action to perform on success</param>
        /// <param name="zCancelAction">The action to perform on cancel</param>
        public void FireGoogleAuthCredentialsErrorEvent(Action zSuccessAction = null, Action zCancelAction = null)
        {
            if (null != GoogleAuthCredentialsError)
            {
                GoogleAuthCredentialsError(this, new GoogleAuthEventArgs(zSuccessAction, zCancelAction));
            }
        }

        #endregion
    }
}
