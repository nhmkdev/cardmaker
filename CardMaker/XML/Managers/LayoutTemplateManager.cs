using System;
using System.Collections.Generic;
using System.IO;
using Support.IO;
using Support.UI;

namespace CardMaker.XML.Managers
{
    public class LayoutTemplateManager : BaseXmlManager
    {
        private const string LAYOUT_TEMPLATES_FOLDER = "templates";
        private const string LAYOUT_TEMPLATE_EXTENSION = "xml";
        private const char NON_FILE_SAFE_REPLACEMENT_CHAR = '_';

        public List<LayoutTemplate> LayoutTemplates { get; set; } 

        private static LayoutTemplateManager m_zInstance;

        public static LayoutTemplateManager Instance
        {
            get
            {
                if (m_zInstance == null)
                {
                    m_zInstance = new LayoutTemplateManager();
                }
                return m_zInstance;
            }
        }

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
                        SerializationUtils.DeserializeFromXmlFile(sLayoutFilePath, XML_ENCODING, ref zTemplate);
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
                return SerializationUtils.SerializeToXmlFile(sOutput, zLayoutTemplate, XML_ENCODING);
            }
            catch (Exception e)
            {
                Logger.AddLogLine("Error saving LayoutTemplate: {0}".FormatString(e.Message));
                throw;
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
