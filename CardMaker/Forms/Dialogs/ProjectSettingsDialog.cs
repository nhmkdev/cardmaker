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
using CardMaker.Card.Import;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.Properties;
using Support.UI;

namespace CardMaker.Forms.Dialogs
{
    class ProjectSettingsDialog
    {
        public static void ShowProjectSettings(Form parentForm)
        {
            const string TRANSLATOR = "translator";
            const string DEFAULT_DEFINE_REFERENCE_TYPE = "default_define_reference_type";
            const string OVERRIDE_DEFINE_REFRENCE_NAME = "override_define_reference_name";

            var zQuery = new QueryPanelDialog("Project Settings", 550, 300, false);
            zQuery.SetIcon(Resources.CardMakerIcon);

            TranslatorType eTranslator = ProjectManager.Instance.LoadedProjectTranslatorType;
            ReferenceType eDefaultDefineReferenceType = ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType;

            zQuery.AddPullDownBox("Translator",
                Enum.GetNames(typeof(TranslatorType)), (int)eTranslator, TRANSLATOR);

            zQuery.AddPullDownBox("Default Define Reference Type", Enum.GetNames(typeof(ReferenceType)), (int)eDefaultDefineReferenceType, DEFAULT_DEFINE_REFERENCE_TYPE);
            zQuery.AddSelectorBox(
                "Google Project define spreadsheet override",
                ProjectManager.Instance.LoadedProject.overrideDefineReferenceName,
                () =>
                {
                    if (GoogleAuthManager.CheckGoogleCredentials(parentForm))
                    {
                        return new GoogleSpreadsheetBrowser(GoogleReferenceReader.APP_NAME, GoogleReferenceReader.CLIENT_ID,
                            CardMakerInstance.GoogleAccessToken, false);
                    }
                    return null;
                },
                (zGoogleSpreadsheetBrowser, txtOverride) => { txtOverride.Text = zGoogleSpreadsheetBrowser.SelectedSpreadsheet.Title.Text; },
                OVERRIDE_DEFINE_REFRENCE_NAME);

            if (DialogResult.OK == zQuery.ShowDialog(parentForm))
            {
                ProjectManager.Instance.LoadedProject.translatorName = ((TranslatorType)zQuery.GetIndex(TRANSLATOR)).ToString();
                ProjectManager.Instance.LoadedProject.defaultDefineReferenceType = ((ReferenceType)zQuery.GetIndex(DEFAULT_DEFINE_REFERENCE_TYPE)).ToString();
                ProjectManager.Instance.LoadedProject.overrideDefineReferenceName =
                    zQuery.GetString(OVERRIDE_DEFINE_REFRENCE_NAME).Trim();
                ProjectManager.Instance.FireProjectUpdated(true);
                LayoutManager.Instance.InitializeActiveLayout();
            }
        }
    }
}
