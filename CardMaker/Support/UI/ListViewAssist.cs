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
using System.Collections.Generic;
using System.Windows.Forms;

namespace Support.UI
{

	public static class ListViewAssist
	{
		/// <summary>
		/// Method to move all selected items in a listview the given amount up or down. "up" is negative, "down" is positive.
		/// </summary>
		/// <param name="lv"></param>
		/// <param name="nAmount">-1 and 1 are supported</param>
		public static void MoveListViewItems(ListView lv, int nAmount)
		{
			ListViewItem lvi ;
			int nPrevIdx;

			// nothing selected, done
            if(0 >= lv.SelectedItems.Count)
				return;

            switch (nAmount)
            {
                case 1:
                case -1:
                    break;
                default:
                    return;
            }

            // sort the items (necessary to determine the first index to move)
            var listSorted = new List<ListViewItem>();

		    foreach (ListViewItem zItem in lv.SelectedItems)
		    {
		        listSorted.Add(zItem);
		    }

		    listSorted.Sort((x, y) => x.Index.CompareTo(y.Index) );

            // moving down the list view (1)
            if (0 < nAmount)
            {
                int nCurrentListViewIdx = lv.Items.Count - 1;
                // find the first index to actually move (otherwise things will move above/below where they should go)
                for (int nIdx = listSorted.Count - 1; nIdx > -1; nIdx--)
                {
                    if (listSorted[nIdx].Index != nCurrentListViewIdx)
                    {
                        // found the start index, perform the actual moves
                        for (int nSortedIdx = nIdx; nSortedIdx > -1; nSortedIdx--)
                        {
                            lvi = listSorted[nSortedIdx];
                            nPrevIdx = lvi.Index;
                            lv.Items.RemoveAt(nPrevIdx);
                            lv.Items.Insert(Math.Min(lv.Items.Count, nPrevIdx + nAmount), lvi);
                        }
                        break;
                    }
                    nCurrentListViewIdx--;
                }

            }
            // moving up the list view (-1)
            else
            {
                int nCurrentListViewIdx = 0;
                // find the first index to actually move (otherwise things will move above/below where they should go)
                for (int nIdx = 0; nIdx < listSorted.Count; nIdx++)
                {
                    if (listSorted[nIdx].Index != nCurrentListViewIdx)
                    {
                        // found the start index, perform the actual moves
                        for (int nSortedIdx = nIdx; nSortedIdx < listSorted.Count; nSortedIdx++)
                        {
                            lvi = listSorted[nSortedIdx];
                            nPrevIdx = lvi.Index;
                            lv.Items.RemoveAt(nPrevIdx);
                            lv.Items.Insert(Math.Max(0, nPrevIdx + nAmount), lvi);
                        } 
                        break;
                    }
                    nCurrentListViewIdx++;
                }

            }

		}

        /// <summary>
        /// Adds a column to the specified list view
        /// </summary>
        /// <param name="sHeader">The column header</param>
        /// <param name="nWidth">The width of the header</param>
        /// <param name="lView">The listview to add to</param>
        /// <returns>The new ColumnHeader</returns>
        public static ColumnHeader AddColumn(string sHeader, int nWidth, ListView lView)
        {
            var zHeader = new ColumnHeader
            {
                Text = sHeader,
                Width = nWidth
            };
            lView.Columns.Add(zHeader);
            return zHeader;
        }

        /// <summary>
        /// Resizes the column headers (maximize the space per column)
        /// WARNING: If this is triggered within a Resize event the listview should have
        /// a minimum size set to avoid the vertical scroll bar on the column headers themselves.
        /// If not properly set the result may be an infinite loop of resize calls.
        /// This has no affect on vertical scroll bars for the items within the listview
        /// </summary>
        /// <param name="lvInput">The listview to update the column widths of</param>
        public static void ResizeColumnHeaders(ListView lvInput)
        {
            if (0 == lvInput.Columns.Count) return; // bad!
            if (0 == lvInput.ClientSize.Width) return; // no width? (minimized?)

            lvInput.Visible = false;
            lvInput.SuspendLayout();

            var arrayPercents = new float[lvInput.Columns.Count];
            int nColumnsWidth = 0;
            for (int nIdx = 0; nIdx < lvInput.Columns.Count; nIdx++)
            {
                nColumnsWidth += lvInput.Columns[nIdx].Width;
            }
            for (int nIdx = 0; nIdx < lvInput.Columns.Count; nIdx++)
            {
                arrayPercents[nIdx] = (float)lvInput.Columns[nIdx].Width / (float)nColumnsWidth;
            }
            for (int nIdx = 0; nIdx < lvInput.Columns.Count; nIdx++)
            {
                lvInput.Columns[nIdx].Width = (int)(arrayPercents[nIdx] * (float)lvInput.ClientSize.Width);
            }
            lvInput.ResumeLayout();
            lvInput.Visible = true;
        }

	}
}
