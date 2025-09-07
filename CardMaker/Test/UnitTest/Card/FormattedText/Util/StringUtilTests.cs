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
