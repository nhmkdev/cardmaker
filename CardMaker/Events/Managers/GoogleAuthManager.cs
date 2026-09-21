////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2026 Tim Stair
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

using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.Properties;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Support.IO;
using Support.UI;

namespace CardMaker.Events.Managers
{
    public class GoogleAuthManager
    {
        public const string GOOGLE_LOCAL_ID = "user";
        public const string GOOGLE_FILE_STORE = "google";

        private static GoogleAuthManager m_zInstance;

        public GoogleAuthUpdateRequested GoogleAuthUpdateRequested;

        public UserCredential UserCredential { get; private set; }

        public static GoogleAuthManager Instance => m_zInstance ?? (m_zInstance = new GoogleAuthManager());

        private GoogleAuthManager()
        {
            
        }

        #region Event Triggers

        /// <summary>
        /// Fires the GoogleAuthUpdateRequested event
        /// </summary>
        /// <param name="zParentForm">The parent form to use if any dialogs are displayed</param>
        /// <param name="zSuccessAction">The action to perform on success</param>
        /// <param name="zCancelAction">The action to perform on cancel</param>
        public void FireGoogleAuthUpdateRequestedEvent(Form zParentForm, Action zSuccessAction = null, Action zCancelAction = null)
        {
            GoogleAuthUpdateRequested?.Invoke(this, new GoogleAuthEventArgs(zParentForm, zSuccessAction, zCancelAction));
        }

        #endregion

        /// <summary>
        /// Checks if the google credentials are set. This will result in a prompt for the user (via event).
        /// </summary>
        /// <param name="zParentForm">The parent form to use if any dialogs are displayed</param>
        /// <returns>true if the credentials are set, false otherwise</returns>
        public bool CheckGoogleAuth(Form zParentForm)
        {
            if (UserCredential != null)
            {
                return true;
            }
            if (DialogResult.Cancel == MessageBox.Show(zParentForm,
                    "You do not appear to have any Google credentials configured. Press OK to configure.",
                    "Google Credentials Missing",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information))
            {
                return false;
            }
            Instance.FireGoogleAuthUpdateRequestedEvent(zParentForm);
            return UserCredential != null;
        }

        /// <summary>
        /// Updates the Google auth directly (no events or prompts, just a cancel dialog)
        /// </summary>
        /// <param name="zParentForm"></param>
        /// <param name="zSuccessAction"></param>
        /// <param name="zCancelAction"></param>
        public void UpdateGoogleAuth(Form zParentForm, Action zSuccessAction = null, Action zCancelAction = null)
        {
            if (UserCredential != null)
            {
                // nothing to do
                return;
            }

            var tokenSource = new CancellationTokenSource();
            // to avoid issues with the user canceling the "which app" prompt or otherwise a wait dialog with a cancel is displayed
            var zWait = new WaitDialog(1, CredentialUiFlowThread, tokenSource.Token, "Google Auth", new string[]{"Waiting..."}, 200);
            zWait.ShowDialog(zParentForm);
            if (zWait.Canceled)
            {
                tokenSource.Cancel();
                zCancelAction?.Invoke();
            }
            else
            {
                zSuccessAction?.Invoke();
            }

        }

        /// <summary>
        /// Updates the google auth without any forms interactions
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="zSuccessAction">Action to execute on success</param>
        public void UpdateGoogleAuth(CancellationToken cancellationToken, Action zSuccessAction = null)
        {
            CredentialFlow(new CredentialFlowParameters()
            {
                CancellationToken = cancellationToken,
                Success = zSuccessAction
            });
            if (UserCredential != null)
            {
                zSuccessAction?.Invoke();
            }
        }

        private void CredentialUiFlowThread(object zParamObject)
        {
            CredentialFlow(new CredentialFlowParameters()
            {
                CancellationToken = (CancellationToken)zParamObject,
                Success = () => WaitDialog.Instance.ThreadSuccess = true,
                Failure = () => WaitDialog.Instance.ThreadSuccess = false,
                Finally = () => WaitDialog.Instance.CloseWaitDialog()
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zParameters">parameters for handling different aspects of the credential flow</param>
        private void CredentialFlow(CredentialFlowParameters zParameters)
        {
            try
            {
                var sDataStorePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), GOOGLE_FILE_STORE);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.NotASecret)))
                {
                    var credentialTask = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        CardMakerConstants.GOOGLE_SCOPES,
                        // Unique ID for storing token cache locally (appended to the file name)
                        GOOGLE_LOCAL_ID, 
                        zParameters.CancellationToken,
                        // makes a subfolder for credentials (usable across runs too!)
                        new FileDataStore(sDataStorePath, true)
                    );
                    credentialTask.Wait(zParameters.CancellationToken);
                    UserCredential = credentialTask.Result;
                    zParameters.Success?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogLine($"Error: Failed to complete Google Auth: {ex}");
                zParameters.Failure?.Invoke();
            }
            zParameters.Finally?.Invoke();
        }

        private class CredentialFlowParameters
        {
            public Action Success { get; set; }
            public Action Failure { get; set; }
            public Action Finally { get; set; }
            public CancellationToken CancellationToken { get; set; }
        }
    }
}
