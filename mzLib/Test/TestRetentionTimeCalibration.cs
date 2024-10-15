using System.Collections.Generic;
using System.Linq;
using Easy.Common.Extensions;
using NUnit.Framework;

namespace Test;

[TestFixture]
public class TestRetentionTimeCalibration
{
    [Test]
    public void TestRetentionTimeCalibrationConstructor()
    {
        // Arrange
        string peptidesResultsFilePath = @"E:\DatasetsForRT\Mann_11cell_lines\A549\2024-07-10-15-53-11_ClassigBigSearch\Task4-SearchTask\Individual File Results\20100604_Velos1_TaGe_SA_A549_1-calib-averaged_Peptides.psmtsv";
        string quantifiedPeakFilePath = @"E:\\DatasetsForRT\\Mann_11cell_lines\\A549\\2024-07-10-15-53-11_ClassigBigSearch\\Task4-SearchTask\\Individual File Results\\20100604_Velos1_TaGe_SA_A549_1-calib-averaged_QuantifiedPeaks.tsv\";
        string mzmlPath = @"E:\DatasetsForRT\Mann_11cell_lines\A549\2024-07-10-15-53-11_ClassigBigSearch\Task1-CalibrateTask\20100604_Velos1_TaGe_SA_A549_1-calib.mzML";
        List<(string, string, string)> resultsFilesPath = new List<(string, string, string)> { (peptidesResultsFilePath, quantifiedPeakFilePath, mzmlPath) };

        // Act
        RetentionTimeCalibration.RetentionTimeCalibration retentionTimeCalibration = 
            new RetentionTimeCalibration.RetentionTimeCalibration(resultsFilesPath);

        // Assert
        Assert.That(retentionTimeCalibration.ResultsFiles.Count == 1);
        Assert.That(retentionTimeCalibration.ResultsFiles.First().Species.Count > 0);
        Assert.That(retentionTimeCalibration.ResultsFiles.First().MzmlFile.IsNotNullOrEmpty());
        Assert.That(retentionTimeCalibration.ResultsFiles.First().Results.Results.Count > 0);
    }
}