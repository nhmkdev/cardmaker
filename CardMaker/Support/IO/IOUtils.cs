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
using System.Globalization;
using System.IO;
using System.Text;

namespace Support.IO
{
    public static class IOUtils
    {
        /// <summary>
        /// Gets the translation path based on the input root
        /// </summary>
        /// <param name="sRootPath">The startint path</param>
        /// <param name="sDestinationPath">The destination path</param>
        /// <returns></returns>
        public static string GetRelativePath(string sRootPath, string sDestinationPath)
        {
            if (string.IsNullOrEmpty(sRootPath))
            {
                return sDestinationPath;
            }
            if (sDestinationPath.StartsWith(sRootPath)) // check if the destination is under the core
            {
                return sDestinationPath.Remove(0,
                    sRootPath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture))
                        ? (sRootPath.Length)
                        : (sRootPath.Length + 1));
            }
            if (sRootPath[0] != sDestinationPath[0]) // check if the drive letter differs
            {
                return sDestinationPath;
            }

            var zBuilder = new StringBuilder();

            var arrayRoot = sRootPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var arrayDest = sDestinationPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var nMax = (arrayRoot.Length < arrayDest.Length) ? arrayRoot.Length : arrayDest.Length;
            int nIdx;
            for (nIdx = 0; nIdx < nMax; nIdx++)
            {
                if (!arrayRoot[nIdx].Equals(arrayDest[nIdx], StringComparison.CurrentCultureIgnoreCase))
                    break;
            }
            for (int nPrevDir = 0; nPrevDir < (arrayRoot.Length - (nIdx)); nPrevDir++)
            {
                zBuilder.Append(".." + Path.DirectorySeparatorChar);
            }
            for (int nSubDir = nIdx; nSubDir < arrayDest.Length; nSubDir++)
            {
                zBuilder.Append(arrayDest[nSubDir] + Path.DirectorySeparatorChar);
            }
            var sOutput = zBuilder.ToString();

            if (!sDestinationPath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                sOutput = sOutput.Remove(sOutput.Length - 1);
            return sOutput;
        }

        /// <summary>
        /// Gets the fully qualified path of an item based on the root folder and input relative path
        /// </summary>
        /// <param name="sRootPath"></param>
        /// <param name="sRelativePath"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(string sRootPath, string sRelativePath)
        {
            if (File.Exists(sRelativePath))
                return sRelativePath;

            var zBuilder = new StringBuilder();
            var arrayRoot = sRootPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var arrayRelative = sRelativePath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            int nIdx;
            for (nIdx = 0; nIdx < arrayRelative.Length; nIdx++)
            {
                if (!arrayRelative[nIdx].Equals("..", StringComparison.CurrentCultureIgnoreCase))
                    break;
            }
            for (int nRootIdx = 0; nRootIdx < (arrayRoot.Length - nIdx); nRootIdx++)
            {
                zBuilder.Append(arrayRoot[nRootIdx] + Path.DirectorySeparatorChar);
            }

            // if the relative path started with a "..\" remove it
            int nStartIdx = sRelativePath.LastIndexOf("..");
            if (-1 != nStartIdx)
                nStartIdx += 3;
            else
                nStartIdx = 0;

            zBuilder.Append(sRelativePath.Substring(nStartIdx));
            return zBuilder.ToString();
        }

        /// <summary>
        /// Updates the relative path based on the old and new root folder.
        /// </summary>
        /// <param name="sOriginalRoot">The previous root folder</param>
        /// <param name="sRelativePath">The relative path of the item from the previous root</param>
        /// <param name="sNewRoot">The new root path</param>
        /// <returns></returns>
        public static string UpdateRelativePath(string sOriginalRoot, string sRelativePath, string sNewRoot)
        {
            string sAbsolutePath = GetAbsolutePath(sOriginalRoot, sRelativePath);
            return GetRelativePath(sNewRoot, sAbsolutePath);
        }
    }

}