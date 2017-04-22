using System;
using NUnit.Framework;
using Support.Util;

namespace UnitTest.Support.Util
{
    [TestFixture]
    internal class ParseUtilTest
    {
        [TestCase("1", 1, Result = true)]
        [TestCase("1.0", 1, Result = true)]
        [TestCase("1,0", 1, Result = true)]
        [TestCase("1.1", 1.1f, Result = true)]
        [TestCase("1,1", 1.1f, Result = true)]
        [TestCase("123.456", 123.456f, Result = true)]
        [TestCase("123,456", 123.456f, Result = true)]
        [TestCase("c", 0, Result = false)]
        public bool VerifyFloatParse(string sInput, float fExpectedValue)
        {
            float fValue;
            if (!ParseUtil.ParseFloat(sInput, out fValue))
            {
                return false;
            }
            Assert.AreEqual(fExpectedValue, fValue);
            return true;
        }

        [TestCase("1", 1, Result = true)]
        [TestCase("1.0", 1, Result = true)]
        [TestCase("1,0", 1, Result = true)]
        [TestCase("1.1", 1.1, Result = true)]
        [TestCase("1,1", 1.1, Result = true)]
        [TestCase("123.456", 123.456, Result = true)]
        [TestCase("123,456", 123.456, Result = true)]
        [TestCase("c", 0, Result = false)]
        public bool VerifyDecimalParse(string sInput, decimal dExpectedValue)
        {
            decimal dValue;
            if (!ParseUtil.ParseDecimal(sInput, out dValue))
            {
                return false;
            }
            Assert.AreEqual(dExpectedValue, dValue);
            return true;
        }
    }
}
