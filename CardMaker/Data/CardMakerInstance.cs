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

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CardMaker.Data
{
    /// <summary>
    /// Class containing per instance variables used across the application
    /// </summary>
    public static class CardMakerInstance
    {
        public static float ApplicationDPI { get; set; }
        public static bool DrawFormattedTextBorder { get; set; }
        public static bool DrawElementBorder { get; set; }
        public static bool GoogleCredentialsInvalid { get; set; }
        public static string GoogleAccessToken { get; set; }
        public static string LoadedProjectFilePath { get; set; }
        public static string CommandLineProjectFile { get; set; }
        public static Icon ApplicationIcon { get; set; }
        public static Form ApplicationForm { get; set; }

        public static string StartupPath
        {
            get
            {
                return Application.StartupPath + Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// User actively operating on the canvas
        /// </summary>
        public static bool CanvasUserAction { get; set; }

        /// <summary>
        /// LayoutControl element changes should fire events
        /// </summary>
        public static bool LayoutControlFireElementChangeEvents { get; set; }

        /// <summary>
        /// Is there redo/undo processing in progress
        /// </summary>
        public static bool ProcessingUserAction { get; set; }

        static CardMakerInstance()
        {
            ApplicationDPI = 72f;
            DrawFormattedTextBorder = false;
            DrawElementBorder = true;
            GoogleCredentialsInvalid = false;
            GoogleAccessToken = null;
            LoadedProjectFilePath = String.Empty;
        }
    }
}
