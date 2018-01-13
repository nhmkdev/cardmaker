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

namespace Support.IO
{
    public class CSVFile
    {
        // TODO: a lot more error checking!
        private readonly string m_sSourceFile = string.Empty;
        private readonly List<List<string>> m_listRows = new List<List<string>>();
        private readonly List<string> m_listRawText = new List<string>();

        public string Filename { get; }
        public bool DisplayFullPath { get; set; }

        private CSVFile() { }

        public CSVFile(string sFile, bool bKeepQuotes, bool bReadEmptyLines, Encoding eEncoding)
        {
            if (null == sFile)
                // create an empty collection
                return;

            if (!File.Exists(sFile))
            {
                throw new Exception("Specified file does not exist: " + sFile);
            }

            m_sSourceFile = Path.GetFullPath(sFile);
            Filename = Path.GetFileName(sFile);
            string[] arrayLines = File.ReadAllLines(m_sSourceFile, eEncoding);
            var bQuote = false;
            var zBuilder = new StringBuilder();
            var listColumns = new List<string>();
            foreach (string sLine in arrayLines)
            {
                if (0 == sLine.Trim().Length && !bReadEmptyLines)
                {
                    if (bQuote)
                    {
                        // add a newline for the empty line
                        zBuilder.Append("\\n");
                        m_listRawText.Add(sLine);
                    }
                    continue;
                }

                // if this line starts already quoted add a newline
                if (bQuote)
                {
                    zBuilder.Append("\\n");
                }

                m_listRawText.Add(sLine);

                var nIdx = 0;
                while(nIdx < sLine.Length)
                {
                    switch (sLine[nIdx])
                    {
                        case '\"':
                            if (bQuote)
                            {
                                if (bKeepQuotes)
                                {
                                    zBuilder.Append("\"");
                                }

                                listColumns.Add(zBuilder.ToString());
                                // no further characters should be added so the builder is null'd
                                zBuilder = null;
                                nIdx++;
                                // consume characters until a "," is found (nothing post close-quote is kept)
                                while (nIdx < sLine.Length)
                                {
                                    if (',' == sLine[nIdx])
                                    {
                                        zBuilder = new StringBuilder();
                                        break;
                                    }
                                    nIdx++;
                                }
                                bQuote = false;
                            }
                            else
                            {
                                // this throws out any previous text that came before the quote
                                zBuilder = new StringBuilder();
                                bQuote = true;
                                if (bKeepQuotes)
                                    zBuilder.Append("\"");
                            }
                            break;
                        case ',':
                            if (bQuote)
                            {
                                zBuilder.Append(sLine[nIdx]);
                            }
                            else
                            {
                                listColumns.Add(zBuilder.ToString());
                                zBuilder = new StringBuilder();
                            }
                            break;
                        default:
                            zBuilder.Append(sLine[nIdx]);
                            break;
                    }
                    nIdx++;
                }
                // support multiline quoted strings
#warning what about poorly formatted files (ie. no closing quote)
                if (!bQuote)
                {
                    if (null != zBuilder)
                    {
                        listColumns.Add(zBuilder.ToString()); // any end of line item should be added
                    }
                    m_listRows.Add(listColumns);
                    zBuilder = new StringBuilder();
                    listColumns = new List<string>();
                }
            }
        }

        /// <summary>
        /// Seeks the column index based on the specified string in the row indicated.
        /// </summary>
        /// <param name="nRow"></param>
        /// <param name="sItem"></param>
        /// <returns></returns>
        public int GetIndexFromRow(int nRow, string sItem)
        {
            List<string> listLine = GetRow(nRow);
            for (int nIdx = 0; nIdx < listLine.Count; nIdx++)
            {
                if (listLine[nIdx].Equals(sItem, StringComparison.CurrentCultureIgnoreCase))
                {
                    return nIdx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the actual row
        /// </summary>
        /// <param name="nRow"></param>
        /// <returns></returns>
        public List<string> GetRow(int nRow)
        {
            return m_listRows[nRow];
        }

        /// <summary>
        /// Returns the raw line from the CSV
        /// </summary>
        /// <param name="nLine"></param>
        /// <returns></returns>
        public string GetLine(int nLine)
        {
            return m_listRawText[nLine];
        }

        public string GetItem(int nRow, int nColumn)
        {
            return m_listRows[nRow][nColumn];
        }

        public void SetItem(int nRow, int nColumn, string sItem)
        {
            m_listRows[nRow][nColumn] = sItem;
        }

        public void Write(Encoding eEncoding)
        {
            Write(m_sSourceFile, eEncoding);
        }

        public bool Write(string sFile, Encoding eEncoding)
        {
            var bRet = true;
            var listOutputLines = new List<string>();
            foreach (var listLine in m_listRows)
            {
                listOutputLines.Add(string.Join(",", listLine.ToArray()));
            }
            try
            {
                File.WriteAllLines(sFile, listOutputLines.ToArray(), eEncoding);
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

        public void AddRow(string[] arrayElements)
        {
            AddRow(new List<string>(arrayElements));
        }

        public void AddRow(List<string> listElements)
        {
            m_listRows.Add(listElements);
        }

        public void AppendRow(int nRow, string sItem)
        {
            m_listRows[nRow].Add(sItem);
        }

        public int Rows => m_listRows.Count;

        public override string ToString()
        {
            if (DisplayFullPath)
            {
                return m_sSourceFile;
            }
            return Filename;
        }
    }
}