////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2017 Tim Stair
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
using System.Reflection;
using CardMaker.Card.FormattedText;
using CardMaker.Data;
using CardMaker.XML;
using Support.Util;

namespace CardMaker.Card.Translation
{
    public abstract class TranslatorBase
    {
        public Dictionary<string, int> DictionaryColumnNameToIndex { get; private set; }
        public Dictionary<string, string> DictionaryDefines { get; private set; }
        public Dictionary<string, Dictionary<string, int>> DictionaryElementOverrides { get; }
        public List<string> ListColumnNames { get; private set; }

        protected readonly Dictionary<string, ElementString> m_dictionaryElementStringCache = new Dictionary<string, ElementString>();
        protected readonly Dictionary<string, FormattedTextDataCache> m_dictionaryMarkupCache = new Dictionary<string, FormattedTextDataCache>();

        protected TranslatorBase(Dictionary<string, int> dictionaryColumnNameToIndex, Dictionary<string, string> dictionaryDefines,
            Dictionary<string, Dictionary<string, int>> dictionaryElementOverrides, List<string> listColumnNames)
        {
            DictionaryColumnNameToIndex = dictionaryColumnNameToIndex;
            DictionaryDefines = dictionaryDefines;
            DictionaryElementOverrides = dictionaryElementOverrides;
            ListColumnNames = listColumnNames;
        }

        public ElementString TranslateString(string sRawString, int nCardIndex, DeckLine zDeckLine,
            ProjectLayoutElement zElement, string sCacheSuffix = "")
        {
            string sCacheKey = zElement.name + sCacheSuffix;
            ElementString zCached;
            if (m_dictionaryElementStringCache.TryGetValue(sCacheKey, out zCached))
            {
                return zCached;
            }

            var zElementString = TranslateToElementString(sRawString, nCardIndex, zDeckLine, zElement);

            if (zElementString.String.Contains("#nodraw"))
            {
                zElementString.DrawElement = false;
            }

            // all translators perform this post replacement operation
            switch ((ElementType)Enum.Parse(typeof(ElementType), zElement.type))
            {
                case ElementType.Text:
                    zElementString.String = zElementString.String.Replace("\\n", Environment.NewLine);
                    zElementString.String = zElementString.String.Replace("\\q", "\"");
                    zElementString.String = zElementString.String.Replace("\\c", ",");
                    zElementString.String = zElementString.String.Replace("&gt;", ">");
                    zElementString.String = zElementString.String.Replace("&lt;", "<");
                    break;
                case ElementType.FormattedText:
                    // NOTE: never convert \n => <br> here. This will affect file paths that include '\n' (ie. c:\newfile.png)
                    zElementString.String = zElementString.String.Replace("<c>", ",");
                    zElementString.String = zElementString.String.Replace("<q>", "\"");
                    zElementString.String = zElementString.String.Replace("&gt;", ">");
                    zElementString.String = zElementString.String.Replace("&lt;", "<");
                    break;
            }

            AddStringToTranslationCache(sCacheKey, zElementString);

            return zElementString;
        }

        protected abstract ElementString TranslateToElementString(string sRawString, int nCardIndex, DeckLine zDeckLine,
            ProjectLayoutElement zElement);

        public ProjectLayoutElement GetOverrideElement(ProjectLayoutElement zElement, int nCardIndex, List<string> arrayLine, DeckLine zDeckLine)
        {
            Dictionary<string, int> dictionaryOverrideColumns;
            string sNameLower = zElement.name.ToLower();
            DictionaryElementOverrides.TryGetValue(sNameLower, out dictionaryOverrideColumns);
            if (null == dictionaryOverrideColumns)
            {
                return zElement;
            }

            var zOverrideElement = new ProjectLayoutElement();
            zOverrideElement.DeepCopy(zElement, false);
            zOverrideElement.name = zElement.name;

            foreach (string sKey in dictionaryOverrideColumns.Keys)
            {
                Type zType = typeof(ProjectLayoutElement);
                PropertyInfo zProperty = zType.GetProperty(sKey);
                if (null != zProperty && zProperty.CanWrite)
                {
                    MethodInfo zMethod = zProperty.GetSetMethod();
                    int nOverrideValueColumnIdx = dictionaryOverrideColumns[sKey];
                    if (arrayLine.Count <= nOverrideValueColumnIdx)
                    {
                        continue;
                    }
                    string sValue = arrayLine[nOverrideValueColumnIdx].Trim();

                    // Note: TranslateString maintains an element name based cache, the key is critical to make this translation unique
                    sValue = TranslateString(sValue, nCardIndex, zDeckLine, zOverrideElement, sKey).String;

                    if (!string.IsNullOrEmpty(sValue))
                    {
                        if (zProperty.PropertyType == typeof(string))
                        {
                            zMethod.Invoke(zOverrideElement, new object[] { sValue });
                        }
                        else if (zProperty.PropertyType == typeof(float))
                        {
                            float fValue;
                            if (ParseUtil.ParseFloat(sValue, out fValue))
                            {
                                zMethod.Invoke(zOverrideElement, new object[] { fValue });
                            }
                        }
                        else if (zProperty.PropertyType == typeof(bool))
                        {
                            bool bValue;
                            if (bool.TryParse(sValue, out bValue))
                            {
                                zMethod.Invoke(zOverrideElement, new object[] { bValue });
                            }
                        }
                        else if (zProperty.PropertyType == typeof(Int32))
                        {
                            int nValue;
                            if (int.TryParse(sValue, out nValue))
                            {
                                zMethod.Invoke(zOverrideElement, new object[] { nValue });
                            }
                        }
                    }
                }
            }
            zOverrideElement.InitializeCache(); // any cached items must be recached
            return zOverrideElement;
        }

        #region Translation Cache

        public void ResetTranslationCache(ProjectLayoutElement zElement)
        {
            if (m_dictionaryElementStringCache.ContainsKey(zElement.name))
            {
                m_dictionaryElementStringCache.Remove(zElement.name);
            }
        }

        public void ResetTranslationCache()
        {
            m_dictionaryElementStringCache.Clear();
        }

        protected void AddStringToTranslationCache(string sKey, ElementString zElementString)
        {
            if (m_dictionaryElementStringCache.ContainsKey(sKey))
            {
                m_dictionaryElementStringCache.Remove(sKey);
                //Logger.AddLogLine("String Cache: Replace?!");
            }
            m_dictionaryElementStringCache.Add(sKey, zElementString);
        }

        public ElementString GetStringFromTranslationCache(string sKey)
        {
            if (m_dictionaryElementStringCache.ContainsKey(sKey))
                return m_dictionaryElementStringCache[sKey];
            return null;
        }

        public FormattedTextDataCache GetCachedMarkup(string sElementName)
        {
            FormattedTextDataCache zCached;
            if (m_dictionaryMarkupCache.TryGetValue(sElementName, out zCached))
            {
                return zCached;
            }
            return null;
        }

        #endregion

        public void ResetDeckCache()
        {
            ResetTranslationCache();
            ResetMarkupCache();
        }

        #region Markup Cache

        public void AddCachedMarkup(string sElementName, FormattedTextDataCache zFormattedData)
        {
            m_dictionaryMarkupCache.Add(sElementName, zFormattedData);
        }

        public void ResetMarkupCache()
        {
            m_dictionaryMarkupCache.Clear();
        }

        public void ResetMarkupCache(string sElementName)
        {
            if (m_dictionaryMarkupCache.ContainsKey(sElementName))
            {
                m_dictionaryMarkupCache.Remove(sElementName);
            }
        }
        #endregion

    }
}
