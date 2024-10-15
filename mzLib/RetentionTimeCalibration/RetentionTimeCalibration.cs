using MassSpectrometry;
using Readers;

namespace RetentionTimeCalibration;
public class RetentionTimeCalibration
{
    public List<PsmFromTsvFile> ResultsFiles { get; set; }
    public List<Mzml> CalibratedFiles { get; set; }

    private void GetModelPipeline()
    {
    }

    private void WriteCalibratedMzmls()
    {
    }

    private void GetAnchors()
    {
    }

}

internal class FileResultsMapper
{
    public PsmFromTsvFile Results { get; set; }
    public Mzml MzmlFile { get; set; }
    public List<Species> Species { get; set; }

    public FileResultsMapper(PsmFromTsvFile psmFromTsvFile, Mzml mzmlFile)
    {
        Results = psmFromTsvFile;
        MzmlFile = mzmlFile;
    }

    public List<Species> GetSpecies()
    {
        throw new NotImplementedException();
    }
}

internal class Species
{
    public string FullSequence { get; set; }
    public string BaseSequence { get; set; }
    public string FileName { get; set; }
    public double MsTwoRetentionTime { get; set; }
    public double PeakIntensity { get; set; }
    public double PeakRetentionTimeStart { get; set; }
    public double PeakRetentionTimeApex { get; set; }
    public double PeakRetentionTimeEnd { get; set; }
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
}

internal class FileAligner
{

}

internal class Aligner
{
}
