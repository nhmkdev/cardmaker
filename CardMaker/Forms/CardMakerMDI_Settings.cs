////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2015 Tim Stair
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
using System.Globalization;

namespace CardMaker.Forms
{
    public partial class CardMakerMDI
    {
        public string ProjectManagerRoot
        {
            get
            {
                return m_zIniManager.GetValue(IniSettings.ProjectManagerRoot, string.Empty);
            }
            set
            {
                m_zIniManager.SetValue(IniSettings.ProjectManagerRoot, value);
            }
        }

        public decimal PrintPageWidth
        {
            get
            {
                decimal dVal;
                if (decimal.TryParse(m_zIniManager.GetValue(IniSettings.PrintPageWidth, "8.5"), out dVal))
                {
                    return dVal;
                }
                return 8.5m;
            }
            set { m_zIniManager.SetValue(IniSettings.PrintPageWidth, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public decimal PrintPageHeight
        {
            get
            {
                decimal dVal;
                if (decimal.TryParse(m_zIniManager.GetValue(IniSettings.PrintPageHeight, "11"), out dVal))
                {
                    return dVal;
                }
                return 11m;
            }
            set { m_zIniManager.SetValue(IniSettings.PrintPageHeight, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public decimal PrintPageHorizontalMargin
        {
            get
            {
                decimal dVal;
                if (decimal.TryParse(m_zIniManager.GetValue(IniSettings.PrintPageHorizontalMargin, "0.5"), out dVal))
                {
                    return dVal;
                }
                return 0.5m;
            }
            set { m_zIniManager.SetValue(IniSettings.PrintPageHorizontalMargin, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public decimal PrintPageVerticalMargin
        {
            get
            {
                decimal dVal;
                if (decimal.TryParse(m_zIniManager.GetValue(IniSettings.PrintPageVerticalMargin, "0.25"), out dVal))
                {
                    return dVal;
                }
                return 0.25m;
            }
            set { m_zIniManager.SetValue(IniSettings.PrintPageVerticalMargin, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public bool PrintAutoHorizontalCenter
        {
            get { return m_zIniManager.GetValue(IniSettings.PrintAutoCenterLayout, bool.FalseString).Equals(bool.TrueString); }
            set { m_zIniManager.SetValue(IniSettings.PrintAutoCenterLayout, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public bool PrintLayoutBorder
        {
            get { return m_zIniManager.GetValue(IniSettings.PrintLayoutBorder, bool.TrueString).Equals(bool.TrueString); }
            set { m_zIniManager.SetValue(IniSettings.PrintLayoutBorder, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public int PrintDPI
        {
            get
            {
                int nVal;
                if (int.TryParse(m_zIniManager.GetValue(IniSettings.PrintDPI, "300"), out nVal))
                {
                    return nVal;
                }
                return 300;
            }
            set { m_zIniManager.SetValue(IniSettings.PrintDPI, value.ToString(CultureInfo.CurrentCulture)); }
        }

        public bool PrintScaled
        {
            get { return m_zIniManager.GetValue(IniSettings.PrintScaled, bool.FalseString).Equals(bool.TrueString); }
            set { m_zIniManager.SetValue(IniSettings.PrintScaled, value.ToString()); }
        }

        public PrintStyle PrintStyle
        {
            get
            {
                try
                {
                    return
                        (PrintStyle) Enum.Parse(typeof (PrintStyle),
                            m_zIniManager.GetValue(IniSettings.PrintStyle, PrintStyle.Direct.ToString()));
                }
                catch (Exception)
                {
                    return PrintStyle.Direct;
                }
            }
            set { m_zIniManager.SetValue(IniSettings.PrintStyle, value.ToString()); }
        }

        public bool PrintLayoutsOnNewPage
        {
            get { return m_zIniManager.GetValue(IniSettings.PrintLayoutsOnNewPage, bool.FalseString).Equals(bool.TrueString); }
            set { m_zIniManager.SetValue(IniSettings.PrintLayoutsOnNewPage, value.ToString()); }                
        }

    }
}
