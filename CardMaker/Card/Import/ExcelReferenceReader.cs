////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2019 Tim Stair
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
using CardMaker.Events.Managers;
using CardMaker.XML;

namespace CardMaker.Card.Import
{
    class ExcelReferenceReader : ReferenceReader
    {
        // SheetName to Variables
        private readonly Dictionary<string, List<List<string>>> m_dictionaryDataCache = new Dictionary<string, List<List<string>>>();

        public string ReferencePath { get; }

        public ExcelReferenceReader() { /* Intentionally do nothing */ }

        public ExcelReferenceReader(ProjectLayoutReference zReference)
        {
            // Taken from CSV reader
            var previousCurrentDirectory = Directory.GetCurrentDirectory();

            // TODO: May need to parse the reference properly first, and store that information
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

        public void FinalizeReferenceLoad() { }

        public void GetData(ExcelSpreadsheetReference zReference, List<List<string>> listData, bool bRemoveFirstRow)
        {

        }

        public void GetDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            throw new NotImplementedException();
        }

        public void GetProjectDefineData(ProjectLayoutReference zReference, List<List<string>> listDefineData)
        {
            throw new NotImplementedException();
        }

        public void GetReferenceData(ProjectLayoutReference zReference, List<List<string>> listReferenceData)
        {
            throw new NotImplementedException();
        }
    }
}
