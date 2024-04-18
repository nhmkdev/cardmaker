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

using System;
using System.Windows.Forms;
using CardMaker.Card.Import;
using CardMaker.Data;
using CardMaker.Events.Managers;
using Support.Google.Sheets;
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
            const string JS_ESCAPE_SINGLE_QUOTES = "js_escape_single_quotes";
            const string JS_TILDE_CODE = "js_tilde_code";
            const string JS_KEEP_FUNCTIONS = "js_keep_functions";

            var zQuery = FormUtils.InitializeQueryPanelDialog(new QueryPanelDialog("Project Settings", 550, 300, true));

            TranslatorType eTranslator = ProjectManager.Instance.LoadedProjectTranslatorType;
            ReferenceType eDefaultDefineReferenceType = ProjectManager.Instance.LoadedProjectDefaultDefineReferenceType;

            zQuery.ChangeToTab("Base");

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
                        return new GoogleSpreadsheetSelector(new GoogleSpreadsheet(CardMakerInstance.GoogleInitializerFactory), false);
                    }
                    return null;
                },
                (zGoogleSpreadsheetBrowser, txtOverride) =>
                {
                    txtOverride.Text = new GoogleSpreadsheetReference(zGoogleSpreadsheetBrowser.SelectedSpreadsheet)
                        .GenerateSpreadsheetReference();
                },
                OVERRIDE_DEFINE_REFRENCE_NAME);

            zQuery.ChangeToTab("Javascript");
            zQuery.AddCheckBox("Escape Single Quotes", ProjectManager.Instance.LoadedProject.jsEscapeSingleQuotes, JS_ESCAPE_SINGLE_QUOTES);
            zQuery.AddCheckBox("~ Means Code", ProjectManager.Instance.LoadedProject.jsTildeMeansCode, JS_TILDE_CODE);
            zQuery.AddCheckBox("Keep Functions", ProjectManager.Instance.LoadedProject.jsKeepFunctions, JS_KEEP_FUNCTIONS);

            if (DialogResult.OK == zQuery.ShowDialog(parentForm))
            {
                ProjectManager.Instance.LoadedProject.translatorName = ((TranslatorType)zQuery.GetIndex(TRANSLATOR)).ToString();
                ProjectManager.Instance.LoadedProject.defaultDefineReferenceType = ((ReferenceType)zQuery.GetIndex(DEFAULT_DEFINE_REFERENCE_TYPE)).ToString();
                ProjectManager.Instance.LoadedProject.overrideDefineReferenceName =
                    zQuery.GetString(OVERRIDE_DEFINE_REFRENCE_NAME).Trim();
                ProjectManager.Instance.LoadedProject.jsEscapeSingleQuotes = zQuery.GetBool(JS_ESCAPE_SINGLE_QUOTES);
                ProjectManager.Instance.LoadedProject.jsTildeMeansCode = zQuery.GetBool(JS_TILDE_CODE);
                ProjectManager.Instance.LoadedProject.jsKeepFunctions = zQuery.GetBool(JS_KEEP_FUNCTIONS);
                ProjectManager.Instance.FireProjectUpdated(true);
                LayoutManager.Instance.InitializeActiveLayout();
            }
        }
    }
}
