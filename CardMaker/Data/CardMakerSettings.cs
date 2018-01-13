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

using System.Globalization;
using CardMaker.Events.Managers;
using Support.IO;
using Support.Util;

namespace CardMaker.Data
{
    /// <summary>
    /// Settings management for CardMaker
    /// </summary>
    class CardMakerSettings
    {
        private static readonly IniManager s_zIniManager;

        static CardMakerSettings()
        {
            s_zIniManager = new IniManager("cardmaker", false, true, true);
        }

        public static IniManager IniManager => s_zIniManager;

        public static string ProjectManagerRoot
        {
            get
            {
                return s_zIniManager.GetValue(IniSettings.ProjectManagerRoot, string.Empty);
            }
            set
            {
                s_zIniManager.SetValue(IniSettings.ProjectManagerRoot, value);
            }
        }

        public static MeasurementUnit PrintPageMeasurementUnit
        {
            get
            {
                MeasurementUnit eMeasurementUnit;
                MeasurementUnit.TryParse(
                    s_zIniManager.GetValue(IniSettings.PrintPageMeasurementUnit,
                        ((int) MeasurementUnit.Inch).ToString()), out eMeasurementUnit);
                return eMeasurementUnit;
            }
            set { s_zIniManager.SetValue(IniSettings.PrintPageMeasurementUnit, ((int)value).ToString()); }
        }

        public static decimal PrintPageWidth
        {
            get
            {
                decimal dVal;
                if (ParseUtil.ParseDecimal(s_zIniManager.GetValue(IniSettings.PrintPageWidth, "8.5"), out dVal))
                {
                    return dVal;
                }
                return 8.5m;
            }
            set { s_zIniManager.SetValue(IniSettings.PrintPageWidth, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public static decimal PrintPageHeight
        {
            get
            {
                decimal dVal;
                if (ParseUtil.ParseDecimal(s_zIniManager.GetValue(IniSettings.PrintPageHeight, "11"), out dVal))
                {
                    return dVal;
                }
                return 11m;
            }
            set { s_zIniManager.SetValue(IniSettings.PrintPageHeight, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public static decimal PrintPageHorizontalMargin
        {
            get
            {
                decimal dVal;
                if (ParseUtil.ParseDecimal(s_zIniManager.GetValue(IniSettings.PrintPageHorizontalMargin, "0.5"), out dVal))
                {
                    return dVal;
                }
                return 0.5m;
            }
            set { s_zIniManager.SetValue(IniSettings.PrintPageHorizontalMargin, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public static decimal PrintPageVerticalMargin
        {
            get
            {
                decimal dVal;
                if (ParseUtil.ParseDecimal(s_zIniManager.GetValue(IniSettings.PrintPageVerticalMargin, "0.25"), out dVal))
                {
                    return dVal;
                }
                return 0.25m;
            }
            set { s_zIniManager.SetValue(IniSettings.PrintPageVerticalMargin, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public static bool PrintAutoHorizontalCenter
        {
            get { return s_zIniManager.GetValue(IniSettings.PrintAutoCenterLayout, bool.FalseString).Equals(bool.TrueString); }
            set { s_zIniManager.SetValue(IniSettings.PrintAutoCenterLayout, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public static bool PrintLayoutBorder
        {
            get { return s_zIniManager.GetValue(IniSettings.PrintLayoutBorder, bool.TrueString).Equals(bool.TrueString); }
            set { s_zIniManager.SetValue(IniSettings.PrintLayoutBorder, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public static bool PrintLayoutsOnNewPage
        {
            get { return s_zIniManager.GetValue(IniSettings.PrintLayoutsOnNewPage, bool.FalseString).Equals(bool.TrueString); }
            set { s_zIniManager.SetValue(IniSettings.PrintLayoutsOnNewPage, value.ToString()); }
        }

        public static bool EnableGoogleCache
        {
            get { return s_zIniManager.GetValue(IniSettings.EnableGoogleCache, bool.FalseString).Equals(bool.TrueString); }
            set { s_zIniManager.SetValue(IniSettings.EnableGoogleCache, value.ToString()); }
        }

        public static TranslatorType DefaultTranslatorType
        {
            get
            {
                return ProjectManager.GetTranslatorTypeFromString(s_zIniManager.GetValue(IniSettings.DefaultTranslator,
                        TranslatorType.Incept.ToString()));
            }
            set { s_zIniManager.SetValue(IniSettings.DefaultTranslator, value.ToString()); }
        }

        public static int ExportStitchSkipIndex
        {
            get
            {
                int nValue;
                if(int.TryParse(s_zIniManager.GetValue(IniSettings.ExportSkipStitchIndex, "0"), out nValue))
                {
                    return nValue;
                }
                return 0;
            }
            set { s_zIniManager.SetValue(IniSettings.ExportSkipStitchIndex, value.ToString()); }
        }

        public static bool DefineTranslatePrimitiveCharacters
        {
            get { return s_zIniManager.GetValue(IniSettings.DefineTranslatePrimitiveCharacters, bool.FalseString).Equals(bool.TrueString); }
            set { s_zIniManager.SetValue(IniSettings.DefineTranslatePrimitiveCharacters, value.ToString()); }
        }

        public static bool LogInceptTranslation
        {
            get { return s_zIniManager.GetValue(IniSettings.LogInceptTranslation, bool.FalseString).Equals(bool.TrueString); }
            set { s_zIniManager.SetValue(IniSettings.LogInceptTranslation, value.ToString()); }
        }
    }
}
