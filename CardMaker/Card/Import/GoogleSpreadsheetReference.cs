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
using Support.Google.Sheets;
using Support.IO;
using Support.UI;

namespace CardMaker.Card.Import
{
    public class GoogleSpreadsheetReference : SpreadsheetReferenceBase
    {
        public const string GOOGLE_REFERENCE = "google";
        public const char GOOGLE_REFERENCE_SPLIT_CHAR = ';';

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

        public string SpreadsheetName { get; set; }
        public string SheetName { get; set; }
        public string SpreadsheetId { get; set; }

        public override string RelativePath
        {
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException();
        }
        public override bool IsLocalFile => false;

        public GoogleSpreadsheetReference() { }

        public GoogleSpreadsheetReference(GoogleSheetInfo zSheetInfo)
        {
            SpreadsheetName = zSheetInfo.Name;
            SpreadsheetId = zSheetInfo.Id;
        }

        public static GoogleSpreadsheetReference ParseSpreadsheetOnlyReference(string sInput)
        {
            if (string.IsNullOrWhiteSpace(sInput))
                throw new Exception("Unable to read empty Google Spreadsheet reference.");

            var arrayComponents = sInput.Split(GOOGLE_REFERENCE_SPLIT_CHAR);
            switch (arrayComponents.Length)
            {
                // original style
#warning this will never work again because sheets cannot be looked up by name
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

        public static GoogleSpreadsheetReference Parse(string sInput)
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

        public string GenerateFullReference()
        {
            return SerializeToReferenceString(SpreadsheetName, SheetName, SpreadsheetId);
        }

        public string GenerateSpreadsheetReference()
        {
            return SpreadsheetName
                   + GOOGLE_REFERENCE_SPLIT_CHAR
                   + SpreadsheetId;
        }

        public static string SerializeToReferenceString(string sSpreadsheetName, string sSheetName, string sSpreadSheetId = null)
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

        public override string SerializeToReferenceString()
        {
            return SerializeToReferenceString(SpreadsheetName, SheetName, SpreadsheetId);
        }

        public static string ExtractSpreadsheetIDFromURLString(string sUrl)
        {
            try
            {
                var zUri = new Uri(sUrl);
                var arrayPathComponents = zUri.LocalPath.Split(new char[] { '/' });
                return (arrayPathComponents.Length > 3) ? arrayPathComponents[3] : string.Empty;
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Failed to parse URL: {0} Error: {1}".FormatString(sUrl, ex.ToString()));
                return string.Empty;
            }
        }
    }
}
