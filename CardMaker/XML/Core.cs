using System;
using System.IO;
using System.Text;
using Support.IO;

namespace CardMaker.XML
{
    /// <summary>
    /// This class contains all the core XML related functionality
    /// </summary>
    public static class Core
    {
        public static readonly Encoding XML_ENCODING = Encoding.UTF8;

        /// <summary>
        /// Loads a project file and reconfigures the specified TreeView to match
        /// </summary>
        /// <param name="sFile">The project file to load</param>
        /// <param name="tvMain">The treeview to populate</param>
        /// <returns></returns>
        public static Project LoadProject(string sFile)
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
                    sContents = sContents.Replace("xmlns=\"http://tempuri.org/Project.xsd\"", String.Empty);
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
                    Layout = new ProjectLayout[] { new ProjectLayout("Default") }
                };
            }
            return zProject;
        }

        /// <summary>
        /// Initialize the cache for each element in the ProjectLayout
        /// </summary>
        /// <param name="zLayout">The layout to initialize the cache</param>
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
    }
}
