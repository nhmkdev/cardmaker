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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Support.IO
{

    public class IniManager
    {
        private const char CHAR_SPLITTER = '=';
        private const string CONTROL_FORMAT = "{0}_{1}";

        private Dictionary<string, string> m_dictionaryItems = new Dictionary<string, string>();
        private bool m_bReadOnly;
        private bool m_bMatchCase;

        private enum ERestoreStateValue
        {
            LocationX,
            LocationY,
            SizeWidth,
            SizeHeight,
            State,
            End
        }

        /// <summary>
        /// Sets the IniManager to flush to the ini file every time a value is set. (Automatic call to FlushIniSettings)
        /// </summary>
        public bool AutoFlush { get; set; }

        /// <summary>
        /// The ini file path
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Creates a new IniManager 
        /// </summary>
        /// <param name="sFileName">The file to use in the IniManager</param>
        /// <param name="bReadOnly">Flag indicating items can only be read</param>
        public IniManager(string sFileName, bool bReadOnly)
        {
            Init(sFileName, bReadOnly, true, false);
        }

        /// <summary>
        /// Creates a new IniManager
        /// </summary>
        /// <param name="sFileName">Path of the ini file (or just the name)</param>
        /// <param name="bReadOnly">Flag indicating items can only be read</param>
        /// <param name="bUseApplicationPath">Automatically construct the full path using the startup path and append .ini</param>
        public IniManager(string sFileName, bool bReadOnly, bool bUseApplicationPath)
        {
            Init(sFileName, bReadOnly, bUseApplicationPath, false);
        }

        /// <summary>
        /// Creates a new IniManager
        /// </summary>
        /// <param name="sFileName">Path of the ini file (or just the name)</param>
        /// <param name="bReadOnly">Flag indicating items can only be read</param>
        /// <param name="bUseApplicationPath">Automatically construct the full path using the startup path and append .ini</param>
        /// <param name="bMatchCase">Items in the ini file must match the case of the item specified.</param>
        public IniManager(string sFileName, bool bReadOnly, bool bUseApplicationPath, bool bMatchCase)
        {
            Init(sFileName, bReadOnly, bUseApplicationPath, bMatchCase);
        }

        /// <summary>
        /// Core call to setup the inimanager
        /// </summary>
        /// <param name="sFileName">Path of the ini file (or just the name)</param>
        /// <param name="bReadOnly">Flag indicating items can only be read</param>
        /// <param name="bUseApplicationPath">Automatically construct the full path using the startup path and append .ini</param>
        /// <param name="bMatchCase">Items in the ini file must match the case of the item specified.</param>
        private void Init(string sFileName, bool bReadOnly, bool bUseApplicationPath, bool bMatchCase)
        {
            if (bUseApplicationPath)
                Filename = Application.StartupPath + Path.DirectorySeparatorChar + sFileName + ".ini";
            else
                Filename = sFileName;

            m_bReadOnly = bReadOnly;
            m_bMatchCase = bMatchCase;
            m_dictionaryItems = GetIniTable(Filename, CHAR_SPLITTER);
        }

        /// <summary>
        /// Gets the value of the string specified
        /// </summary>
		/// <param name="sItem">The string requested</param>
		/// <param name="sDefault">The default value</param>
		/// <returns>The value of the string or default value</returns>
        public string GetValue(string sItem, string sDefault="")
        {
            string sCheck = m_bMatchCase ? sItem : sItem.ToLower();

            string sValue;
            if (m_dictionaryItems.TryGetValue(sCheck, out sValue))
            {
                return sValue;
            }

            return sDefault;
        }

        /// <summary>
        /// Gets the value of the string specified
        /// </summary>
        /// <param name="eItem">The enum.ToString() requested</param>
        /// <param name="sDefault">The default value</param>
        /// <returns>The value of the string or string.empty</returns>
        public string GetValue(Enum eItem, string sDefault="")
        {
            return GetValue(eItem.ToString(), sDefault);
        }

		/// <summary>
        /// Sets the value of a specified string
        /// </summary>
        /// <param name="sItem">The key string</param>
        /// <param name="sValue">The value string</param>
        public void SetValue(string sItem, string sValue)
        {
            if(m_bReadOnly)
                throw new Exception("Attempting to write to a read-only IniManager! " + Filename);

            string sCheck = m_bMatchCase ? sItem : sItem.ToLower();

            if (m_dictionaryItems.ContainsKey(sCheck))
                m_dictionaryItems[sCheck] = sValue;
            else
                m_dictionaryItems.Add(sCheck, sValue);

            if (AutoFlush)
                FlushIniSettings();
        }

        /// <summary>
        /// Sets the value of a specified string
        /// </summary>
        /// <param name="eItem">The key string</param>
        /// <param name="sValue">The value string</param>
        public void SetValue(Enum eItem, string sValue)
        {
            SetValue(eItem.ToString(), sValue);
        }

        /// <summary>
        /// Flushes the Ini settings to the file associated with this object
        /// </summary>
        public void FlushIniSettings()
        {
            string sDirectory = Path.GetDirectoryName(Filename);
            if (sDirectory != null && !Directory.Exists(sDirectory))
            {
                Directory.CreateDirectory(sDirectory);
            }

            try
            {
                var zWriter = new StreamWriter(Filename, false);

                foreach (var sKey in m_dictionaryItems.Keys)
                {
                    zWriter.WriteLine(sKey + CHAR_SPLITTER + m_dictionaryItems[sKey]);
                }
                zWriter.Close();
            }
#warning do nothing?
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Method to return all of the current keys
        /// </summary>
        /// <returns>Returns all of the key strings</returns>
        public string[] GetKeys()
        {
            var arrayKeys = new string[m_dictionaryItems.Keys.Count];
            int nIdx = 0;
            foreach (string sKey in m_dictionaryItems.Keys)
            {
                arrayKeys[nIdx] = sKey;
                nIdx++;
            }
            return arrayKeys;
        }
        
        /// <summary>
        /// Reads the specified ini file
        /// </summary>
        /// <param name="sFile">The file to read from</param>
        /// <param name="cSplitItem">The character to split on</param>
        /// <returns>Dictionary of keys and values read from the file</returns>
        private Dictionary<string, string> GetIniTable(string sFile, char cSplitItem)
        {
            var dictionaryItems = new Dictionary<string, string>();
            try
            {
                if (File.Exists(sFile))
                {
                    var arrayLines = File.ReadAllLines(sFile);
                    foreach (var sLine in arrayLines)
                    {
                        var arraySplit = sLine.Split(new char[] {cSplitItem}, 2);
                        var sItem = m_bMatchCase ? arraySplit[0] : arraySplit[0].ToLower();
                        if (!dictionaryItems.ContainsKey(sItem))
                            dictionaryItems.Add(sItem, arraySplit[1]);
                    }
                }
            }
#warning do nothing??
            catch (Exception)
            {
            }
            return dictionaryItems;
        }

        /// <summary>
        /// Gets the string representation of the specified form's state
        /// </summary>
        /// <param name="zForm">The form to get the state of</param>
        /// <returns></returns>
        public static string GetFormSettings(Form zForm)
        {
            if (FormWindowState.Normal == zForm.WindowState)
            {
                return zForm.Location.X + ";" + zForm.Location.Y + ";" +
                       zForm.Size.Width + ";" + zForm.Size.Height + ";" +
                       (int) zForm.WindowState;
            }
            return zForm.RestoreBounds.Location.X + ";" + zForm.RestoreBounds.Location.Y + ";" +
                    zForm.RestoreBounds.Size.Width + ";" + zForm.RestoreBounds.Size.Height + ";" +
                    (int)zForm.WindowState;
        }

        /// <summary>
        /// Restores the state of a form based on the input string
        /// </summary>
        /// <param name="zForm">The form to restore the state of</param>
        /// <param name="sInput">The string representation of the form state</param>
        public static void RestoreState(Form zForm, string sInput)
        {
            string[] arraySplit = sInput.Split(new char[] { ';' });
            var nValues = new int[(int)ERestoreStateValue.End];
            if ((int)ERestoreStateValue.End == arraySplit.Length)
            {
                for (int nIdx = 0; nIdx < arraySplit.Length; nIdx++)
                {
                    nValues[nIdx] = int.Parse(arraySplit[nIdx]);
                }
                zForm.Location = new Point(nValues[(int)ERestoreStateValue.LocationX], nValues[(int)ERestoreStateValue.LocationY]);
                zForm.Size = new Size(nValues[(int)ERestoreStateValue.SizeWidth], nValues[(int)ERestoreStateValue.SizeHeight]);
                zForm.WindowState = (FormWindowState)nValues[(int)ERestoreStateValue.State];
            }
        }

        /// <summary>
        /// Stores the settings of the controls in the specified form
        /// </summary>
        /// <param name="zForm">The form to store the controls of</param>
        public void StoreControlSettings(Form zForm)
        {
            foreach (Control zControl in zForm.Controls)
            {
                string sKeyName = string.Format(CONTROL_FORMAT, zForm.Name, zControl.Name);
                if (zControl is TextBox)
                     SetValue(sKeyName, ((TextBox)zControl).Text);
                else if (zControl is NumericUpDown)
                    SetValue(sKeyName, ((NumericUpDown)zControl).Value.ToString(CultureInfo.CurrentCulture));
                else if (zControl is CheckBox)
                    SetValue(sKeyName, ((CheckBox)zControl).Checked.ToString());
            }
        }

        /// <summary>
        /// Restores the settings of the controls in the specified form. 
        /// Proceed with caution in relation to value change events!
        /// </summary>
        /// <param name="zForm"></param>
        public void RestoreControlSettings(Form zForm)
        {
            foreach (Control zControl in zForm.Controls)
            {
                string sKeyName = string.Format(CONTROL_FORMAT, zForm.Name, zControl.Name);
                if (zControl is TextBox)
                {
                    ((TextBox) zControl).Text = GetValue(sKeyName, string.Empty);
                }
                else if (zControl is NumericUpDown)
                {
                    ((NumericUpDown) zControl).Value = decimal.Parse(GetValue(sKeyName, "0"));
                }
                else if (zControl is CheckBox)
                {
                    ((CheckBox) zControl).Checked = bool.Parse(GetValue(sKeyName, bool.FalseString));
                }
            }
        }
    }
}
