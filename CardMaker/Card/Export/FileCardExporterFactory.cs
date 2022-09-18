﻿////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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

using System.Drawing.Imaging;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.Forms;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Export
{
    public class FileCardExporterFactory
    {
        enum ExportOptionKey
        {
            Format,
            NameFormat,
            NameFormatOverride,
            Folder,
            StitchSkipIndex,
    }

        public static readonly ImageFormat[] AllowedImageFormats =
        {
            ImageFormat.Bmp,
            ImageFormat.Emf,
            ImageFormat.Exif,
            ImageFormat.Gif,
            ImageFormat.Icon,
            ImageFormat.Jpeg,
            ImageFormat.Png,
            ImageFormat.Tiff,
            ImageFormat.Wmf
        };

        public static readonly string[] AllowedImageFormatNames = AllowedImageFormats.Select(zFormat => zFormat.ToString()).ToArray();

        public static readonly Dictionary<string, ImageFormat> AllowedImageFormatDictionary =
            AllowedImageFormats.ToDictionary(zFormat => zFormat.ToString().ToUpper(), zFormat => zFormat);

        public static CardExportBase BuildFileCardExporter(bool bExportAllLayouts)
        {
            return bExportAllLayouts ? BuildProjectExporter() : BuildLayoutExporter();
        }

        private static CardExportBase BuildProjectExporter()
        {
            var zQuery = new QueryPanelDialog("Export to Images", 750, false);
            zQuery.SetIcon(CardMakerResources.CardMakerIcon);

            var sDefinition = ProjectManager.Instance.LoadedProject.exportNameFormat; // default to the project level definition
            var nDefaultFormatIndex = GetLastFormatIndex();

            zQuery.AddPullDownBox("Format", AllowedImageFormatNames, nDefaultFormatIndex, ExportOptionKey.Format);
            zQuery.AddCheckBox("Override Layout File Name Formats", false, ExportOptionKey.NameFormatOverride);
            zQuery.AddNumericBox("Stitch Skip Index", CardMakerSettings.ExportStitchSkipIndex, 0, 65535, 1, 0, ExportOptionKey.StitchSkipIndex);
            zQuery.AddTextBox("File Name Format (optional)", sDefinition ?? string.Empty, false, ExportOptionKey.NameFormat);
                // associated check box and the file format override text box
            zQuery.AddEnableControl(ExportOptionKey.NameFormatOverride, ExportOptionKey.NameFormat);
            zQuery.AddFolderBrowseBox("Output Folder", 
                Directory.Exists(ProjectManager.Instance.LoadedProject.lastExportPath) ? ProjectManager.Instance.LoadedProject.lastExportPath : ProjectManager.Instance.ProjectPath, 
                ExportOptionKey.Folder);
            zQuery.UpdateEnableStates();

            if (DialogResult.OK != zQuery.ShowDialog(CardMakerInstance.ApplicationForm))
            {
                return null;
            }
            var sFolder = zQuery.GetString(ExportOptionKey.Folder);
            SetupExportFolder(sFolder);

            if (!Directory.Exists(sFolder))
            {
                FormUtils.ShowErrorMessage("The folder specified does not exist!");
                return null;
            }

            ProjectManager.Instance.LoadedProject.lastExportPath = sFolder;
            var nStartLayoutIdx = 0;
            var nEndLayoutIdx = ProjectManager.Instance.LoadedProject.Layout.Length - 1;
            var bOverrideLayout = false;
            bOverrideLayout = zQuery.GetBool(ExportOptionKey.NameFormatOverride);

            CardMakerSettings.IniManager.SetValue(IniSettings.LastImageExportFormat, AllowedImageFormatNames[zQuery.GetIndex(ExportOptionKey.Format)]);
            CardMakerSettings.ExportStitchSkipIndex = (int)zQuery.GetDecimal(ExportOptionKey.StitchSkipIndex);

            return new FileCardExporter(nStartLayoutIdx, nEndLayoutIdx, sFolder, bOverrideLayout ? zQuery.GetString(ExportOptionKey.NameFormat) : null,
                CardMakerSettings.ExportStitchSkipIndex, AllowedImageFormats[zQuery.GetIndex(ExportOptionKey.Format)]);
        }

        private static CardExportBase BuildLayoutExporter()
        {
            var zQuery = new QueryPanelDialog("Export to Images", 750, false);
            zQuery.SetIcon(CardMakerResources.CardMakerIcon);

            var sDefinition = LayoutManager.Instance.ActiveLayout.exportNameFormat;
            var nDefaultFormatIndex = GetLastFormatIndex();


            zQuery.AddPullDownBox("Format", AllowedImageFormatNames, nDefaultFormatIndex, ExportOptionKey.Format);
            zQuery.AddNumericBox("Stitch Skip Index", CardMakerSettings.ExportStitchSkipIndex, 0, 65535, 1, 0, ExportOptionKey.StitchSkipIndex);
            zQuery.AddTextBox("File Name Format (optional)", sDefinition ?? string.Empty, false, ExportOptionKey.NameFormat);
            zQuery.AddFolderBrowseBox("Output Folder", 
                Directory.Exists(ProjectManager.Instance.LoadedProject.lastExportPath) ? ProjectManager.Instance.LoadedProject.lastExportPath : ProjectManager.Instance.ProjectPath, 
                ExportOptionKey.Folder);

            zQuery.UpdateEnableStates();

            if (DialogResult.OK != zQuery.ShowDialog(CardMakerInstance.ApplicationForm))
            {
                return null;
            }
            var sFolder = zQuery.GetString(ExportOptionKey.Folder);
            SetupExportFolder(sFolder);

            if (!Directory.Exists(sFolder))
            {
                FormUtils.ShowErrorMessage("The folder specified does not exist!");
                return null;
            }

            ProjectManager.Instance.LoadedProject.lastExportPath = sFolder;
            var nLayoutIndex = ProjectManager.Instance.GetLayoutIndex(LayoutManager.Instance.ActiveLayout);
            if (-1 == nLayoutIndex)
            {
                FormUtils.ShowErrorMessage("Unable to determine the current layout. Please select a layout in the tree view and try again.");
                return null;
            }

            CardMakerSettings.IniManager.SetValue(IniSettings.LastImageExportFormat, AllowedImageFormatNames[zQuery.GetIndex(ExportOptionKey.Format)]);
            CardMakerSettings.ExportStitchSkipIndex = (int)zQuery.GetDecimal(ExportOptionKey.StitchSkipIndex);

            return new FileCardExporter(nLayoutIndex, nLayoutIndex, sFolder, zQuery.GetString(ExportOptionKey.NameFormat),
                CardMakerSettings.ExportStitchSkipIndex, AllowedImageFormats[zQuery.GetIndex(ExportOptionKey.Format)]);
        }

        public static CardExportBase BuildImageExporter()
        {
            var zQuery = new QueryPanelDialog("Export Image", 750, false);
            zQuery.SetIcon(CardMakerResources.CardMakerIcon);

            var sDefinition = LayoutManager.Instance.ActiveLayout.exportNameFormat;
            var nDefaultFormatIndex = GetLastFormatIndex();

            zQuery.AddPullDownBox("Format", AllowedImageFormatNames, nDefaultFormatIndex, ExportOptionKey.Format);
            zQuery.AddTextBox("File Name Format (optional)", sDefinition ?? string.Empty, false, ExportOptionKey.NameFormat);
            zQuery.AddFolderBrowseBox("Output Folder",
                Directory.Exists(ProjectManager.Instance.LoadedProject.lastExportPath) ? ProjectManager.Instance.LoadedProject.lastExportPath : string.Empty,
                ExportOptionKey.Folder);

            zQuery.UpdateEnableStates();

            if (DialogResult.OK != zQuery.ShowDialog(CardMakerInstance.ApplicationForm))
            {
                return null;
            }
            var sFolder = zQuery.GetString(ExportOptionKey.Folder);
            SetupExportFolder(sFolder);

            if (!Directory.Exists(sFolder))
            {
                FormUtils.ShowErrorMessage("The folder specified does not exist!");
                return null;
            }

            ProjectManager.Instance.LoadedProject.lastExportPath = sFolder;
            var nLayoutIndex = ProjectManager.Instance.GetLayoutIndex(LayoutManager.Instance.ActiveLayout);
            if (-1 == nLayoutIndex)
            {
                FormUtils.ShowErrorMessage("Unable to determine the current layout. Please select a layout in the tree view and try again.");
                return null;
            }

            CardMakerSettings.IniManager.SetValue(IniSettings.LastImageExportFormat, AllowedImageFormatNames[zQuery.GetIndex(ExportOptionKey.Format)]);

            return new FileCardSingleExporter(nLayoutIndex, nLayoutIndex + 1, sFolder, zQuery.GetString(ExportOptionKey.NameFormat),
                AllowedImageFormats[zQuery.GetIndex(ExportOptionKey.Format)], LayoutManager.Instance.ActiveDeck.CardIndex);
        }

        public static CardExportBase BuildImageClipboardExporter()
        {
            var nLayoutIndex = ProjectManager.Instance.GetLayoutIndex(LayoutManager.Instance.ActiveLayout);
            if (-1 == nLayoutIndex)
            {
                FormUtils.ShowErrorMessage("Unable to determine the current layout. Please select a layout in the tree view and try again.");
                return null;
            }
            return new FileCardClipboardExporter(nLayoutIndex, nLayoutIndex + 1, LayoutManager.Instance.ActiveDeck.CardIndex);
        }

        private static int GetLastFormatIndex()
        {
            var lastImageFormat = CardMakerSettings.IniManager.GetValue(IniSettings.LastImageExportFormat, string.Empty);

            if (lastImageFormat != string.Empty)
            {
                for (var nIdx = 0; nIdx < AllowedImageFormatNames.Length; nIdx++)
                {
                    if (AllowedImageFormatNames[nIdx].Equals(lastImageFormat))
                    {
                        return nIdx;
                    }
                }
            }
            return 0;
        }

        private static void SetupExportFolder(string sFolder)
        {
            if (Directory.Exists(sFolder))
            {
                return;
            }
            try
            {
                Directory.CreateDirectory(sFolder);
            }
            catch (Exception e)
            {
                Logger.AddLogLine("Error creating folder {0}: {1}".FormatString(sFolder, e.Message));
            }
        }
    }
}
