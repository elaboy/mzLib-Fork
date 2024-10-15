using MassSpectrometry;
using Microsoft.ML;
using Microsoft.ML.Data;
using Readers;
using System.Data;

namespace RetentionTimeCalibration;
public class RetentionTimeCalibration
{
    public Dictionary<MsDataFile, List<(string, double)>> ResultsFiles { get; set; }
    public List<Species> CalibratedSpecies { get; set; } 
    public List<Mzml> CalibratedFiles { get; set; }

    public RetentionTimeCalibration(MsDataFile[] dataFiles, Dictionary<string, List<string>> fullSequences,
        Dictionary<string, List<double>> retentionTimes)
    {
        Dictionary<MsDataFile, List<(string, double)>> resultsFiles = new Dictionary<MsDataFile, List<(string, double)>>();

        for (int i = 0; i < dataFiles.Length; i++)
        {
            resultsFiles[dataFiles[i]] = new List<(string, double)>();

            // zip fullSequence list and the retentionTime list to end up with a list of tuples[fullSequence, retentionTime]
            var fullSequencesAndRetentionTimes = fullSequences[dataFiles[i].FilePath]
                .Zip(retentionTimes[dataFiles[i].FilePath],
                    (fullSequence, retentionTime) => (fullSequence, retentionTime));

            resultsFiles[dataFiles[i]].AddRange(fullSequencesAndRetentionTimes);
        }

        ResultsFiles = resultsFiles;
    }
    private void WriteCalibratedMzmls()
    {

        throw new NotImplementedException();
    }

    public List<Species> GetAnchors()
    {
        var allSpecies = ResultsFiles.SelectMany(x => x.Value.Select(tuple => new Species
        {
            FileName = x.Key.FilePath,
            FullSequence = tuple.Item1,
            ScanRetentionTime = tuple.Item2,
            MsDataScan = x.Key.GetOneBasedScan(x.Key.GetClosestOneBasedSpectrumNumber(tuple.Item2))
        })).ToList();

        List<Species> anchorSpecies =
            allSpecies.GroupBy(s => s.FullSequence)
                .Where(g => g.Count() == ResultsFiles.Count)
                .Select(g => g.First())
                .ToList();

        return anchorSpecies;
    }

    private List<Species> SpeciesToPredict(List<Species> anchors)
    {
        var allSpecies = ResultsFiles.SelectMany(x => x.Value.Select(tuple => new Species
        {
            FileName = x.Key.FilePath,
            FullSequence = tuple.Item1,
            ScanRetentionTime = tuple.Item2,
            MsDataScan = x.Key.GetOneBasedScan(x.Key.GetClosestOneBasedSpectrumNumber(tuple.Item2))
        })).ToList();

        List<Species> speciesToPredict = allSpecies.Except(anchors, new SpeciesComparer()).ToList();

        return speciesToPredict;
    }
}

public class SpeciesComparer : IEqualityComparer<Species>
{
    public bool Equals(Species x, Species y)
    {
        return x.FullSequence == y.FullSequence;
    }

    public int GetHashCode(Species obj)
    {
        return HashCode.Combine(obj.FullSequence);
    }
}

//public class FileResultsMapper
//{
//    public PsmFromTsvFile Results { get; set; }
//    public Mzml MzmlFile { get; set; }
//    public List<Species> Species { get; set; }

//    public FileResultsMapper(PsmFromTsvFile psmFromTsvFile, Mzml mzmlFile)
//    {
//        Results = psmFromTsvFile;
//        MzmlFile = mzmlFile;
//        Species = GetSpecies();
//    }

//    public List<Species> GetSpecies()
//    {
//        List<Species> species = new List<Species>();

//        foreach (var psm in Results.Results)
//        {
//            MsDataScan msDataScan = MzmlFile.GetOneBasedScan(psm.PrecursorScanNum);
//            Species newSpecies = new Species(
//                psm.FullSequence,
//                psm.BaseSeq,
//                psm.FileNameWithoutExtension,
//                psm.RetentionTime.Value,
//                msDataScan);

//            species.Add(newSpecies);
//        }

//        return species;
//    }
//}


public static class Aligner
{
    public static PredictionEngine<PreCalibrated, Calibrated> GetOLSPredictionEngine(List<Species> anchors)
    {
        MLContext mlContext = new MLContext();

        List<PreCalibrated> preCalibratedAnchors = anchors.Select(x => new PreCalibrated()
        {
            AnchorRetentionTime = (float)x.ScanRetentionTime,
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
    public string FileName { get; set; }
    public double ScanRetentionTime { get; set; }
    public MsDataScan MsDataScan { get; set; }

    public Species()
    {
        
    }

    public Species(string fileName, string fullSequence, double scanRetentionTime, MsDataScan msDataScan)
    {
        FullSequence = fullSequence;
        FileName = fileName;
        ScanRetentionTime = scanRetentionTime;
        MsDataScan = msDataScan;
        ScanRetentionTime = scanRetentionTime;
    }
}

internal class FileAligner
{

}
