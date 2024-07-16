using MassSpectrometry;
using NUnit.Framework;
using System.Collections.Generic;

namespace Test;
internal class TestRetentionTimeAligner
{
    internal class TestRetentionTimeAlignable : MassSpectrometry.IRetentionTimeAlignable
    {
        public string FileName { get; set; }
        public double RetentionTime { get; set; }
        public string Identifier { get; set; }
    }

    #region Generate Dummy Data
    static TestRetentionTimeAlignable Example1 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile1.psmtsv",
        Identifier = "sumPeptide1",
        RetentionTime = 1
    };

    static TestRetentionTimeAlignable Example1_copy = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile1_copy.psmtsv",
        Identifier = "sumPeptide1_copy",
        RetentionTime = 1
    };

    static TestRetentionTimeAlignable Example2 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile2.psmtsv",
        Identifier = "sumPeptide2",
        RetentionTime = 2
    };

    static TestRetentionTimeAlignable Example3 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile3.psmtsv",
        Identifier = "sumPeptide3",
        RetentionTime = 3
    };

    static TestRetentionTimeAlignable Example4 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile4.psmtsv",
        Identifier = "sumPeptide4",
        RetentionTime = 4
    };

    static TestRetentionTimeAlignable Example5 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile5.psmtsv",
        Identifier = "sumPeptide5",
        RetentionTime = 5
    };

    static TestRetentionTimeAlignable Example6 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile6.psmtsv",
        Identifier = "sumPeptide6",
        RetentionTime = 6
    };

    static TestRetentionTimeAlignable Example7 = new TestRetentionTimeAlignable()
    {
        FileName = "sumFile7.psmtsv",
        Identifier = "sumPeptide7",
        RetentionTime = 7
    };

    #endregion

    [Test]
    public void TestConstructor()
    {
        List<IRetentionTimeAlignable> psms = new List<IRetentionTimeAlignable>();
        psms.AddRange(new IRetentionTimeAlignable[]
        {
            Example1, Example2, Example3, Example4,
            Example5, Example6, Example7
        });

        RetentionTimeAligner aligner = new RetentionTimeAligner(psms);

        Assert.That(aligner.FilesInHarmonizer.Count == 7);
    }
}

