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
using System.Collections.Generic;
using System.IO;
using System.Text;
using CardMaker.Events.Managers;
using CardMaker.XML;
using Support.IO;

namespace CardMaker.Card.Import
{
    public class CSVReferenceReader : ReferenceReader
    {
        public string ReferencePath { get; }

        public CSVReferenceReader() { }

        public CSVReferenceReader(ProjectLayoutReference zReference)
        {
            var previousCurrentDirectory = Directory.GetCurrentDirectory();

            if (null != ProjectManager.Instance.ProjectPath && Directory.Exists(ProjectManager.Instance.ProjectPath))
            {
                Directory.SetCurrentDirectory(ProjectManager.Instance.ProjectPath);
                ReferencePath = (File.Exists(zReference.RelativePath)
                                ? Path.GetFullPath(zReference.RelativePath)
                                : ProjectManager.Instance.ProjectPath + zReference.RelativePath);
            }
            else
            {
                ReferencePath = File.Exists(zReference.RelativePath) ? Path.GetFullPath(zReference.RelativePath) : zReference.RelativePath;
            }
            Directory.SetCurrentDirectory(previousCurrentDirectory);
        }

        public void GetData(string sPath, List<List<string>> listData, bool bLogNotFound, int nStartIdx, string nameAppend = "")
        {
            CSVFile zCSVParser = null;
            var bTryCopy = false;
            var sCombinedPath =
                Path.GetDirectoryName(sPath) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(sPath) + nameAppend + Path.GetExtension(sPath);
            if (!File.Exists(sCombinedPath))
            {
                if (bLogNotFound)
                {
                    var sMsg = "CSV File not found: " + sCombinedPath;
                    Logger.AddLogLine(sMsg);
                    IssueManager.Instance.FireAddIssueEvent(sMsg);
                }
                return;
            }
            try
            {
                zCSVParser = new CSVFile(sCombinedPath, false, false, Encoding.UTF8);
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
                zCSVParser = new CSVFile(sTmpFile, false, false, Encoding.UTF8);
                File.Delete(sTmpFile); // remove the temp file
            }

            for (var nLine = nStartIdx; nLine < zCSVParser.Rows; nLine++)
            {
                listData.Add(zCSVParser.GetRow(nLine));
            }
        }

        public void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData)
        {
            GetData(ReferencePath, listReferenceData, true, 0);
        }

        public void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            if (null == ProjectManager.Instance.ProjectFilePath)
            {
                return;
            }

            var sReferencePath =
                Path.GetDirectoryName(ProjectManager.Instance.ProjectFilePath)
                + Path.DirectorySeparatorChar
                + Path.GetFileNameWithoutExtension(ProjectManager.Instance.ProjectFilePath)
                + ".csv";

            GetData(sReferencePath, listDefineData, false, 1, Deck.DEFINES_DATA_POSTFIX);
        }

        public void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            GetData(ReferencePath, listDefineData, false, 1, Deck.DEFINES_DATA_POSTFIX);
        }

        public void FinalizeReferenceLoad()
        {

        }
    }
}
