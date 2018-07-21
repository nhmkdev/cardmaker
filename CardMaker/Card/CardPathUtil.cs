using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CardMaker.Events.Managers;

namespace CardMaker.Card
{
    public static class CardPathUtil
    {
        private const string APPLICATION_FOLDER_MARKER = "{appfolder}";

        public static string getPath(string sPath)
        {
            if (sPath.StartsWith(APPLICATION_FOLDER_MARKER))
            {
                sPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    sPath.Replace(APPLICATION_FOLDER_MARKER, string.Empty));
            }
            if (!File.Exists(sPath))
            {
                sPath = ProjectManager.Instance.ProjectPath + sPath;
            }
            return sPath;
        }

    }
}
