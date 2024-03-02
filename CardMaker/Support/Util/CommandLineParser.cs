////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Support.Util
{
	/// <summary>
	/// Summary description for CommandLineParser.
	/// </summary>
	public class CommandLineParser
	{
        private readonly Dictionary<string, List<string>> s_dictionaryArgs = new Dictionary<string, List<string>>();

		/// <summary>
		/// Private constructor (static class)
		/// </summary>
		public CommandLineParser(){}

		/// <summary>
		/// Parse the command line into a Hashtable. Arguments are space delimited.
		/// Examples: 
		/// -x filename		(would associate the "filename" with -x)
		/// -y				(simply flags -y as part of the command line)
		/// </summary>
		/// <param name="args"></param>
		public CommandLineParser Parse(string[] args)
		{
			var nIdx = 0;
            // sort out the command line into a dictionary of lists
            var listArgumentValues = new List<string>();
            // initialize a dumping ground for no arg params
            AddArgument(string.Empty, listArgumentValues);
            while (nIdx < args.Length)
			{
                if (args[nIdx].StartsWith("-"))
                {
                    listArgumentValues = new List<string>();
                    AddArgument(args[nIdx].ToUpper().Substring(1), listArgumentValues);
                }
                else
                {
                    listArgumentValues.Add(args[nIdx]);
                }
                nIdx++;
            }

            return this;
        }

		/// <summary>
		/// Adds the specified argument to the argument table if it has not been previously defined.
		/// </summary>
		/// <param name="sArg">Argument string</param>
		/// <param name="sValue">Argument value</param>
		private void AddArgument(string sArg, List<string> listArgumentValues)
		{
            if (s_dictionaryArgs.ContainsKey(sArg))
            {
                s_dictionaryArgs.Remove(sArg);
            }
            s_dictionaryArgs.Add(sArg, listArgumentValues);
		}

        /// <summary>
        /// Gets the value associated with the argument or the supplied default
        /// </summary>
        /// <param name="sArg">The argument to get</param>
        /// <param name="sDefault">The default value to get if the argument is not specified</param>
        /// <returns></returns>
        public string GetStringArg(string sArg, string sDefault)
        {
            string sReturn = GetStringArg(sArg);
            if (null == sReturn)
            {
                return sDefault;
            }
            return sReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eArg"></param>
        /// <returns></returns>
        public string GetStringArg(Enum eArg)
        {
            return GetStringArg(eArg.ToString());
        }

        /// <summary>
        /// Gets the value associated with the argument.
        /// </summary>
        /// <param name="sArg">The argument to get the value of.</param>
        /// <returns></returns>
        public string GetStringArg(string sArg)
		{
            string sKey = sArg.ToUpper();
            if (s_dictionaryArgs.ContainsKey(sKey))
            {
                if (null == s_dictionaryArgs[sKey])
                {
                    return string.Empty;
                }
                StringBuilder zBuilder = new StringBuilder();
                foreach(string sValue in s_dictionaryArgs[sKey])
                {
                    zBuilder.Append(sValue + " ");
                }
                return zBuilder.ToString().Trim();
            }
            return null;
		}

        /// <summary>
        /// Gets the arguments associated with an option as an array
        /// </summary>
        /// <param name="eArg"></param>
        /// <returns></returns>
        public string[] GetStringArgs(Enum eArg)
        {
            return GetStringArgs(eArg.ToString());
        }

        /// <summary>
        /// Gets the arguments associated with an option as an array
        /// </summary>
        /// <param name="sArg"></param>
        /// <returns></returns>
        public string[] GetStringArgs(string sArg)
        {
            string sKey = sArg.ToUpper();
            if (s_dictionaryArgs.ContainsKey(sKey))
            {
                return s_dictionaryArgs[sKey].ToArray();
            }
            return null;
        }

        /// <summary>
        /// Gets whether the argument was specified on the command line.
        /// </summary>
        /// <param name="sArg"></param>
        /// <returns></returns>
        public bool GetFlagArg(Enum eArg)
        {
            return GetFlagArg(eArg.ToString());
        }

        /// <summary>
        /// Gets whether the argument was specified on the command line.
        /// </summary>
        /// <param name="sArg"></param>
        /// <returns></returns>
        public bool GetFlagArg(string sArg)
		{
            return s_dictionaryArgs.ContainsKey(sArg.ToUpper());
		}

		/// <summary>
		/// Gets the number of arguments from the command line
		/// </summary>
		/// <returns>The number of command line arguments(pairs) </returns>
		public int GetArgCount()
		{
            return s_dictionaryArgs.Count;
		}

		/// <summary>
		/// Wrapper for printing the usage of the application.
		/// </summary>
		/// <param name="sUsage">Usage string</param>
		public static void PrintUsage(string sUsage)
		{
			Console.WriteLine("[" + Assembly.GetExecutingAssembly().ManifestModule.ToString() + " Command Line] " +  
				Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine(sUsage);
		}

		
		/// <summary>
		/// Debugging to see what the args are
		/// </summary>
		public void PrintArgs()
		{
            foreach (var sKey in s_dictionaryArgs.Keys)
			{
                Console.WriteLine(sKey + " [" + GetStringArg(sKey) + "]");
			}
		}

	}
}
