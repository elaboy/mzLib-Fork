using Readers;

namespace RetentionTimeCalibration;
public class RetentionTimeCalibration
{
    public List<PsmFromTsvFile> ResultsFiles { get; set; }
    public List<PsmFromTsvFile> CalibratedFiles { get; set; }

}

internal class FileResultsMapper
{
    public PsmFromTsvFile Results { get; set; }
    public Mzml MzmlFile { get; set; }

    public FileResultsMapper(PsmFromTsvFile psmFromTsvFile, Mzml mzmlFile)
    {
        Results = psmFromTsvFile;
        MzmlFile = mzmlFile;
    }
}

internal class FileAligner
{

}

internal class Aligner
{
}
