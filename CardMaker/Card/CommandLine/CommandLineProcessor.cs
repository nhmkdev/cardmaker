////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2020 Tim Stair
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
using CardMaker.Card.Export;
using CardMaker.Data;
using CardMaker.Events.Managers;
using Support.IO;
using Support.Progress;
using Support.UI;
using Support.Util;

namespace CardMaker.Card.CommandLine
{
    public class CommandLineProcessor
    {
        public CommandLineUtil CommandLineUtil { get; set; }

        private ProgressReporterFactory m_zProgressReporterFactory;

        private static readonly Dictionary<string, Type> dictionaryExporterType = new Dictionary<string, Type>()
        {
            { "PDF", typeof(PDFCommandLineExporter) },
        };

        private CommandLineParser m_zCommandLineParser;
        
        static CommandLineProcessor()
        {
            foreach (var sImageFormatName in FileCardExporterFactory.AllowedImageFormatNames)
            {
                dictionaryExporterType.Add(sImageFormatName.ToUpper(), typeof(ImageCommandLineExporter));
            }
        }

        protected CommandLineProcessor()
        {
            CommandLineUtil = new CommandLineUtil();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zCommandLineParser">The parser to use within this processor</param>
        public CommandLineProcessor(CommandLineParser zCommandLineParser, ProgressReporterFactory zProgressReporterFactory)
        {
            m_zCommandLineParser = zCommandLineParser;
            m_zProgressReporterFactory = zProgressReporterFactory;
        }

        /// <summary>
        /// Processes the command line
        /// </summary>
        /// <returns>true if the command line was processed, false otherwise (load the UI)</returns>
        public bool Process()
        {
            // detect if just a project file path is specified (normal UI load)
            if (ShouldLaunchGUI())
            {
                return false;
            }
            // Switch all progress reporting over to the command line
            CardMakerInstance.ProgressReporterFactory = m_zProgressReporterFactory == null
                ? new ConsoleProgressReporterFactory()
                : m_zProgressReporterFactory;
            Logger.InitLogger(new ConsoleLogger(), false);

            if (!LoadProject() || !PerformExport())
            {
                Environment.Exit(1);
            }
            return true;
        }

        /// <summary>
        /// Performs an export operation (determining what to do)
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        private bool PerformExport()
        {
            Type typeExporter;
            var sExportFormat = m_zCommandLineParser.GetStringArg(CommandLineArg.ExportFormat.ToString(), string.Empty)
                .ToUpper();
            if (dictionaryExporterType.TryGetValue(sExportFormat, out typeExporter))
            {
                var zExporter = (CommandLineExporterBase) Activator.CreateInstance(typeExporter);
                zExporter.CommandLineParser = m_zCommandLineParser;
                zExporter.CommandLineUtil = CommandLineUtil;
                zExporter.ConfigureGoogleCredential();
                return zExporter.Export();
            }
            else
            {
                CommandLineUtil.ExitWithError("Invalid export format specified: " + sExportFormat);
            }

            return false;
        }

        /// <summary>
        /// Loads the project specified on the command line
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        private bool LoadProject()
        {
            CardMakerInstance.CommandLineProjectFile = m_zCommandLineParser.GetStringArg(CommandLineArg.ProjectPath); ;
            if (!File.Exists(CardMakerInstance.CommandLineProjectFile))
            {
                CommandLineUtil.ExitWithError("Project file does not exist: " + CardMakerInstance.CommandLineProjectFile);
            }

            try
            {
                ProjectManager.Instance.OpenProject(CardMakerInstance.CommandLineProjectFile);
            }
            catch (Exception ex)
            {
                Logger.AddLogLine("Failed to load project: {0}--{1}".FormatString(CardMakerInstance.CommandLineProjectFile, ex.ToString()));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if the command line consists only of a project file path
        /// </summary>
        /// <returns>true if this is a project only command line, false otherwise</returns>
        private bool ShouldLaunchGUI()
        {
            if (m_zCommandLineParser.GetArgCount() == 1)
            {
                var sFilePath = m_zCommandLineParser.GetStringArg(string.Empty);
                if (File.Exists(sFilePath))
                {
                    CardMakerInstance.CommandLineProjectFile = sFilePath;
                }
                // if there's only one arg (the blank one) assume UI mode.
                return true;
            }
            return false;
        }
    }
}
