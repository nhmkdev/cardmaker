﻿////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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
using System.IO;
using CardMaker.Data;
using CardMaker.Events.Args;
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Events.Managers
{
    /// <summary>
    /// Handles general Project related communication between components
    /// </summary>
    public class ProjectManager
    {
        private static ProjectManager m_zInstance;

        public Project LoadedProject { get; private set; }

        public TranslatorType LoadedProjectTranslatorType { get; private set; }

        public ReferenceType LoadedProjectDefaultDefineReferenceType { get; private set; }

        public string ProjectFilePath { get; private set; }

        public string ProjectPath { get; private set; }

        /// <summary>
        /// Fired when a project is opened
        /// </summary>
        public event ProjectOpened ProjectOpened;

        /// <summary>
        /// Fired when the project is changed (generally a high level Layout change)
        /// </summary>
        public event ProjectUpdated ProjectUpdated;

        /// <summary>
        /// Fired when a layout is added
        /// </summary>
        public event LayoutAdded LayoutAdded;

        /// <summary>
        /// Fired when a layout is renamed
        /// </summary>
        public event LayoutRenamed LayoutRenamed;

        /// <summary>
        /// Fired when an element is renamed
        /// </summary>
        public event ElementRenamed ElementRenamed;

        /// <summary>
        /// Fired when an element is added
        /// </summary>
        public event ElementsAdded ElementsAdded;

        /// <summary>
        /// Fired when an element is removed
        /// </summary>
        public event ElementsRemoved ElementsRemoved;

        public static ProjectManager Instance => m_zInstance ?? (m_zInstance = new ProjectManager());

        private Dictionary<string, ProjectLayout> m_zProjectLayoutDictionary = new Dictionary<string, ProjectLayout>();

        public ProjectManager()
        {
            ProjectUpdated += (sender, e) => UpdateSettings();
            LayoutRenamed += OnLayoutRenamed;
            ElementRenamed += OnElementRenamed;
            ElementsRemoved += OnElementsRemoved;
            ProjectOpened += (sender, e) => InitializeLookups(true);
        }

        #region Event Triggers

        /// <summary>
        /// Fires the ProjectUpdated event
        /// </summary>
        public void FireProjectUpdated(bool bDataChange)
        {
            ProjectUpdated?.Invoke(this, new ProjectEventArgs(LoadedProject, ProjectFilePath, bDataChange));
        }

        /// <summary>
        /// Fires the FireLayoutRenamed event
        /// </summary>
        public void FireLayoutRenamed(ProjectLayout zLayout, string sOldName)
        {
            LayoutRenamed?.Invoke(this, new LayoutRenamedEventArgs(zLayout, sOldName));
        }

        /// <summary>
        /// Fires the ElementRenamed event
        /// </summary>
        public void FireElementRenamed(ProjectLayoutElement zElement, string sOldName)
        {
            ElementRenamed?.Invoke(this, new ElementRenamedEventArgs(zElement, sOldName));
        }

        /// <summary>
        /// Fires the ElementsAdded event
        /// </summary>
        public void FireElementsAdded(List<ProjectLayoutElement> listAddedElements)
        {
            ElementsAdded?.Invoke(this, new ElementEventArgs(listAddedElements));
        }

        /// <summary>
        /// Fires the ElementsRemoved event
        /// </summary>
        public void FireElementsRemoved(List<ProjectLayoutElement> listRemovedElements)
        {
            ElementsRemoved?.Invoke(this, new ElementEventArgs(listRemovedElements));
        }

        #endregion

        #region Events

        private void OnLayoutRenamed(object sender, LayoutRenamedEventArgs e)
        {
            InitializeLookups(false);

            // update all nested reference elements to match the name
            if (LoadedProject == null) return;
            foreach (var zLayout in LoadedProject.Layout)
            {
                if (zLayout.Element == null) continue;
                foreach (var zElement in zLayout.Element)
                {
                    if (string.Equals(zElement.layoutreference, e.OldName))
                    {
                        zElement.layoutreference = e.Layout.Name;
                    }
                }
            }
        }

        private void OnElementRenamed(object sender, ElementRenamedEventArgs e)
        {
            // update all nested reference elements to match the name
            if (LoadedProject == null) return;
            foreach (var zLayout in LoadedProject.Layout)
            {
                if(zLayout.Element == null) continue;
                foreach (var zElement in zLayout.Element)
                {
                    if (string.Equals(zElement.elementreference, e.OldName))
                    {
                        zElement.elementreference = e.Element.name;
                        zElement.name = e.Element.name;
                    }
                }
            }
        }

        private void OnElementsRemoved(object sender, ElementEventArgs e)
        {
            // update all nested reference elements to detach (deep copy)
            if (LoadedProject == null) return;
            var sLayoutName = LayoutManager.Instance.ActiveDeck.CardLayout.Name;
            foreach (var zLayout in LoadedProject.Layout)
            {
                if (zLayout.Element == null) continue;
                foreach (var zElement in zLayout.Element)
                {
                    foreach (var zRemovedElement in e.Elements)
                    {
                        if (string.Equals(zElement.layoutreference, sLayoutName)
                            && string.Equals(zElement.elementreference, zRemovedElement.name))
                        {
                            zElement.DeepCopy(zRemovedElement, true);
                            Logger.AddLogLine("Detached reference: {0}:{1}".FormatString(sLayoutName, zRemovedElement.name));
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Loads the project file for use by ProjectManager event listeners
        /// </summary>
        /// <param name="sProjectFile"></param>
        /// <returns></returns>
        public void OpenProject(string sProjectFile)
        {
            LoadedProject = LoadProject(sProjectFile);
            UpdateSettings();
            SetLoadedProjectFile(sProjectFile);
            ProjectOpened?.Invoke(this, new ProjectEventArgs(LoadedProject, ProjectFilePath));
        }

        /// <summary>
        /// Performs a save of the current Project
        /// </summary>
        /// <param name="sFile">The path to save</param>
        /// <returns></returns>
        public bool Save(string sFile)
        {
            string sProjectPath = Path.GetDirectoryName(sFile);
            string sOldProjectPath = null == ProjectFilePath ? string.Empty : Path.GetDirectoryName(ProjectFilePath);

            bool bOldPathValid = !string.IsNullOrEmpty(sOldProjectPath);

            if (sProjectPath != null &&
                !sProjectPath.Equals(sOldProjectPath, StringComparison.CurrentCultureIgnoreCase))
            {
                // change the relative paths for the references
                foreach (var zLayout in LoadedProject.Layout)
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

            if (SerializationUtils.SerializeToXmlFile(sFile, LoadedProject, CardMakerConstants.XML_ENCODING))
            {
                SetLoadedProjectFile(sFile);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the specified layout to the project (new data)
        /// </summary>
        /// <param name="zLayout"></param>
        public void AddLayout(ProjectLayout zLayout)
        {
            // update the Project (no null check on zProject.Layout necessary... can never have 0 layouts)
            var listLayouts = new List<ProjectLayout>(LoadedProject.Layout) {zLayout};
            LoadedProject.Layout = listLayouts.ToArray();
            LayoutManager.InitializeElementCache(zLayout);
            LayoutAdded?.Invoke(this, new Args.LayoutEventArgs(zLayout, null));
            FireProjectUpdated(true);
        }

        /// <summary>
        /// Configures the instance variables related to the loaded project
        /// </summary>
        /// <param name="sProjectFile">The file path to the project</param>
        private void SetLoadedProjectFile(string sProjectFile)
        {
            ProjectFilePath = sProjectFile;
            ProjectPath = string.IsNullOrEmpty(ProjectFilePath) ? null : (Path.GetDirectoryName(sProjectFile) + Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Returns the layout index based on the active project
        /// </summary>
        /// <param name="zLayout"></param>
        /// <returns>The index, or -1 if not found</returns>
        public int GetLayoutIndex(ProjectLayout zLayout)
        {
            if (null != LoadedProject)
            {
                for (int nIdx = 0; nIdx < LoadedProject.Layout.Length; nIdx++)
                {
                    if (LoadedProject.Layout[nIdx] == zLayout)
                    {
                        return nIdx;
                    }
                }
            }
            return -1;
        }

        private void InitializeLookups(bool bInitializeElementLookup = true)
        {
            m_zProjectLayoutDictionary.Clear();
            if(LoadedProject == null) return;
            foreach (var zLayout in LoadedProject.Layout)
            {
                m_zProjectLayoutDictionary[zLayout.Name] = zLayout;
                if (bInitializeElementLookup)
                {
                    zLayout.InitializeElementLookup();
                }
            }
        }

        private void UpdateSettings()
        {
            LoadedProjectTranslatorType = GetTranslatorTypeFromString(LoadedProject.translatorName);
            LoadedProjectDefaultDefineReferenceType = GetReferenceTypeFromString(LoadedProject.defaultDefineReferenceType);
        }

        public ProjectLayoutElement LookupElementReference(ProjectLayoutElement zElement)
        {
            var zReferenceElement =
                LookupLayoutByName(zElement.layoutreference)?.LookupElement(zElement.elementreference);
            return null == zReferenceElement
                ? zElement
                : zReferenceElement;
        }

        public ProjectLayout LookupLayoutByName(string sLayoutName)
        {
            return null != sLayoutName && m_zProjectLayoutDictionary.ContainsKey(sLayoutName)
                ? m_zProjectLayoutDictionary[sLayoutName]
                : null;
        }

        public static TranslatorType GetTranslatorTypeFromString(string sInput)
        {
            TranslatorType eTranslatorType;
            if (!Enum.TryParse(sInput, true, out eTranslatorType))
            {
                return TranslatorType.Incept;
            }
            return eTranslatorType;
        }

        public static ReferenceType GetReferenceTypeFromString(string sInput)
        {
            ReferenceType eTranslatorType;
            if (!Enum.TryParse(sInput, true, out eTranslatorType))
            {
                return ReferenceType.CSV;
            }
            return eTranslatorType;
        }

        /// <summary>
        /// Opens a project file for static usage (no events are fired)
        /// </summary>
        /// <param name="sFile">The project file to load</param>
        /// <returns></returns>
        public static Project LoadProject(string sFile)
        {
            Project zProject = null;
            // reset the collection of CardLayout objects
            if (File.Exists(sFile))
            {
                if (!SerializationUtils.DeserializeFromXmlFile(sFile, CardMakerConstants.XML_ENCODING, ref zProject))
                {
                    Logger.AddLogLine("Failed to load project. Attempting upgrade from previous version.");
                    string sContents = File.ReadAllText(sFile);
                    // Fix the previous version's mistakes!
                    sContents = sContents.Replace("xmlns=\"http://tempuri.org/Project.xsd\"", string.Empty);
                    Logger.AddLogLine(!SerializationUtils.DeserializeFromXmlString(sContents, CardMakerConstants.XML_ENCODING, ref zProject)
                            ? "Failed to load project. The project file appears to be corrupt."
                            : "This project file is in an older format. Please save it using this version.");
                }
            }
            else
            {
                Logger.AddLogLine("No existing file specified. Loading defaults...");
                zProject = new Project
                {
                    translatorName = CardMakerSettings.DefaultTranslatorType.ToString(),
                    Layout = new ProjectLayout[] { new ProjectLayout("Default") }
                };
            }
            return zProject;
        }
    }
}
