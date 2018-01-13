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
using System.Reflection;
using System.Windows.Forms;

namespace Support.UI
{
    public class PanelEx : Panel
    {
        private bool m_bDisableScrollToControl;

        public PanelEx()
        {
            // remove flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            // required to keep drawing when scrolling...
            SetScrollState(ScrollStateFullDrag, true);
            base.OnResize(eventargs);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            if (m_bDisableScrollToControl)
            {
                return AutoScrollPosition;
            }
            return base.ScrollToControl(activeControl);
        }

        public void ScrollToXY(int nX, int nY)
        {
            SetDisplayRectLocation(nX, nY);
            AdjustFormScrollbars(true);
        }

        /// <summary>
        /// Configures a ScrollableControl to allow full drag and draw.
        /// </summary>
        /// <param name="scrollableControl"></param>
        public static void SetupScrollState(ScrollableControl scrollableControl)
        {
            Type scrollableControlType = typeof(ScrollableControl);
            MethodInfo setScrollStateMethod = scrollableControlType.GetMethod("SetScrollState", BindingFlags.NonPublic | BindingFlags.Instance);
            setScrollStateMethod.Invoke(scrollableControl, new object[] { ScrollStateFullDrag, true });
        }

        /// <summary>
        /// Configures the scroll to control functionality within the Panel
        /// </summary>
        public bool DisableScrollToControl
        {
            get { return m_bDisableScrollToControl; }
            set { m_bDisableScrollToControl = value; }
        }
    }
}