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
using System.Collections;
using System.Windows.Forms;

namespace Support.UI
{
	// Implements the manual sorting of items by columns.
	public class ListViewItemComparer : IComparer
	{

        // Usage Example
        /*

        private void lv_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItemComparer.SortColumn(lv, e, false);
        }

        */

        private readonly int m_nColumn;
		private readonly SortOrder m_sOrder;
		private readonly bool m_bNumber;
		
		public ListViewItemComparer(SortOrder sOrder)
		{
			m_nColumn = 0;
			m_sOrder = sOrder;
		}

        public static void SortColumn(ListView lv, ColumnClickEventArgs e, bool bNumberCompare)
        {
            if (SortOrder.Ascending == lv.Sorting)
            {
                lv.Sorting = SortOrder.Descending;
                lv.ListViewItemSorter = new ListViewItemComparer(e.Column,
                    SortOrder.Descending, bNumberCompare);
            }
            else
            {
                lv.Sorting = SortOrder.Ascending;
                lv.ListViewItemSorter = new ListViewItemComparer(e.Column,
                    SortOrder.Ascending, bNumberCompare);
            }
        }

		/// <summary>
		/// Basic sorter that allows for number/string based sorting.
		/// </summary>
		/// <param name="column">column to sort by index</param>
		/// <param name="sOrder">the sort order</param>
		/// <param name="bNumber">Flag indicating whether the column should be sorted as numbers</param>
		public ListViewItemComparer(int column, SortOrder sOrder, bool bNumber)
		{
			m_nColumn = column;
			m_sOrder = sOrder;
			m_bNumber = bNumber;
		}
		
		public int Compare(object x, object y)
		{
			if(m_bNumber)
			{
				return NumCompare(x, y);
			}
			else
			{
				return StringCompare(x, y);
			}
		}

		/// <summary>
		/// Compares the strings using the String.Compare method.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int StringCompare(object x, object y)
		{
			int nValue = String.Compare(((ListViewItem)x).SubItems[m_nColumn].Text, ((ListViewItem)y).SubItems[m_nColumn].Text, StringComparison.CurrentCulture);
			switch(m_sOrder)
			{
				case SortOrder.Ascending:
					return nValue;
				case SortOrder.Descending:
					return nValue * -1;
				default:
					return 0;
			}
		}

		/// <summary>
		/// Compares the strings as integers. Sorted non-integers are always assumed equal.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private int NumCompare(object x, object y)
		{
			var nValue = 0;
			try
			{
				var xValue = long.Parse(((ListViewItem)x).SubItems[m_nColumn].Text.Replace(",", string.Empty));
                var yValue = long.Parse(((ListViewItem)y).SubItems[m_nColumn].Text.Replace(",", string.Empty));
				if(xValue == yValue)
				{
					nValue = 0;
				}
				else if(xValue > yValue)
				{
					nValue = 1;
				}
				else
				{
					nValue = -1;
				}
				switch(m_sOrder)
				{
					case SortOrder.Ascending:
						return nValue;
					case SortOrder.Descending:
						return nValue * -1;
					default:
						return 0;
				}
			}
			catch(Exception)
			{
				return nValue;
			}
		}
	}
}
