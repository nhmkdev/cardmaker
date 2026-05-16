////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2026 Tim Stair
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
using System.IO;
using CardMaker.Events.Managers;
using Support.IO;

namespace CardMaker.Card.ImageLoad
{
    internal static class ImagePathUtil
    {
        private const string APPLICATION_FOLDER_MARKER = "{appfolder}";

        /// <summary>
        /// Gets the path of the file based on path, project path, and local reference path
        /// </summary>
        /// <param name="sFile"></param>
        /// <returns>The file path if valid, null otherwise</returns>
        public static string GetExistingFilePath(string sFile)
        {
            try
            {
                if (sFile.StartsWith(APPLICATION_FOLDER_MARKER))
                {
                    sFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        sFile.Replace(APPLICATION_FOLDER_MARKER, string.Empty));
                }

                if (File.Exists(sFile))
                {
                    return sFile;
                }

                sFile = Path.Combine(ProjectManager.Instance.ProjectPath, sFile);
                if (File.Exists(sFile))
                {
                    return sFile;
                }

                if (sFile.Length > 1)
                {
                    switch (sFile[0])
                    {
                        case '/':
                        case '\\':
                            // last ditch effort (support for files like this: "/file.png"
                            sFile = Path.Combine(ProjectManager.Instance.ProjectPath, sFile.Substring(1));
                            if (File.Exists(sFile))
                            {
                                return sFile;
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.AddLogLine($"Invalid File Path: {sFile} -- {e.Message}");
            }

            return null;
        }
    }
}
