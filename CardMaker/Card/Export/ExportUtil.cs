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
using System.Collections.Generic;
using System.Linq;

namespace CardMaker.Card.Export
{
    public static class ExportUtil
    {
        /// <summary>
        /// Gets the card indices specified in the input string
        /// </summary>
        /// <param name="sCardIndices">The string to check for indices</param>
        /// <returns>The indices specified in the string</returns>
        public static Tuple<string, int[]> GetCardIndices(string sCardIndices)
        {
            var listCardIndices = new List<int>();
            if (null != sCardIndices)
            {
                var arrayRanges = sCardIndices.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sRange in arrayRanges)
                {
                    var arrayEntries = sRange.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    int nStart;
                    int nEnd;
                    // NOTE: all of the inputs are cardId (1 based) -- converted to 0 based internally
                    switch (arrayEntries.Length)
                    {
                        case 1:
                            if (int.TryParse(arrayEntries[0], out nStart))
                            {
                                listCardIndices.Add(nStart - 1);
                            }
                            else
                            {
                                return new Tuple<string, int[]>("Invalid Card Index: " + sRange, null);
                            }
                            break;
                        case 2:
                            if (int.TryParse(arrayEntries[0], out nStart) && int.TryParse(arrayEntries[1], out nEnd))
                            {
                                listCardIndices.AddRange(Enumerable.Range(nStart - 1, (nEnd - nStart) + 1));
                            }
                            else
                            {
                                return new Tuple<string, int[]>("Invalid Card Index Range: " + sRange, null);
                            }
                            break;
                        default:
                            return new Tuple<string, int[]>("Invalid Card Index Range: " + sRange, null);
                    }
                }

                return new Tuple<string, int[]>(string.Empty, listCardIndices.Count() == 0 ? null : listCardIndices.ToArray());
            }

            return null;
        }
    }
}
