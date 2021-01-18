////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2021 Tim Stair
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
using Support.Google.Sheets;

namespace CardMaker.Card.Import
{
    public class GoogleSpreadsheetReference
    {
        private enum GoogleSpreadsheetReferenceIndex : int
        {
            GooglePrefix = 0,
            SpreadsheetName = 1,
            SheetName = 2,
            SpreadsheetId = 3
        }

        private enum GoogleSpreadsheetOnlyReferenceIndex : int
        {
            SpreadsheetName = 0,
            SpreadsheetId = 1
        }

        public const string GOOGLE_REFERENCE = "google";
        public const char GOOGLE_REFERENCE_SPLIT_CHAR = ';';

        public string SpreadsheetName { get; set; }
        public string SheetName { get; set; }
        public string SpreadsheetId { get; set; }

        public GoogleSpreadsheetReference() { }

        public GoogleSpreadsheetReference(GoogleSheetInfo zSheetInfo)
        {
            SpreadsheetName = zSheetInfo.Name;
            SpreadsheetId = zSheetInfo.Id;
        }

        public static GoogleSpreadsheetReference parseSpreadsheetOnlyReference(string sInput)
        {
            if (string.IsNullOrWhiteSpace(sInput))
                throw new Exception("Unable to read empty Google Spreadsheet reference.");

            var arrayComponents = sInput.Split(GOOGLE_REFERENCE_SPLIT_CHAR);
            switch (arrayComponents.Length)
            {
                // original style
                case 1:
                    return new GoogleSpreadsheetReference()
                    {
                        SpreadsheetName = arrayComponents[(int)GoogleSpreadsheetOnlyReferenceIndex.SpreadsheetName]
                    };
                // new style (with id)
                case 2:
                    return new GoogleSpreadsheetReference()
                    {
                        SpreadsheetName = arrayComponents[(int)GoogleSpreadsheetOnlyReferenceIndex.SpreadsheetName],
                        SpreadsheetId = arrayComponents[(int)GoogleSpreadsheetOnlyReferenceIndex.SpreadsheetId]
                    };
                default:
                    throw new Exception("Unable to read invalid Google Spreadsheet reference.");
            }
        }

        public static GoogleSpreadsheetReference parse(string sInput)
        {
            if(string.IsNullOrWhiteSpace(sInput))
                throw new Exception("Unable to read empty Google Spreadsheet reference.");

            var arrayComponents = sInput.Split(GOOGLE_REFERENCE_SPLIT_CHAR);
            switch (arrayComponents.Length)
            {
                // original style
                case 3:
                    return new GoogleSpreadsheetReference()
                    {
                        SpreadsheetName = arrayComponents[(int)GoogleSpreadsheetReferenceIndex.SpreadsheetName],
                        SheetName = arrayComponents[(int)GoogleSpreadsheetReferenceIndex.SheetName]
                    };
                // new style (with id)
                case 4:
                    return new GoogleSpreadsheetReference()
                    {
                        SpreadsheetName = arrayComponents[(int)GoogleSpreadsheetReferenceIndex.SpreadsheetName],
                        SheetName = arrayComponents[(int)GoogleSpreadsheetReferenceIndex.SheetName],
                        SpreadsheetId = arrayComponents[(int)GoogleSpreadsheetReferenceIndex.SpreadsheetId]
                    };
                default:
                    throw new Exception("Unable to read invalid Google Spreadsheet reference.");
            }
        }

        public string generateFullReference()
        {
            return generateFullReference(SpreadsheetName, SheetName, SpreadsheetId);
        }

        public string generateSpreadsheetReference()
        {
            return generateSpreadsheetReference(SpreadsheetName, SpreadsheetId);
        }

        public static string generateSpreadsheetReference(string sSpreadsheetName, string sSpreadSheetId = null)
        {
            return sSpreadsheetName
                   + GOOGLE_REFERENCE_SPLIT_CHAR
                   + sSpreadSheetId;
        }

        public static string generateFullReference(string sSpreadsheetName, string sSheetName, string sSpreadSheetId = null)
        {
            return GOOGLE_REFERENCE
                   + GOOGLE_REFERENCE_SPLIT_CHAR
                   + sSpreadsheetName
                   + GOOGLE_REFERENCE_SPLIT_CHAR
                   + sSheetName
                   + (sSpreadSheetId == null
                       ? ""
                       : GOOGLE_REFERENCE_SPLIT_CHAR + sSpreadSheetId)
                ;
        }
    }
}
