////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
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

using System.Timers;
using CardMaker.Data;
using Support.IO;
using Support.UI;

namespace CardMaker.Events.Managers
{
    public class AutoSaveManager
    {
        private static AutoSaveManager m_zInstance;

        private System.Timers.Timer m_zTimer;
        private AbstractDirtyForm m_zAbstractDirtyForm;

        public static AutoSaveManager Instance => m_zInstance ?? (m_zInstance = new AutoSaveManager());

        ~AutoSaveManager()
        {
            m_zTimer?.Stop();
        }

        public void Init(AbstractDirtyForm zAbstractDirtyForm)
        {
            m_zAbstractDirtyForm = zAbstractDirtyForm;
            m_zTimer = new System.Timers.Timer(GetTimeInterval())
            {
                AutoReset = true,
                Enabled = CardMakerSettings.AutoSaveEnabled
            };
            m_zTimer.Elapsed += autoSaveTimer_Elapsed;
            LogAutoSaveState();

        }

        private void autoSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (m_zAbstractDirtyForm != null 
                && m_zAbstractDirtyForm.Dirty 
                && !string.IsNullOrWhiteSpace(m_zAbstractDirtyForm.LoadedFile))
            {
                Logger.AddLogLine("AutoSaving {0}...".FormatString(m_zAbstractDirtyForm.LoadedFile));
                m_zAbstractDirtyForm.InvokeAction(() => m_zAbstractDirtyForm.InitSave(false));
            }
        }

        public void ToggleAutoSave()
        {
            m_zTimer.Enabled = !m_zTimer.Enabled;
            CardMakerSettings.AutoSaveEnabled = m_zTimer.Enabled;
            LogAutoSaveState();
        }

        private void LogAutoSaveState()
        {
            if (IsEnabled())
                Logger.AddLogLine(
                    "Auto-Save Enabled with interval: {0}m".FormatString(CardMakerSettings.AutoSaveIntervalMinutes));
            else
                Logger.AddLogLine("Auto-Save Disabled");
        }

        public void EnableAutoSave(bool bEnable)
        {
            ConfigureTimerInterval();
            if (bEnable != IsEnabled())
                ToggleAutoSave();
        }

        private void ConfigureTimerInterval()
        {
            m_zTimer.Interval = GetTimeInterval();
        }

        private int GetTimeInterval()
        {
            return CardMakerSettings.AutoSaveIntervalMinutes * 60 * 1000;
        }

        

        public bool IsEnabled()
        {
            return m_zTimer.Enabled;
        }
    }
}
