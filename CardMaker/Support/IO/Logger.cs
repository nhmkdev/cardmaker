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
using System.IO;
using System.Windows.Forms;

namespace Support.IO
{
    public interface LoggerI
    {
        void AddLogLines(string[] arrayLines);
        void ClearLog();
    }

    public static class Logger
    {
        private static LoggerI s_iLogger;
        private static readonly string s_sLogFile = Application.StartupPath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".log";
        private static bool m_bLogToFile;

        private static StreamWriter s_zStreamWriter;

        public static string LogFile => s_sLogFile;

        /// <summary>
        /// The logger must be initialized through this method.
        /// </summary>
        /// <param name="iLogger">An object implementing the LoggerI interface.</param>
        /// <param name="bLogToFile">Flag indicating whether to log the output or not.</param>
        public static void InitLogger(LoggerI iLogger, bool bLogToFile)
        {
            s_iLogger = iLogger;
            m_bLogToFile = bLogToFile;
        }

        /// <summary>
        /// Clears the log (and log file if enabled) 
        /// </summary>
        public static void Clear()
        {
            if (m_bLogToFile)
            {
                if (null != s_zStreamWriter)
                {
                    s_zStreamWriter.Close();
                    s_zStreamWriter = null;
                }
                s_zStreamWriter = new StreamWriter(s_sLogFile, false);
            }
            s_iLogger.ClearLog();
        }

        /// <summary>
        /// Adds a single line
        /// </summary>
        /// <param name="sLine">Line to add</param>
        public static void AddLogLine(string sLine)
        {
            AddLogLines(sLine.Split(new [] { Environment.NewLine}, StringSplitOptions.None));
        }

        /// <summary>
        /// Adds an array of lines to the log
        /// </summary>
        /// <param name="arrayLines">Strings to log</param>
        public static void AddLogLines(string[] arrayLines)
        {
            if (null == s_iLogger)
            {
                return;
            }
            s_iLogger.AddLogLines(arrayLines);
            if (m_bLogToFile && (null != s_zStreamWriter))
            {
                foreach (string sLine in arrayLines)
                {
                    s_zStreamWriter.WriteLine(sLine);
                }
            }
        }

        /// <summary>
        /// Concludes the log (only applies to file logging)
        /// </summary>
        public static void EndLog()
        {
            if (m_bLogToFile && (null != s_zStreamWriter))
            {
                s_zStreamWriter.Close();
                s_zStreamWriter = null;
            }
        }
    }
}
