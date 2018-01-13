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

#if !MONO_BUILD

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Support.UI
{
	/// <summary>
	/// This class allows for access to win32 functionality that is not directly supported in .NET (hiding/showing the console)
	/// </summary>
	public class Win32 
	{
        // Win32 #defines
        private const int SW_HIDE = 0;
        private const int SW_RESTORE = 9;
        private const int WM_SETREDRAW = 0xB;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW = 5;
        private const uint SWP_NOACTIVATE = 0x0010;

		private Win32(){}

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleTitle(
			string sConsoleTitle // window title
			);

		[DllImport("user32")] 
		private static extern int FindWindow( 
			string lpClassName, // class name 
			string lpWindowName // window name 
			); 

		[DllImport("user32")]
		private static extern int ShowWindow(
			int hwnd,		// window handle
			int nCmdShow	// command
			);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(
             int hWnd,           // window handle
             int hWndInsertAfter,    // placement-order handle
             int X,          // horizontal position
             int Y,          // vertical position
             int cx,         // width
             int cy,         // height
             uint uFlags);       // window positioning flags

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

		/// <summary>
		/// Used to show/hide windows by their title.
		/// </summary>
		/// <param name="sName"></param>
		/// <param name="bShow"></param>
		private static void ShowWindow(string sName, bool bShow)
		{
		    var nHandle = FindWindow(null, sName);

		    ShowWindow(nHandle, bShow ? SW_RESTORE : SW_HIDE);
		}

		/// <summary>
		/// Used to show/hide the console window in console enabled applications. The Console will flicker on at startup.
		/// </summary>
        /// <param name="sConsoleTitle">A unique name for this instance of the console</param>
		/// <param name="bShow">Flag indicating whether to show or hide the console</param>
		public static void ShowConsole(string sConsoleTitle, bool bShow)
		{
			// force the name of console and change the visibility
			SetConsoleTitle(sConsoleTitle);
            ShowWindow(sConsoleTitle, bShow);
		}

        public static void SetRedraw(IntPtr HANDLE, bool bRedraw)
        {
            SendMessage(HANDLE, WM_SETREDRAW, (IntPtr)(bRedraw ? 1 : 0), IntPtr.Zero);
        }

        public static void ShowTopmost(IntPtr nFormHandle)
        {
            ShowWindow(nFormHandle.ToInt32(), SW_SHOW);
            SetForegroundWindow(nFormHandle.ToInt32());
        }

        public static void ShowInactiveTopmost(IntPtr nFormHandle, int nLeft, int nTop, int nWidth, int nHeight)
        {
            ShowWindow(nFormHandle.ToInt32(), SW_SHOWNOACTIVATE);
            SetWindowPos(nFormHandle.ToInt32(), -1, nLeft, nTop, nWidth, nHeight, SWP_NOACTIVATE);
        }


        #region RichTextBox

        // these are specific to rich text boxes
        private const int EM_GETSCROLLPOS = 0x0400 + 221;
        private const int EM_SETSCROLLPOS = 0x0400 + 222;

        // it's this or using actual pointers and forcing unsafe code
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, ref Point lp);

        public static Point GetRichTextScrollPosition(IntPtr HANDLE)
        {
            var pLocation = new Point();
            SendMessage(HANDLE, EM_GETSCROLLPOS, (IntPtr)0, ref pLocation);
            return pLocation;
        }

        public static void SetRichTextScrollPosition(IntPtr HANDLE, Point pLocation)
        {
            SendMessage(HANDLE, EM_SETSCROLLPOS, (IntPtr)0, ref pLocation);
        }

        #endregion

    } 
}
#endif