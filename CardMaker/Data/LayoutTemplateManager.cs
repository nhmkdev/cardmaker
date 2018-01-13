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

using System;
using System.Collections.Generic;
using System.IO;
using CardMaker.XML;
using Support.IO;
using Support.UI;

namespace CardMaker.Data
{
    /// <summary>
    /// Handles LayoutTemplate management
    /// </summary>
    public class LayoutTemplateManager
    {
        private const string LAYOUT_TEMPLATES_FOLDER = "templates";
        private const string LAYOUT_TEMPLATE_EXTENSION = "xml";
        private const char NON_FILE_SAFE_REPLACEMENT_CHAR = '_';

        private static LayoutTemplateManager m_zInstance;

        public List<LayoutTemplate> LayoutTemplates { get; set; } 
        
        public static LayoutTemplateManager Instance => m_zInstance ?? (m_zInstance = new LayoutTemplateManager());

        /// <summary>
        /// Loads the template files from the specified startup path
        /// </summary>
        /// <param name="sStartupPath">The path containing the templates folder</param>
        public void LoadLayoutTemplates(string sStartupPath)
        {
            var listLayoutTemplates = new List<LayoutTemplate>();
            var sLayoutsPath = Path.Combine(sStartupPath, LAYOUT_TEMPLATES_FOLDER);
            try
            {
                if (!Directory.Exists(sLayoutsPath))
                {
                    Logger.AddLogLine("LayoutTemplates folder does not exist: {0}".FormatString(sLayoutsPath));
                    LayoutTemplates = listLayoutTemplates;
                }

                var arrayTemplateFiles = Directory.GetFiles(sLayoutsPath, "*.{0}".FormatString(LAYOUT_TEMPLATE_EXTENSION));
                LayoutTemplate zTemplate = null;
                foreach (var sLayoutFilePath in arrayTemplateFiles)
                {
                    try
                    {
                        SerializationUtils.DeserializeFromXmlFile(sLayoutFilePath, CardMakerConstants.XML_ENCODING, ref zTemplate);
                        listLayoutTemplates.Add(zTemplate);
                    }
                    catch (Exception)
                    {
                        Logger.AddLogLine("Failed to load LayoutTemplate file: {0}".FormatString(sLayoutFilePath));
                    }
                }
            }
            catch (Exception)
            {
                Logger.AddLogLine(string.Format("Failed to access template files in {0}", sLayoutsPath));
            }

            LayoutTemplates = listLayoutTemplates;
        }

        /// <summary>
        /// Saves the specified layout template
        /// </summary>
        /// <param name="sStartupPath">The path containing the templates folder</param>
        /// <param name="zLayoutTemplate">The template to save</param>
        /// <returns>true on success, false otherwise</returns>
        public bool SaveLayoutTemplate(string sStartupPath, LayoutTemplate zLayoutTemplate)
        {
            var sLayoutsPath = Path.Combine(sStartupPath, LAYOUT_TEMPLATES_FOLDER);
            try
            {
                if (!Directory.Exists(sLayoutsPath))
                {
                    Directory.CreateDirectory(sLayoutsPath);
                }
                var sFileName = ReplaceNonFileSafeCharacters(zLayoutTemplate.Layout.Name);
                var sOutput = Path.Combine(sLayoutsPath, sFileName) + "." + LAYOUT_TEMPLATE_EXTENSION;
                return SerializationUtils.SerializeToXmlFile(sOutput, zLayoutTemplate, CardMakerConstants.XML_ENCODING);
            }
            catch (Exception e)
            {
                Logger.AddLogLine("Error saving LayoutTemplate: {0}".FormatString(e.Message));
            }
            return false;
        }

        /// <summary>
        /// Deletes the specified layout template
        /// </summary>
        /// <param name="sStartupPath">The path containing the templates folder</param>
        /// <param name="zLayoutTemplate">The template to delete</param>
        public void DeleteLayoutTemplate(string sStartupPath, LayoutTemplate zLayoutTemplate)
        {
            var sLayoutsPath = Path.Combine(sStartupPath, LAYOUT_TEMPLATES_FOLDER);
            try
            {
                var sFileName = ReplaceNonFileSafeCharacters(zLayoutTemplate.Layout.Name);
                var sFilePath = Path.Combine(sLayoutsPath, sFileName) + "." + LAYOUT_TEMPLATE_EXTENSION;
                File.Delete(sFilePath);
            }
            catch (Exception e)
            {
                Logger.AddLogLine("Error deleting LayoutTemplate: {0}".FormatString(e.Message));
            }
        }

        /// <summary>
        /// Replaces any non file safe characters
        /// </summary>
        /// <param name="sInput">The raw file name to potentially alter</param>
        /// <returns>The altered file name</returns>
        private static string ReplaceNonFileSafeCharacters(string sInput)
        {
            var sFileName = sInput;
            foreach (var cInvalid in Path.GetInvalidFileNameChars())
            {
                sFileName = sFileName.Replace(cInvalid, NON_FILE_SAFE_REPLACEMENT_CHAR);
            }
            return sFileName;
        }

    }
}
