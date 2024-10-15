using MassSpectrometry;
using Microsoft.ML;
using Microsoft.ML.Data;
using Readers;
using System.Data;
using Readers.QuantificationResults;

namespace RetentionTimeCalibration;
public class RetentionTimeCalibration
{
    public List<FileResultsMapper> ResultsFiles { get; set; }
    public List<Species> CalibratedSpecies { get; set; }
    public List<Mzml> CalibratedFiles { get; set; }

    public RetentionTimeCalibration(List<(
        string peptidesResultsFilePath, 
        string quantifiedPeakFilePath, 
        string mzmlPath)> resultsFilesPath)
    {
        FileResultsMapper[] resultsFiles = new FileResultsMapper[resultsFilesPath.Count];

        Parallel.For(0, resultsFilesPath.Count, i =>
        {
            PsmFromTsvFile psmFromTsvFile = 
                new PsmFromTsvFile(resultsFilesPath[i].peptidesResultsFilePath);
            psmFromTsvFile.LoadResults();

            Mzml mzmlFile = new Mzml(resultsFilesPath[i].mzmlPath);
            mzmlFile.LoadAllStaticData();

            resultsFiles[i] = new FileResultsMapper(psmFromTsvFile, mzmlFile);
            FileResultsMapper mapper = new FileResultsMapper(psmFromTsvFile, mzmlFile);

        });

        ResultsFiles = resultsFiles.ToList();
    }
    private void WriteCalibratedMzmls()
    {
        throw new NotImplementedException();
    }

    private List<Species> GetAnchors()
    {
        var allSpecies = ResultsFiles.SelectMany(x => x.Species);
        List<Species> anchorSpecies =
            allSpecies.GroupBy(s => new { s.FullSequence, s.BaseSequence })
                .Where(g => g.Count() == ResultsFiles.Count)
                .Select(g => g.First())
                .ToList();

        return anchorSpecies;
    }

    private List<Species> SpeciesToPredict(List<Species> anchors)
    {
        var allSpecies = ResultsFiles.SelectMany(x => x.Species);
        List<Species> speciesToPredict = allSpecies.Except(anchors, new SpeciesComparer()).ToList();

        return speciesToPredict;
    }
}

public class SpeciesComparer : IEqualityComparer<Species>
{
    public bool Equals(Species x, Species y)
    {
        return x.FullSequence == y.FullSequence && x.BaseSequence == y.BaseSequence;
    }

    public int GetHashCode(Species obj)
    {
        return HashCode.Combine(obj.FullSequence, obj.BaseSequence);
    }
}

public class FileResultsMapper
{
    public PsmFromTsvFile Results { get; set; }
    public Mzml MzmlFile { get; set; }
    public List<Species> Species { get; set; }

    public FileResultsMapper(PsmFromTsvFile psmFromTsvFile, Mzml mzmlFile)
    {
        Results = psmFromTsvFile;
        MzmlFile = mzmlFile;
        Species = GetSpecies();
    }

    public List<Species> GetSpecies()
    {
        List<Species> species = new List<Species>();

        foreach (var psm in Results.Results)
        {
            MsDataScan msDataScan = MzmlFile.GetOneBasedScan(psm.PrecursorScanNum);
            Species newSpecies = new Species(
                psm.FullSequence,
                psm.BaseSeq,
                psm.FileNameWithoutExtension,
                psm.RetentionTime.Value, 
                msDataScan);

            species.Add(newSpecies);
        }

        return species;
    }
}


public static class Aligner
{
    public static PredictionEngine<PreCalibrated, Calibrated> GetOLSPredictionEngine(List<Species> anchors)
    {
        MLContext mlContext = new MLContext();

        List<PreCalibrated> preCalibratedAnchors = anchors.Select(x => new PreCalibrated()
        {
            AnchorRetentionTime = (float)x.ScanRetentionTime,
            BaseSequence = x.BaseSequence,
            FullSequence = x.FullSequence
        }).ToList();


        IDataView? dataView = mlContext.Data.LoadFromEnumerable(preCalibratedAnchors);

        var pipeline = mlContext.Transforms.CopyColumns(
                "Label",
                nameof(PreCalibrated.AnchorRetentionTime))
            .Append(mlContext.Transforms
                .Concatenate("Features", nameof(PreCalibrated.UnCalibratedRetentionTime)))
            .Append(mlContext.Regression.Trainers.Ols("Label", "Features"));

        var model = pipeline.Fit(dataView);

        return mlContext.Model.CreatePredictionEngine<PreCalibrated, Calibrated>(model);
    }

    //public static List<Species> PredictSpeciesRetentionTime(
    //    PredictionEngine<PreCalibrated, Calibrated> predictionEngine, List<Species> speciesToPredictRT)
    //{

    //}
}

public class PreCalibrated
{
    [NoColumn]
    public string FullSequence { get; set; }
    [NoColumn]
    public string BaseSequence { get; set; }

    public float AnchorRetentionTime { get; set; }
    public float UnCalibratedRetentionTime { get; set; }
}

public class Calibrated
{
    [ColumnName("Score")]
    public float CalibratedRetentionTime { get; set; }
}

public class Species
{
    public string FullSequence { get; set; }
    public string BaseSequence { get; set; }
    public string FileName { get; set; }
    public double? MsTwoRetentionTime { get; set; }
    public double? PeakIntensity { get; set; }
    public double? PeakRetentionTimeStart { get; set; }
    public double? PeakRetentionTimeApex { get; set; }
    public double? PeakRetentionTimeEnd { get; set; }
    public double ScanRetentionTime { get; set; }
    public MsDataScan MsDataScan { get; set; }

    public Species(string fileName, string baseSequence,
        string fullSequence, double scanRetentionTime, MsDataScan msDataScan,
        double msTwoRetentionTime, double peakIntensity, double peakRetentionTimeStart,
        double peakRetentionTimeApex, double peakRetentionTimeEnd)
    {
        FullSequence = fullSequence;
        BaseSequence = baseSequence;
        FileName = fileName;
        ScanRetentionTime = scanRetentionTime;
        MsDataScan = msDataScan;
        MsTwoRetentionTime = msTwoRetentionTime;
        PeakIntensity = peakIntensity;
        PeakRetentionTimeStart = peakRetentionTimeStart;
        PeakRetentionTimeApex = peakRetentionTimeApex;
        PeakRetentionTimeEnd = peakRetentionTimeEnd;
        ScanRetentionTime = scanRetentionTime;
    }
    public Species(string fileName, string baseSequence,
        string fullSequence, double scanRetentionTime, MsDataScan msDataScan)
    {
        FullSequence = fullSequence;
        BaseSequence = baseSequence;
        FileName = fileName;
        ScanRetentionTime = scanRetentionTime;
        MsDataScan = msDataScan;
        ScanRetentionTime = scanRetentionTime;
    }
}

internal class FileAligner
{

}
