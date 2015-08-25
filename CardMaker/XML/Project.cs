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

using CardMaker.Forms;
using Support.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CardMaker.XML
{
    public class Project
    {
        #region Properties

        [XmlElementAttribute("Layout")]
        public ProjectLayout[] Layout { get; set; }

        public string lastExportPath { get; set; }

        public string exportNameFormat { get; set; }

        #endregion

        public void RemoveProjectLayout(TreeNode tnLayout)
        {
            var zLayout = (ProjectLayout) tnLayout.Tag;

            // no need to null check, 1 layout must always exist
            var listLayouts = new List<ProjectLayout>(Layout);
            listLayouts.Remove(zLayout);
            Layout = listLayouts.ToArray();

            tnLayout.Parent.Nodes.Remove(tnLayout);
        }

        public bool HasExternalReference()
        {
            foreach (var zLayout in Layout)
            {
                if (null == zLayout.Reference)
                    continue;
                foreach (var zReference in zLayout.Reference)
                {
                    if (zReference.RelativePath.StartsWith(CardMakerMDI.GOOGLE_REFERENCE))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Save(string sFile, string sOldFile)
        {
            string sProjectPath = Path.GetDirectoryName(sFile);
            string sOldProjectPath = Path.GetDirectoryName(sOldFile);

            bool bOldPathValid = !string.IsNullOrEmpty(sOldProjectPath);

            if (sProjectPath != null &&
                !sProjectPath.Equals(sOldProjectPath, StringComparison.CurrentCultureIgnoreCase))
            {
                // change the relative paths for the references
                foreach (var zLayout in Layout)
                {
                    if (null != zLayout.Reference)
                    {
                        foreach (ProjectLayoutReference zReference in zLayout.Reference)
                        {
                            zReference.RelativePath = bOldPathValid
                                ? IOUtils.UpdateRelativePath(sOldProjectPath, zReference.RelativePath, sProjectPath)
                                : zReference.RelativePath =
                                    IOUtils.GetRelativePath(sProjectPath, zReference.RelativePath);
                        }
                    }
                }
            }

            return SerializationUtils.SerializeToXmlFile(sFile, this, Core.XML_ENCODING);
        }
    }
}