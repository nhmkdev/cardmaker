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

namespace Support.Util
{
	/// <summary>
	/// Basic class designed to simplify tracking/displaying time measurements.
	/// </summary>
	public class TimeMeasure
	{
        private static readonly Dictionary<string, Timer> m_dictionaryTimers = new Dictionary<string, Timer>();

		private struct Timer
		{
            public DateTime m_dtStart;
            public long m_lCount;
            public long m_lLastSpan;
            public long m_lAverageSpan;
            public long m_lTotalSpan;
		}

		private TimeMeasure(){}

		/// <summary>
		/// This method resets the state of the TimeMeasure object.
		/// </summary>
		/// <param name="sID">Timer ID string</param>
		public static void Init(string sID)
		{
            Timer zTimer;
            if (!m_dictionaryTimers.TryGetValue(sID, out zTimer))
            {
                m_dictionaryTimers.Add(sID, GetNewTimer());
            }
		}

		/// <summary>
		/// Starts the measurement.
		/// </summary>
		/// <param name="sID">Timer ID string</param>
		public static void StartMeasure(string sID)
		{	
			Timer zTimer;
            if (!m_dictionaryTimers.TryGetValue(sID, out zTimer))
            {
                zTimer = GetNewTimer();
                m_dictionaryTimers.Add(sID, zTimer);
            }
			zTimer.m_dtStart = DateTime.Now;
		}

		/// <summary>
		/// Ends the measurement.
		/// </summary>
		/// <param name="sID">Timer ID string</param>
		public static long EndMeasure(string sID)
		{
			Timer zTimer = m_dictionaryTimers[sID];
			DateTime dtNow = DateTime.Now;
			zTimer.m_lCount++;
			zTimer.m_lLastSpan = dtNow.Ticks - zTimer.m_dtStart.Ticks;
			zTimer.m_lTotalSpan += zTimer.m_lLastSpan;
			zTimer.m_lAverageSpan = zTimer.m_lTotalSpan / zTimer.m_lCount;
            return zTimer.m_lLastSpan;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sID">Timer ID string</param>
		/// <returns></returns>
		public static long GetMeasure(string sID)
		{
            Timer zTimer;
            if (m_dictionaryTimers.TryGetValue(sID, out zTimer))
            {
                return zTimer.m_lLastSpan;
            }
            return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sID">Timer ID string</param>
		/// <returns></returns>
		public static long GetAverage(string sID)
		{
            Timer zTimer;
            if (m_dictionaryTimers.TryGetValue(sID, out zTimer))
            {
                return zTimer.m_lAverageSpan;
            }
            return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sID">Timer ID string</param>
		/// <returns></returns>
		public static long GetTimerCount(string sID)
		{
			Timer zTimer;
            if (m_dictionaryTimers.TryGetValue(sID, out zTimer))
            {
                return zTimer.m_lCount;
            }
            return -1;
		}

		/// <summary>
		/// Gets the timer information for the specified item
		/// </summary>
		/// <param name="sID">Timer identifier</param>
		/// <returns>The timer string associated with the ID specified or string.empty if the timer does not exist</returns>
		public static string GetTimerInfo(string sID)
		{
			Timer zTimer;
            if (m_dictionaryTimers.TryGetValue(sID, out zTimer))
            {
                return sID + "\tS:" + zTimer.m_lLastSpan + "\tA:" + zTimer.m_lAverageSpan + "\tT:" + zTimer.m_lTotalSpan + "\tC:" +
                    zTimer.m_lCount;
            }
            return string.Empty;
		}

        private static Timer GetNewTimer()
        {
            return new Timer
            {
                m_dtStart = DateTime.MinValue,
                m_lAverageSpan = 0,
                m_lCount = 0,
                m_lLastSpan = 0,
                m_lTotalSpan = 0
            };
        }

		/// <summary>
		/// Writes the timer info string for the specified ID to the console
		/// </summary>
		/// <param name="sID">Timer identifier</param>
		public static void ConsoleWrite(string sID)
		{
			Console.WriteLine(GetTimerInfo(sID));
		}
	}
}
