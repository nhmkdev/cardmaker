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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CardMaker.Card.Shapes
{
    // NOTE: All member variables are only for editor use. The drawing parameters are passed via the argument array in the ShapeManager

    public abstract class AbstractShape
    {
        protected int m_nThickness = 0;
        protected string m_nOverrideWidth = NO_SIZE_OVERRIDE;
        protected string m_nOverrideHeight = NO_SIZE_OVERRIDE;
        protected string m_sName = string.Empty;

        public const string NO_SIZE_OVERRIDE = "-";

        public enum ShapeInformationIndex
        {
            Name,
            Thickness,
            OverrideWidth,
            OverrideHeight,
            BasicShapeInformation = OverrideHeight,
        }

        [BrowsableAttribute(false)]
        public string Name
        {
            get { return m_sName; }
        }

        [DescriptionAttribute("Border Thickness (0 is fill)")]
        public int Thickness
        {
            get { return m_nThickness; }
            set { m_nThickness = value; }
        }

        [DescriptionAttribute("Override Width (- is none)")]
        public string OverrideWidth
        {
            get { return m_nOverrideWidth; }
            set { m_nOverrideWidth = value; }
        }

        [DescriptionAttribute("Override Height (- is none)")]
        public string OverrideHeight
        {
            get { return m_nOverrideHeight; }
            set { m_nOverrideHeight = value; }
        }

        public override string ToString()
        {
            return m_sName;
        }

        public virtual string ToCardMakerString()
        {
            return "#" + m_sName + ";" + m_nThickness + ";" + m_nOverrideWidth + ";" + m_nOverrideHeight + "#";
        }

        protected string ToCardMakerString(string sExtendedVariables)
        {
            return "#" + m_sName + ";" + m_nThickness + ";" + m_nOverrideWidth + ";" + m_nOverrideHeight + ";" + sExtendedVariables + "#";
        }

        public virtual void InitializeItem(string sInput)
        {
            InitializeVariableArray(sInput);
        }

        protected string[] InitializeVariableArray(string sInput)
        {
            string[] arrayVariables = GetVariableArray(sInput);
            if (null != arrayVariables)
            {
                if ((int)ShapeInformationIndex.BasicShapeInformation <= arrayVariables.Length)
                {
                    bool bParse = true;
                    bParse &= int.TryParse(arrayVariables[(int)ShapeInformationIndex.Thickness], out m_nThickness);
                    m_nOverrideWidth = arrayVariables[(int)ShapeInformationIndex.OverrideWidth];
                    m_nOverrideHeight = arrayVariables[(int)ShapeInformationIndex.OverrideHeight];
                    if (bParse)
                        return arrayVariables;
                }
            } 
            return null;
        }

        public static string[] GetVariableArray(string sInput)
        {
            if (sInput.StartsWith("#") && sInput.EndsWith("#"))
            {
                sInput = sInput.Substring(1, sInput.Length - 2);
                return sInput.Split(new char[] { ';' });
            }
            return null;
        }

        public static string GetShapeType(string sInput)
        {
            string[] arrayVariables = GetVariableArray(sInput);
            if (null != arrayVariables && (int)ShapeInformationIndex.Name < arrayVariables.Length)
            {
                return arrayVariables[(int)ShapeInformationIndex.Name];
            }
            return null;
        }

        public abstract bool DrawShape(GraphicsPath zPath, Rectangle zTargetRect, ShapeInfo zInfo);
    }
}
