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

using CardMaker.Card.FormattedText.Util;
using NUnit.Framework;

namespace UnitTest.Card.FormattedText.Util
{
    [TestFixture]
    internal class StringUtilTests
    {
        [TestCase("", "")]
        [TestCase("\t", "\t")]
        [TestCase("\ta", "\tA")]
        [TestCase("\ta\tb", "\tA\tB")]
        [TestCase("a\tb", "A\tB")]
        [TestCase("alpha beta gamma", "Alpha Beta Gamma")]
        [TestCase("1alpha 2beta 3gamma", "1alpha 2beta 3gamma")]
        [TestCase("-alpha =beta !gamma", "-alpha =beta !gamma")]
        [TestCase("alpha,beta, gamma", "Alpha,beta, Gamma")]
        [TestCase("Alpha Beta Gamma", "Alpha Beta Gamma")]
        [TestCase("Alpha Beta Gamma\t  ", "Alpha Beta Gamma\t  ")]
        [TestCase("ALPHA BETA gAmma\t  ", "Alpha Beta Gamma\t  ")]
        public void ValidateConvertToTitleCase(string sInput, string sExpected)
        {
            var sResult = StringUtil.ConvertToTitleCase(sInput);
            Assert.AreEqual(sResult, sExpected);
        }
    }
}
