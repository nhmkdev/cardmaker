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

using System;
using System.Windows.Forms;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Forms;
using Support.IO;

namespace CardMaker.Events.Managers
{
    public class GoogleAuthManager
    {
        private static GoogleAuthManager m_zInstance;

        public GoogleAuthUpdateRequested GoogleAuthUpdateRequested;

        public GoogleAuthCredentialsError GoogleAuthCredentialsError;

        public static GoogleAuthManager Instance => m_zInstance ?? (m_zInstance = new GoogleAuthManager());

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
            GoogleAuthUpdateRequested?.Invoke(this, new GoogleAuthEventArgs(zSuccessAction, zCancelAction));
        }

        /// <summary>
        /// Fires the GoogleAuthCredentialsError event
        /// </summary>
        /// <param name="zSuccessAction">The action to perform on success</param>
        /// <param name="zCancelAction">The action to perform on cancel</param>
        public void FireGoogleAuthCredentialsErrorEvent(Action zSuccessAction = null, Action zCancelAction = null)
        {
            GoogleAuthCredentialsError?.Invoke(this, new GoogleAuthEventArgs(zSuccessAction, zCancelAction));
        }

        #endregion

        /// <summary>
        /// Checks if the google credentials are set.
        /// </summary>
        /// <returns>true if the credentials are set, false otherwise</returns>
        public static bool CheckGoogleCredentials(Form parentForm)
        {
            if (string.IsNullOrEmpty(CardMakerInstance.GoogleAccessToken))
            {
                if (DialogResult.Cancel == MessageBox.Show(parentForm,
                        "You do not appear to have any Google credentials configured. Press OK to configure.",
                        "Google Credentials Missing",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information))
                {
                    return false;
                }
                Instance.FireGoogleAuthUpdateRequestedEvent();
                return false;
            }
            return true;
        }

        public static void UpdateGoogleAuth(Form parentForm, Action zSuccessAction = null, Action zCancelAction = null)
        {
            var zDialog = new GoogleCredentialsDialog()
            {
                Icon = parentForm.Icon
            };
            switch (zDialog.ShowDialog(parentForm))
            {
                case DialogResult.OK:
                    CardMakerInstance.GoogleAccessToken = zDialog.GoogleAccessToken;
                    Logger.AddLogLine("Updated Google Credentials");
                    zSuccessAction?.Invoke();
                    break;
                default:
                    zCancelAction?.Invoke();
                    break;
            }
        }
    }
}
