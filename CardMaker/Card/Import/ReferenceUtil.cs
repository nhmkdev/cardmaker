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

using CardMaker.XML;
using Support.IO;
using Support.UI;
using System.IO;
using CardMaker.Events.Managers;

namespace CardMaker.Card.Import
{
    public static class ReferenceUtil
    {
        /// <summary>
        /// Updates the relative path in the reference based on the provided inputs
        /// </summary>
        /// <param name="zReference">Reference to update the relative path in</param>
        /// <param name="sNewRootPath">The new root path</param>
        /// <param name="sOldRootPath">The old root path</param>
        /// <returns></returns>
        public static ProjectLayoutReference UpdateReferenceRelativePath(ProjectLayoutReference zReference, string sNewRootPath, string sOldRootPath)
        {
            if (zReference == null)
            {
                return null;
            }
            var zSpreadsheetReference = GetSpreadsheetReference(zReference);
            if (null == zSpreadsheetReference)
            {
                Logger.AddLogLine(
                    "Failed to determine reference type from: {0}".FormatString(zReference.RelativePath));
                return zReference;
            }

            if (!zSpreadsheetReference.IsLocalFile)
            {
                // nothing to change
                return zReference;
            }

            zSpreadsheetReference.RelativePath = !string.IsNullOrEmpty(sOldRootPath)
                ? IOUtils.UpdateRelativePath(sOldRootPath, zSpreadsheetReference.RelativePath, sNewRootPath)
                : zReference.RelativePath =
                    IOUtils.GetRelativePath(sNewRootPath, zSpreadsheetReference.RelativePath);

            // Note: ProjectLayoutReference RelativePath is an overloaded string
            zReference.RelativePath = zSpreadsheetReference.SerializeToReferenceString();
            return zReference;
        }

        /// <summary>
        /// Converts a relative project path to the full path (based on the loaded project)
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static string ConvertRelativeProjectPathToFullPath(string sPath)
        {
            string sFullPath;
            if (null != ProjectManager.Instance.ProjectPath && Directory.Exists(ProjectManager.Instance.ProjectPath))
            {
                sFullPath = File.Exists(sPath)
                    ? Path.GetFullPath(sPath)
                    : Path.Combine(ProjectManager.Instance.ProjectPath, sPath);
            }
            else
            {
                sFullPath = File.Exists(sPath) ? Path.GetFullPath(sPath) : sPath;
            }
            return sFullPath;
        }

#warning TODO: create a mapping from reference prefix -> SpreadsheetReferenceBase implementations (call constructor)
        private static SpreadsheetReferenceBase GetSpreadsheetReference(ProjectLayoutReference zReference)
        {
            if (zReference == null)
            {
                return null;
            }
            SpreadsheetReferenceBase zReferenceReader = null;
            if (zReference.RelativePath.StartsWith(GoogleSpreadsheetReference.GOOGLE_REFERENCE +
                                                   GoogleSpreadsheetReference.GOOGLE_REFERENCE_SPLIT_CHAR))
            {
                zReferenceReader = GoogleSpreadsheetReference.Parse(zReference.RelativePath);
            }
            if (zReference.RelativePath.StartsWith(ExcelSpreadsheetReference.EXCEL_REFERENCE +
                                                   ExcelSpreadsheetReference.EXCEL_REFERENCE_SPLIT_CHAR))
            {
                zReferenceReader = ExcelSpreadsheetReference.Parse(zReference.RelativePath);
            }
            return zReferenceReader ?? CSVSpreadsheetReference.Parse(zReference.RelativePath);
        }
    }
}
