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
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;
using Support.IO;

namespace CardMaker.XML
{
    public class ProjectLayout
    {
        public static readonly string[] AllowedExportRotations = { "0", "90", "-90", "180" };
        
        private Dictionary<string, ProjectLayoutElement> m_dictionaryElements = new Dictionary<string, ProjectLayoutElement>();

        #region Properties

        [XmlElement("Element")]
        public ProjectLayoutElement[] Element { get; set; }

        [XmlElement("Reference")]
        public ProjectLayoutReference[] Reference { get; set; }

        public string exportNameFormat { get; set; }

        public int exportRotation { get; set; }

        public bool exportTransparentBackground { get; set; }

        public string exportBackgroundColor { get; set; }

        public bool exportPDFAsPageBack { get; set; }

        public int exportWidth { get; set; }
        
        public int exportHeight { get; set; }

        public float zoom { get; set; } = 1;

        public bool exportLayoutBorder { get; set; }

        public int exportLayoutBorderCrossSize { get; set; }

        public string exportCropDefinition { get; set; }

        [XmlAttribute]
        public bool combineReferences { get; set; }

        [XmlAttribute]
        public int width { get; set; }

        [XmlAttribute]
        public int height { get; set; }

        [XmlAttribute]
        public int buffer { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int defaultCount { get; set; }

        [XmlAttribute]
        public int dpi { get; set; }

        [XmlAttribute]
        public bool drawBorder { get; set; }

        #endregion

        public Color GetExportBackgroundColor()
        {
            var zColor = ColorSerialization.TranslateColorString(exportBackgroundColor, 255, out var bSucceeded);
            return bSucceeded ? zColor : Color.White;
        }

        public Rectangle getExportCropDefinition()
        {
            if(string.IsNullOrWhiteSpace(exportCropDefinition))
                return Rectangle.Empty;
            var entries = exportCropDefinition.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (entries.Length == 4)
            {
                int x, y, width, height;
                if (int.TryParse(entries[0], out x)
                    && int.TryParse(entries[1], out y)
                    && int.TryParse(entries[2], out width)
                    && int.TryParse(entries[3], out height))
                {
                    return new Rectangle(x, y, width, height);
                }
            }
            return Rectangle.Empty;
        }

        public ProjectLayout()
        {
        }

        public ProjectLayout(string sName)
        {
            width = 200;
            height = 200;
            Name = sName;
            defaultCount = 1;
            dpi = 100;
            drawBorder = true;
        }

        public ProjectLayoutElement LookupElement(string sElementName)
        {
            if (string.IsNullOrWhiteSpace(sElementName))
            {
                return null;
            }

            // Maintaining this cache via events and otherwise is extremely tedious and error-prone. 
            // Force a refresh on any misses (which should be generally rare, with a few exceptions).
            if (!m_dictionaryElements.ContainsKey(sElementName))
            {
                InitializeElementLookup();
            }
            return m_dictionaryElements.ContainsKey(sElementName) 
                ? m_dictionaryElements[sElementName]
                : null;
        }

        /// <summary>
        /// Performs a partial deep copy based on the input element, the name field is left unchanged
        /// </summary>
        /// <param name="zLayout">The layout to copy from</param>
        /// <param name="bCopyRefs">Flag indicating whether to copy the references</param>
        public void DeepCopy(ProjectLayout zLayout, bool bCopyRefs = true)
        {
            width = zLayout.width;
            height = zLayout.height;
            defaultCount = zLayout.defaultCount;
            dpi = zLayout.dpi;
            drawBorder = zLayout.drawBorder;
            buffer = zLayout.buffer;
            zoom = zLayout.zoom;
            exportCropDefinition = zLayout.exportCropDefinition;
            combineReferences = zLayout.combineReferences;
            exportNameFormat = zLayout.exportNameFormat;
            exportRotation = zLayout.exportRotation;
            exportWidth = zLayout.exportWidth;
            exportHeight = zLayout.exportHeight;
            exportTransparentBackground = zLayout.exportTransparentBackground;
            exportPDFAsPageBack = zLayout.exportPDFAsPageBack;
            exportLayoutBorder = zLayout.exportLayoutBorder;
            exportLayoutBorderCrossSize = zLayout.exportLayoutBorderCrossSize;

            if (null != zLayout.Element)
            {
                var listElements = new List<ProjectLayoutElement>();
                foreach (ProjectLayoutElement zElement in zLayout.Element)
                {
                    var zElementCopy = new ProjectLayoutElement(zElement.name);
                    zElementCopy.DeepCopy(zElement, true);
                    listElements.Add(zElementCopy);
                }
                Element = listElements.ToArray();
            }
            if (bCopyRefs && null != zLayout.Reference)
            {
                var listReferences = new List<ProjectLayoutReference>();
                foreach (var zReference in zLayout.Reference)
                {
                    var zReferenceCopy = new ProjectLayoutReference();
                    zReferenceCopy.DeepCopy(zReference);
                    listReferences.Add(zReferenceCopy);
                }
                Reference = listReferences.ToArray();
            }

            InitializeElementLookup();
        }

        public void InitializeElementLookup()
        {
            m_dictionaryElements.Clear();
            if (null != Element)
            {
                foreach (var zElement in Element)
                {
                    m_dictionaryElements[zElement.name] = zElement;
                }
            }
        }

    }
}