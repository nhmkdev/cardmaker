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

using System.Drawing;
using CardMaker.Forms;
using Support.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CardMaker.XML
{
    public class Project
    {
        #region Properties

        // TODO: move this to a common place
        public static readonly Encoding XML_ENCODING = Encoding.UTF8;

        public static Color DEFAULT_REFERENCE_COLOR = Color.LightGreen;

        [XmlElementAttribute("Layout")]
        public ProjectLayout[] Layout { get; set; }

        public string lastExportPath { get; set; }

        public string exportNameFormat { get; set; }

        #endregion

        public static Project LoadProject(string sFile, TreeView tvMain)
        {
            Project zProject = null;
            // reset the collection of CardLayout objects
            if (File.Exists(sFile))
            {
                if (!SerializationUtils.DeserializeFromXmlFile(sFile, XML_ENCODING, ref zProject))
                {
                    Logger.AddLogLine("Failed to load project. Attempting upgrade from previous version.");
                    string sContents = File.ReadAllText(sFile);
                    // Fix the previous version's mistakes!
                    sContents = sContents.Replace("xmlns=\"http://tempuri.org/Project.xsd\"", string.Empty);
                    if (!SerializationUtils.DeserializeFromXmlString(sContents, XML_ENCODING, ref zProject))
                    {
                        Logger.AddLogLine("Failed to load project. The project file appears to be corrupt.");
                    }
                    else
                    {
                        Logger.AddLogLine("This project file is in an older format. Please save it using this version.");
                    }

                }
            }
            else
            {
                Logger.AddLogLine("No existing file specified. Loading defaults...");
                zProject = new Project
                {
                    Layout = new ProjectLayout[] {new ProjectLayout("Default")}
                };
            }

            if (null != tvMain)
            {
                //TODO: this code should be elsewhere ?
                tvMain.Nodes.Clear();
                var tnRoot = new TreeNode("Layouts")
                {
                    Tag = zProject
                };
                foreach (var zLayout in zProject.Layout)
                {
                    // no need to update the project
                    AddProjectLayout(tnRoot, zLayout, null);

                    InitializeElementCache(zLayout);
                }
                tvMain.Nodes.Add(tnRoot);
                tnRoot.ExpandAll();
            }
            return zProject;
        }

        public static void InitializeElementCache(ProjectLayout zLayout)
        {
            // mark all fields as specified
            if (null != zLayout.Element)
            {
                foreach (var zElement in zLayout.Element)
                {
                    zElement.InitializeCache();
                }
            }            
        }

        /// <summary>
        /// Adds a project layout tree node
        /// </summary>
        /// <param name="tnRoot"></param>
        /// <param name="zLayout"></param>
        /// <param name="zProject"></param>
        /// <returns></returns>
        public static TreeNode AddProjectLayout(TreeNode tnRoot, ProjectLayout zLayout, Project zProject)
        {
            TreeNode tnLayout = tnRoot.Nodes.Add(zLayout.Name);
            tnLayout.Tag = zLayout;

            if (null != zProject)
            {
                // update the Project (no null check on zProject.Layout necessary... can never have 0 layouts)
                var listLayouts = new List<ProjectLayout>(zProject.Layout);
                listLayouts.Add(zLayout);
                zProject.Layout = listLayouts.ToArray();
            }

            if (null != zLayout.Reference)
            {
                foreach (ProjectLayoutReference zReference in zLayout.Reference)
                {
                    // no need to update the layout
                    AddReferenceNode(tnLayout, zReference, null);
                }
                tnLayout.Expand();
            }

            return tnLayout;
        }

        public void RemoveProjectLayout(TreeNode tnLayout)
        {
            var zLayout = (ProjectLayout) tnLayout.Tag;

            // no need to null check, 1 layout must always exist
            var listLayouts = new List<ProjectLayout>(Layout);
            listLayouts.Remove(zLayout);
            Layout = listLayouts.ToArray();

            tnLayout.Parent.Nodes.Remove(tnLayout);
        }

        /// <summary>
        /// UI facing method for adding a reference node (for use from the context menu to add a new reference)
        /// </summary>
        /// <param name="tnLayout"></param>
        /// <param name="sFile"></param>
        /// <param name="bSetAsDefault"></param>
        /// <param name="zLayout"></param>
        /// <returns>The new Reference tree node or null if there is an existing reference by the same definition</returns>
        public static TreeNode AddReferenceNode(TreeNode tnLayout, string sFile, bool bSetAsDefault,
            ProjectLayout zLayout)
        {
            var sProjectPath = CardMakerMDI.ProjectPath;
            var zReference = new ProjectLayoutReference
            {
                Default = bSetAsDefault,
                RelativePath = IOUtils.GetRelativePath(sProjectPath,
                    sFile)
            };
            return AddReferenceNode(tnLayout, zReference, zLayout);
        }

        /// <summary>
        /// Internal/Project load handling for adding a reference node.
        /// </summary>
        /// <param name="tnLayout"></param>
        /// <param name="zReference"></param>
        /// <param name="zLayout">The layout to update the references for (may be null if no update is needed - ie. project loading)</param>
        /// <returns></returns>
        protected static TreeNode AddReferenceNode(TreeNode tnLayout, ProjectLayoutReference zReference,
            ProjectLayout zLayout)
        {
            var sProjectPath = CardMakerMDI.ProjectPath;
            var sFullReferencePath = zReference.RelativePath;
            if (!string.IsNullOrEmpty(sProjectPath))
            {
                sFullReferencePath = sProjectPath + Path.DirectorySeparatorChar + zReference.RelativePath;
            }

            if (zLayout != null && zLayout.Reference != null)
            {
                // duplicate check
                foreach (var zExistingReference in zLayout.Reference)
                {
                    if (zExistingReference.RelativePath.Equals(zReference.RelativePath,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        return null;
                    }
                }
            }

            var tnReference = new TreeNode(Path.GetFileName(sFullReferencePath))
            {
                BackColor = zReference.Default ? DEFAULT_REFERENCE_COLOR : Color.White,
                ToolTipText = zReference.RelativePath,
                Tag = zReference
            };
            tnLayout.Nodes.Add(tnReference);

            if (null != zLayout)
            {
                // update the ProjectLayout
                var listReferences = new List<ProjectLayoutReference>();
                if (null != zLayout.Reference)
                {
                    listReferences.AddRange(zLayout.Reference);
                }
                listReferences.Add(zReference);
                zLayout.Reference = listReferences.ToArray();
            }

            return tnReference;
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

            return SerializationUtils.SerializeToXmlFile(sFile, this, XML_ENCODING);
        }

    }
}