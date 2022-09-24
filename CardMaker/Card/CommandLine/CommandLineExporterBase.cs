////////////////////////////////////////////////////////////////////////////////
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

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using CardMaker.Card.Export;
using CardMaker.Data;
using CardMaker.Events.Managers;
using PdfSharp;
using Support.Util;

namespace CardMaker.Card.CommandLine
{
    public abstract class CommandLineExporterBase
    {
        public CommandLineParser CommandLineParser { get; set; }
        public CommandLineUtil CommandLineUtil { get; set; }
        protected string Description { get; set; }

        public abstract CardExportBase CreateExporter();

        /// <summary>
        /// Performs the export action
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        public bool Export()
        {
            var zFileCardExporter = CreateExporter();
            zFileCardExporter.ProgressReporter = CardMakerInstance.ProgressReporterFactory.CreateReporter(
                Description,
                new string[] { ProgressName.LAYOUT, ProgressName.REFERENCE_DATA, ProgressName.CARD },
                zFileCardExporter.ExportThread);
            try
            {
                zFileCardExporter.ProgressReporter.StartProcessing(null);
                return true;
            }
            catch (Exception e)
            {
                CommandLineUtil.ExitWithError("Export failed. " + e);
                return false;
            }
        }

        /// <summary>
        /// Gets the layout indices
        /// </summary>
        /// <returns>The layout indices specified on the command line (defaults to all)</returns>
        protected int[] GetLayoutIndices()
        {
            var arrayLayoutNames = CommandLineParser.GetStringArgs(CommandLineArg.LayoutNames);
            // get the indices of the layouts by name
            if (null != arrayLayoutNames)
            {
                var idx = 0;
                var dictionaryLayoutNameToLayout = 
                    ProjectManager.Instance.LoadedProject.Layout.ToDictionary(layout => layout.Name.ToUpper(), layout => idx++);
                var listMissingLayout = arrayLayoutNames
                    .Where(sLayoutName => !dictionaryLayoutNameToLayout.ContainsKey(sLayoutName.ToUpper())).ToList();
                if (listMissingLayout.Count > 0)
                {
                    CommandLineUtil.ExitWithError("Invalid layout names specified: " + string.Join(",", listMissingLayout.ToArray()));
                }
                return arrayLayoutNames
                    .Where(sLayoutName => dictionaryLayoutNameToLayout.ContainsKey(sLayoutName.ToUpper()))
                    .Select(sLayoutName => dictionaryLayoutNameToLayout[sLayoutName.ToUpper()]).ToArray();
            }

            // default to all layouts
            return Enumerable.Range(0, ProjectManager.Instance.LoadedProject.Layout.Length).ToArray();
        }

        /// <summary>
        /// Gets the card indices specified on the command line
        /// </summary>
        /// <returns>The indices specified on the command line (defaults to all)</returns>
        public int[] GetCardIndices()
        {
            var sCardIndices = CommandLineParser.GetStringArg(CommandLineArg.CardIndices);
            var listCardIndices = new List<int>();
            if (null != sCardIndices)
            {
                var arrayRanges = sCardIndices.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sRange in arrayRanges)
                {
                    var arrayEntries = sRange.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    int nStart;
                    int nEnd;
                    // NOTE: all of the inputs are cardId (1 based) -- converted to 0 based internally
                    switch (arrayEntries.Length)
                    {
                        case 1:
                            if (int.TryParse(arrayEntries[0], out nStart))
                            {
                                listCardIndices.Add(nStart - 1);
                            }
                            else
                            {
                                CommandLineUtil.ExitWithError("Invalid Card Index: " + sRange);
                            }
                            break;
                        case 2:
                            if (int.TryParse(arrayEntries[0], out nStart) && int.TryParse(arrayEntries[1], out nEnd))
                            {
                                listCardIndices.AddRange(Enumerable.Range(nStart - 1, (nEnd - nStart) + 1));
                            }
                            else
                            {
                                CommandLineUtil.ExitWithError("Invalid Card Index Range: " + sRange);
                            }
                            break;
                        default:
                            CommandLineUtil.ExitWithError("Invalid Card Index Range: " + sRange);
                            break;
                    }
                }

                return listCardIndices.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Loads the Google credentials from the command line
        /// </summary>
        public void ConfigureGoogleCredential()
        {
            var sGoogleCredential = CommandLineParser.GetStringArg(CommandLineArg.GoogleCredential);
            if (null != sGoogleCredential)
            {
                CardMakerInstance.GoogleAccessToken = sGoogleCredential;
                CardMakerInstance.GoogleCredentialsInvalid = false;
            }
        }

        /// <summary>
        /// Gets the export path (and deals with some windows quote hacks)
        /// </summary>
        /// <param name="bFolder">Flag indicating if the desired path is a folder</param>
        /// <returns>The path specified or null.</returns>
        protected string GetExportPath(bool bFolder)
        {
            var sRawPath = CommandLineParser.GetStringArg(CommandLineArg.ExportPath);
            if (!bFolder)
            {
                return sRawPath;
            }
            if (-1 != sRawPath.IndexOf("/") && !sRawPath.EndsWith("/"))
            {
                return sRawPath + "/";
            }
            if (-1 != sRawPath.IndexOf("\\"))
            {
                // this is a garbage fix thanks to the bizarre world of quotes
                if (sRawPath.EndsWith("\""))
                {
                    sRawPath = sRawPath.Substring(0, sRawPath.Length - 1);
                }
                if (!sRawPath.EndsWith("\\"))
                {
                    return sRawPath + "\\";
                }
            }

            return sRawPath;
        }

        /// <summary>
        /// Gets the page orientation from the command line (PDF specific)
        /// </summary>
        /// <returns>The page orientation (default: portrait)</returns>
        protected PageOrientation GetPageOrientation()
        {
            PageOrientation ePageOrientation;
            return PageOrientation.TryParse(CommandLineParser.GetStringArg(CommandLineArg.PageOrientation), true,
                out ePageOrientation)
                ? ePageOrientation
                : PageOrientation.Portrait;
        }

        /// <summary>
        /// Gets the image format from the command line
        /// </summary>
        /// <returns>The image format (default: png)</returns>
        protected ImageFormat GetImageFormat()
        {
            var sExportFormat = CommandLineParser.GetStringArg(CommandLineArg.ExportFormat);
            ImageFormat eImageFormat;
            return FileCardExporterFactory.AllowedImageFormatDictionary.TryGetValue(sExportFormat.ToUpper(), out eImageFormat)
                ? eImageFormat
                : ImageFormat.Png;
        }
    }
}
