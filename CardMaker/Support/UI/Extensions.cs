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
using System.Windows.Forms;

namespace Support.UI
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Invokes the desired action if required
        /// </summary>
        /// <param name="zControl">Control to invoke upon</param>
        /// <param name="zAction">The Action to take</param>
        /// <returns>false if the action was invoked, true otherwise</returns>
        public static bool InvokeActionIfRequired(this Control zControl, Action zAction)
        {
            if (zControl.InvokeRequired)
            {
                zControl.Invoke(zAction);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Invokes the desired action
        /// </summary>
        /// <param name="zControl">Control to invoke upon</param>
        /// <param name="zAction">The Action to take</param>
        public static void InvokeAction(this Control zControl, Action zAction)
        {
            if (zControl.InvokeRequired)
            {
                zControl.Invoke(zAction);
            }
            else
            {
                zAction();
            }
        }

        /// <summary>
        /// Invokes the desired func as necessary
        /// </summary>
        /// <typeparam name="T">Template type for return value</typeparam>
        /// <param name="zControl">Control to invoke upon</param>
        /// <param name="zFunc">The Func to execute</param>
        /// <returns>The return value of the Func</returns>
        public static T InvokeFunc<T>(this Control zControl, Func<T> zFunc)
        {
            if (zControl.InvokeRequired)
            {
                return(T) zControl.Invoke(zFunc);
            }
            return zFunc();
        }

        /// <summary>
        /// Extensions for string formatting
        /// </summary>
        /// <param name="str"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string FormatString(this string str, params object[] list)
        {
            return string.Format(str, list);
        }
    }
}
