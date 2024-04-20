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
using System.Collections.Generic;
using System.IO;
using System.Text;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Card.Import
{
    public class CSVReferenceReader : ReferenceReader
    {
        private CSVSpreadsheetReference m_zSpreadsheetReference;

        public override ReferenceType ReferenceReaderType => ReferenceType.CSV;

        public CSVReferenceReader() { }

        public CSVReferenceReader(ProjectLayoutReference zReference)
        {
            m_zSpreadsheetReference = CSVSpreadsheetReference.Parse(zReference.RelativePath);
        }

        private List<ReferenceLine> GetData(string sPath, bool bLogNotFound, int nStartRow, string nameAppend = "")
        {
            CSVFile zCSVParser = null;
            var listReferenceLines = new List<ReferenceLine>();
            var bTryCopy = false;
            // TODO: should this be using ReferenceUtil.ConvertRelativeProjectPathToFullPath
            var sCombinedPath =
                Path.GetDirectoryName(sPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(sPath) + nameAppend + Path.GetExtension(sPath);

            if (ReferenceCache.TryGetCachedReference(sCombinedPath, out var listCachedReferenceLines))
            {
                return listCachedReferenceLines;
            }

            if (!File.Exists(sCombinedPath))
            {
                if (bLogNotFound)
                {
                    var sMsg = "CSV File not found: " + sCombinedPath;
                    ProgressReporter.AddIssue(sMsg);
                    IssueManager.Instance.FireAddIssueEvent(sMsg);
                }
                return listReferenceLines;
            }
            try
            {
                zCSVParser = new CSVFile(sCombinedPath, Encoding.UTF8);
            }
            catch (Exception)
            {
                bTryCopy = true;
            }

            // This is a ridiculous hack to work around excel locking the file from even being read
            if (bTryCopy)
            {
                var sTmpFile = sCombinedPath + "." + DateTime.Now.Millisecond + ".tmp";
                File.Copy(sCombinedPath, sTmpFile);
                zCSVParser = new CSVFile(sTmpFile, Encoding.UTF8);
                File.Delete(sTmpFile); // remove the temp file
            }

            for (var nRow = nStartRow; nRow < zCSVParser.Rows; nRow++)
            {
                listReferenceLines.Add(new ReferenceLine(zCSVParser.GetRow(nRow), sPath, nRow));
            }

            ReferenceCache.CacheReference(sCombinedPath, listReferenceLines);
            return listReferenceLines;
        }

        public override List<ReferenceLine> GetProjectDefineData()
        {
            if (string.IsNullOrEmpty(ProjectManager.Instance.ProjectFilePath))
            {
                return new List<ReferenceLine>();
            }

            var sReferencePath =
                Path.GetDirectoryName(ProjectManager.Instance.ProjectFilePath)
                + Path.DirectorySeparatorChar
                + Path.GetFileNameWithoutExtension(ProjectManager.Instance.ProjectFilePath)
                + ".csv";

            return GetData(sReferencePath, false, 1, Deck.DEFINES_DATA_SUFFIX);
        }

        public override List<ReferenceLine> GetDefineData()
        {
            return GetData(
                ReferenceUtil.ConvertRelativeProjectPathToFullPath(m_zSpreadsheetReference.RelativePath), 
                false, 
                1, 
                Deck.DEFINES_DATA_SUFFIX);
        }

        public override List<ReferenceLine> GetReferenceData()
        {
            return GetData(
                ReferenceUtil.ConvertRelativeProjectPathToFullPath(m_zSpreadsheetReference.RelativePath),
                true,
                0);
        }
    }
}
