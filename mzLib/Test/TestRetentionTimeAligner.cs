using System;
using MassSpectrometry;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;
using Proteomics.PSM;
using Readers;

namespace Test;
internal class TestRetentionTimeAligner
{
    internal class TestRetentionTimeAlignable : MassSpectrometry.IRetentionTimeAlignable
    {
        public string FileName { get; set; }
        public double RetentionTime { get; set; }
        public string Identifier { get; set; }
    }

    private static List<IRetentionTimeAlignable> DummyTestData;
    private static int FilesInHarmonizerSize;
    private static int AllSpeciesInAllFilesSize;
    private static int HarmonizedSpeciesSize;
    private static List<IRetentionTimeAlignable> PsmTestData;


    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        //string psmPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"AlignmentTestData\HundredPsmsA549AllPsms.psmtsv");
        //PsmTestData = SpectrumMatchTsvReader.ReadPsmTsv(psmPath, out _)/*.Select(psm => (IRetentionTimeAlignable)psm)*/
        //    .Cast<IRetentionTimeAlignable>().ToList();


        DummyTestData = new List<IRetentionTimeAlignable>()
        {
            new TestRetentionTimeAlignable()
            {
                FileName = "file1",
                Identifier = "peptide1",
                RetentionTime = 1
            },
            new TestRetentionTimeAlignable()
            {
                FileName = "file2",
                Identifier = "peptide1",
                RetentionTime = 1
            },
            new TestRetentionTimeAlignable()
            {
                FileName = "file3",
                Identifier = "peptide1",
                RetentionTime = 1
            },
            new TestRetentionTimeAlignable()
            {
                FileName = "file2",
                Identifier = "peptide2",
                RetentionTime = 2
            }
        };

        FilesInHarmonizerSize = DummyTestData.DistinctBy(x => x.FileName).ToList().Count;
        AllSpeciesInAllFilesSize = DummyTestData.Count;
        HarmonizedSpeciesSize = DummyTestData.DistinctBy(x => x.Identifier).ToList().Count;
    }

    [Test]
    public void TestAlignerDummyData()
    {
        var testData = DummyTestData;

        RetentionTimeAligner aligner = new RetentionTimeAligner(testData);

        Assert.That(aligner.FilesInHarmonizer.Count == FilesInHarmonizerSize);
        Assert.That(aligner.AllSpeciesInAllFiles.Count == AllSpeciesInAllFilesSize);
        Assert.That(aligner.HarmonizedSpecies.Count == HarmonizedSpeciesSize);
    }

    [Test]
    public void TestAlignerEmptyData()
    {
        var emptyList = new List<IRetentionTimeAlignable>();
        Assert.Throws<InvalidOperationException>(() => new RetentionTimeAligner(emptyList));
    }

    [Test]
    public void TestCalibrate()
    {

    }

    [Test]
    public void TestExtensionMethods()
    {
        var testData = DummyTestData;

        RetentionTimeAligner aligner = new RetentionTimeAligner(testData);

        //Save dictionary
        RetentionTimeAlignerExtensionMethods.SaveResults(aligner, "alignerDictionary.json");

        //load aligner dictionary
        Dictionary<string, Dictionary<string, double>> loadAligner = RetentionTimeAlignerExtensionMethods.LoadResults(
            Path.Combine(
                TestContext.CurrentContext.TestDirectory, "alignerDictionary.json"));

        Assert.That(aligner.HarmonizedSpecies.Keys.Count.Equals(loadAligner.Keys.Count));

        // Remove file from test directory
        File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "alignerDictionary.json"));
    }

    //[Test]
    //public void TestAlignerPsmData()
    //{
    //    var filterToAvoidDuplicates = PsmTestData.GroupBy(x => x.FileName).ToList();

    //    List<IRetentionTimeAlignable> filteredPsms = new();

    //    foreach (var file in filterToAvoidDuplicates)
    //    {
    //        var identifierGrouped = file.GroupBy(x => x.Identifier).ToList();
    //        foreach (var sequence in identifierGrouped)
    //        {
    //            filteredPsms.Add(new TestRetentionTimeAlignable()
    //            {
    //                FileName = file.Key,
    //                Identifier = sequence.Key,
    //                RetentionTime = sequence.Select(x => x.RetentionTime).Mean()
    //            });
    //        }
    //    }

    //    RetentionTimeAligner aligner = new RetentionTimeAligner(filteredPsms);

    //    Assert.That(aligner.FilesInHarmonizer.Count == FilesInHarmonizerSize);
    //    Assert.That(aligner.AllSpeciesInAllFiles.Count == AllSpeciesInAllFilesSize);
    //    Assert.That(aligner.HarmonizedSpecies.Count == HarmonizedSpeciesSize);

    //    // Test the deep copy method

    //}
}

