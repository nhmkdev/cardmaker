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
using System.Xml.Serialization;

namespace CardMaker.XML
{
    public class ProjectLayout
    {
        public static readonly string[] AllowedExportRotations = { "0", "90", "-90" };
        
        #region Properties

        [XmlElement("Element")]
        public ProjectLayoutElement[] Element { get; set; }

        [XmlElement("Reference")]
        public ProjectLayoutReference[] Reference { get; set; }

        public string exportNameFormat { get; set; }

        public int exportRotation { get; set; }

        public bool exportTransparentBackground { get; set; }

        public bool exportPDFAsPageBack { get; set; }

        public int exportWidth { get; set; }
        
        public int exportHeight { get; set; }

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

        /// <summary>
        /// Performs a partial deepy copy based on the input element, the name field is left unchanged
        /// </summary>
        /// <param name="zLayout">The layout to copy from</param>
        /// <param name="bCopyRefs">Flag indicating whether to copy the refereces</param>
        public void DeepCopy(ProjectLayout zLayout, bool bCopyRefs = true)
        {
            width = zLayout.width;
            height = zLayout.height;
            defaultCount = zLayout.defaultCount;
            dpi = zLayout.dpi;
            drawBorder = zLayout.drawBorder;
            buffer = zLayout.buffer;
            combineReferences = zLayout.combineReferences;
            exportNameFormat = zLayout.exportNameFormat;
            exportRotation = zLayout.exportRotation;
            exportWidth = zLayout.exportWidth;
            exportHeight = zLayout.exportHeight;
            exportTransparentBackground = zLayout.exportTransparentBackground;
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
        }

    }
}