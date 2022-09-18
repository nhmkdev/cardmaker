using System.Linq;
using NUnit.Framework;
using Support.IO;

namespace UnitTest.Support.IO
{
    [TestFixture]
    class CSVFileTest
    {
        [TestCase("1", new string[] {"1"})]
        [TestCase("\"1\"", new string[] { "1" })]
        [TestCase("1,\"1\"", new string[] { "1", "1" })]
        [TestCase("\"\"", new string[] { "" })]
        [TestCase("\"\"\"\"", new string[] { "\"" })]
        [TestCase("\"a\"\"\"", new string[] { "a\"" })]
        [TestCase("\"a\"\"z\"", new string[] { "a\"z" })]
        public void testParse(string input, string[] expectedEntries)
        {
            var csvFile = new CSVFile();
            csvFile.ProcessCSVLines(new string[] { input });
            Assert.AreEqual(expectedEntries.ToList(), csvFile.GetRow(0));
        }
    }
}
