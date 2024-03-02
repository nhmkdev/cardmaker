////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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
using System.Windows.Forms;
using CardMaker.Card.CommandLine;
using CardMaker.Card.Shapes;
using CardMaker.Data;
using CardMaker.Events.Managers;
using CardMaker.Forms;
using Support.Progress;
using Support.UI;
using Support.Util;

namespace CardMaker
{
    public static class Program
    {
        // this is primarily here for the functional tests of the command line (repeating them breaks things)
        static Program()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Initialize();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            MainEntry(new ConsoleProgressReporterFactory(), args);
        }

        public static void MainEntry(ProgressReporterFactory zProgressReporterFactory,
            string[] args)
        {
            var commandLineProcessor = new CommandLineProcessor(
                new CommandLineParser().Parse(args),
                zProgressReporterFactory)
            {
                CommandLineUtil = new CommandLineUtil()
            };
            if (!commandLineProcessor.Process())
            {
#if !MONO_BUILD
                Win32.ShowConsole(Application.ExecutablePath, false);
#endif
                Application.Run(new CardMakerMDI());
            }
        }

        /// <summary>
        /// Cross application initialization of components.
        /// </summary>
        static void Initialize()
        {
            ShapeManager.Init();
            InitializationManager.RestoreReplacementChars();

            var zForm = new Form();
            var zGraphics = zForm.CreateGraphics();
            try
            {
                CardMakerInstance.ApplicationDPI = zGraphics.DpiX;
            }
            finally
            {
                zGraphics.Dispose();
            }
        }
    }
}