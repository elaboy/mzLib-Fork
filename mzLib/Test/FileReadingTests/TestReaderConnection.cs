using MassSpectrometry;
using NUnit.Framework;
using Readers;
using System.Collections.Generic;
using System.IO;

namespace Test.FileReadingTests
{
    [TestFixture]
    public sealed class TestReaderConnection
    {
        [Test]
        [Parallelizable(ParallelScope.All)]
        [TestCase("FileReadingTests/ConnectionFileTestFiles/sliced_ethcd.mzML")]
        [TestCase("FileReadingTests/ConnectionFileTestFiles/sliced_ethcd.raw")]
        [TestCase("FileReadingTests/ConnectionFileTestFiles/small.RAW")]
        //[TestCase("FileReadingTests/ConnectionFileTestFiles/SmallCalibratibleYeast.mfg")]
        [TestCase("FileReadingTests/ConnectionFileTestFiles/SmallCalibratibleYeast.mzml")]
        public void TestReaderClosesConnection(string filePath)
        {
            string spectraPath = Path.Combine(TestContext.CurrentContext.TestDirectory, filePath);
            MsDataFile datafile = MsDataFileReader.GetDataFile(spectraPath);
            List<MsDataScan> scans = datafile.GetAllScansList();
            
            datafile.Dispose();

            File.Move(spectraPath,
                Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "FileReadingTests/ConnectionFileTestFiles/ToHere/"));

            Assert.Pass();
        }
    }
}
