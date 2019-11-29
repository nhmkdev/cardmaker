using System;

namespace CardMaker.Card.Import
{
    class ExcelSpreadsheetReference
    {
        public const string EXCEL_REFERENCE = "excel";
        public const char EXCEL_REFERENCE_SPLIT_CHAR = ';';

        private enum ExcelSpreadsheetOnlyReferenceIndex : int
        {
            SpreadsheetFile = 0,
            SheetName = 1
        }

        public string SpreadsheetFile { get; set; }
        public string SheetName { get; set; }

        public ExcelSpreadsheetReference() { }
        public ExcelSpreadsheetReference(string sFilename, string sSheet)
        {
            SpreadsheetFile = sFilename;
            SheetName = sSheet;
        }

        public static ExcelSpreadsheetReference parse(string sInput)
        {
            if(string.IsNullOrWhiteSpace(sInput))
                throw new Exception("Unable to read empty Excel Spreadsheet reference.");

            var arrayComponents = sInput.Split(EXCEL_REFERENCE_SPLIT_CHAR);
            if (arrayComponents.Length == 3)
            {
                return new ExcelSpreadsheetReference(arrayComponents[(int)ExcelSpreadsheetOnlyReferenceIndex.SpreadsheetFile + 1],
                    arrayComponents[(int)ExcelSpreadsheetOnlyReferenceIndex.SheetName + 1]);
            }

            throw new Exception("Unable to read invalid Excel Spreadsheet reference.");
        }

        public static string generateFullReference(string sSpreadsheetFile, string sSheetName)
        {
            return EXCEL_REFERENCE
                   + EXCEL_REFERENCE_SPLIT_CHAR
                   + sSpreadsheetFile
                   + EXCEL_REFERENCE_SPLIT_CHAR
                   + sSheetName;
        }
    }
}
