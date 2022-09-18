using System;
using System.IO;
using CardMaker;
using CardMaker.XML;
using Moq;
using NUnit.Framework;
using Support.Progress;
using UnitTest.DeckObject;

namespace UnitTest.CommandLine
{

    [TestFixture]
    internal class CommandLineTest
    {
        private const string UnitTestFolder = @"unit_test_temp\";
        private const string ProjectFile = @"CardMaker\bin\Release\net6.0-windows\Sample\sample_project.cmp";
        private const string PdfFile = "unit_test.pdf";
        private string m_sProjectRoot = string.Empty;
        private string m_sTestTemp = string.Empty;


        [SetUp]
        public void Setup()
        {
            m_sProjectRoot =
                Directory.GetParent(
                    Path.GetDirectoryName(Path.GetDirectoryName(
                        TestContext.CurrentContext.TestDirectory))).Parent.FullName;
            m_sTestTemp = Path.Combine(m_sProjectRoot, UnitTestFolder);

            if (Directory.Exists(m_sTestTemp))
            {
                Directory.Delete(m_sTestTemp, true);
            }
            Directory.CreateDirectory(m_sTestTemp);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(m_sTestTemp, true);
        }

        [Test]
        public void ExecutePDFExport()
        {
            var sProjectFile = Path.Combine(m_sProjectRoot, ProjectFile);
            var sPdfExport = Path.Combine(m_sTestTemp, PdfFile);
            var args = new string[]
            {
                "-projectPath", sProjectFile,
                "-exportFormat", "pdf",
                "-exportPath", sPdfExport
            };
            Program.MainEntry(new ConsoleProgressReporterFactory(false), args);
            Assert.IsTrue(File.Exists(sPdfExport));
        }

        [Test]
        public void ExecutePNGExport()
        {
            var sProjectFile = Path.Combine(m_sProjectRoot, ProjectFile);
            var args = new string[]
            {
                "-projectPath", sProjectFile,
                "-exportFormat", "png",
                "-exportPath", m_sTestTemp
            };
            Program.MainEntry(new ConsoleProgressReporterFactory(false), args);
            // just verifying the first layout
            AssertFileRangeExistence(m_sTestTemp, "Default_", ".png", 1, 18);
        }

        [Test]
        public void ExecuteRangePNGExport()
        {
            var sProjectFile = Path.Combine(m_sProjectRoot, ProjectFile);
            var args = new string[]
            {
                "-projectPath", sProjectFile,
                "-exportFormat", "png",
                "-exportPath", m_sTestTemp,
                "-layoutNames", "Default",
                "-cardIndices", "3-10"
            };
            Program.MainEntry(new ConsoleProgressReporterFactory(false), args);
            // just verifying the first layout
            AssertFileRangeExistence(m_sTestTemp, "Default_", ".png", 3, 10);
        }

        private void AssertFileRangeExistence(string sPath, string sPrefix, string sSuffix, int nStart, int nEnd)
        {
            var nPad = nEnd.ToString().Length;
            for (var nIdx = nStart; nIdx <= nEnd; nIdx++)
            {
                var path = Path.Combine(sPath, sPrefix + nIdx.ToString().PadLeft(nPad, '0') + sSuffix);
                Assert.IsTrue(File.Exists(path));
            }
        }
    }
}
