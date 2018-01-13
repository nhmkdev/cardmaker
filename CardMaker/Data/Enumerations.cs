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


using System.Collections.Generic;

namespace CardMaker.Data
{
    public static class EnumUtil
    {
        private static readonly Dictionary<string, ElementType> s_dictionaryStringElementType = new Dictionary<string, ElementType>();

        static EnumUtil()
        {
            for (var nIdx = 0; nIdx < (int)ElementType.End; nIdx++)
            {
                s_dictionaryStringElementType.Add(((ElementType)nIdx).ToString(), (ElementType)nIdx);
            }
        }

        public static ElementType GetElementType(string sType)
        {
            ElementType eType;
            if (s_dictionaryStringElementType.TryGetValue(sType, out eType))
            {
                return eType;
            }
            return ElementType.End;
        }
    }

    public enum ElementType
    {
        Text,
        FormattedText,
        Graphic,
        Shape,
        End
    }

    public enum MeasurementUnit
    {
        Inch,
        Millimeter,
        Centimeter
    }

    public enum IniSettings
    {
        PreviousProjects,
        ReplacementChars,
        ProjectManagerRoot,
        PrintPageMeasurementUnit,
        PrintPageWidth,
        PrintPageHeight,
        PrintPageVerticalMargin,
        PrintPageHorizontalMargin,
        PrintAutoCenterLayout,
        PrintLayoutBorder,
        LastImageExportFormat,
        PrintLayoutsOnNewPage,
        EnableGoogleCache,
        DefaultTranslator,
        ExportSkipStitchIndex,
        DefineTranslatePrimitiveCharacters,
        LogInceptTranslation
    }

    public enum ExportType
    {
        PDFSharp,
        Image
    }

    public enum TranslatorType
    {
        Incept = 0,
        JavaScript
    }

    public enum ReferenceType
    {
        CSV = 0,
        Google = 1
    }
}
