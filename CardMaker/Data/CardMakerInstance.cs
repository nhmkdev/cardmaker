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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CardMaker.Card;

namespace CardMaker.Data
{
    /// <summary>
    /// Class containing per instance variables used across the application
    /// </summary>
    public static class CardMakerInstance
    {
        /// <summary>
        /// The DPI of the CardMaker form application
        /// </summary>
        public static float ApplicationDPI { get; set; }

        /// <summary>
        /// Flag indicating whether to draw the formatted text borders
        /// </summary>
        public static bool DrawFormattedTextBorder { get; set; }

        /// <summary>
        /// Flag indicating whether to draw the element borders
        /// </summary>
        public static bool DrawElementBorder { get; set; }

        /// <summary>
        /// Flag indicating whether to draw the selected element guides
        /// </summary>
        public static bool DrawSelectedElementGuides { get; set; }

        /// <summary>
        /// Flag indicating whether to attempt to draw the layout dividers
        /// </summary>
        public static bool DrawLayoutDividers { get; set; }

        /// <summary>
        /// The number of dividers to render horizontally
        /// </summary>
        public static int LayoutDividerHorizontalCount { get; set; }

        /// <summary>
        /// The number of dividers to render vertically
        /// </summary>
        public static int LayoutDividerVerticalCount { get; set; }

        /// <summary>
        /// Flag indicating whether to draw the selected element rotation bounds
        /// </summary>
        public static bool DrawSelectedElementRotationBounds { get; set; }

        /// <summary>
        /// Flag indicating if the Google credentials are invalid
        /// </summary>
        public static bool GoogleCredentialsInvalid { get; set; }

        /// <summary>
        /// The current Google access token
        /// </summary>
        public static string GoogleAccessToken { get; set; }

        /// <summary>
        /// The project file indicated on the command line (first argument)
        /// </summary>
        public static string CommandLineProjectFile { get; set; }

        /// <summary>
        /// Flag to all reference readers to refresh their cache as necessary
        /// </summary>
        public static bool ForceDataCacheRefresh { get; set; }

        /// <summary>
        /// The application icon
        /// </summary>
        public static Icon ApplicationIcon { get; set; }

        /// <summary>
        /// The main application form
        /// </summary>
        public static Form ApplicationForm { get; set; }

        public static string StartupPath => Application.StartupPath + Path.DirectorySeparatorChar;

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

        public static Random Random { get; set; }

        static CardMakerInstance()
        {
            ApplicationDPI = 72f;
            DrawFormattedTextBorder = false;
            DrawElementBorder = true;
            DrawSelectedElementGuides = true;
            DrawSelectedElementRotationBounds = true;
            GoogleCredentialsInvalid = false;
            GoogleAccessToken = null;
            ProcessingUserAction = false;
            Random = new Random();
        }
    }
}
