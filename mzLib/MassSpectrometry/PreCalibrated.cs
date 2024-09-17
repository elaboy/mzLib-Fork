using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace MassSpectrometry;
public class PreCalibrated
{
    [NoColumn]
    public string FileName { get; set; }
    [NoColumn]
    public string FullSequence { get; set; }
    [NoColumn]
    public string BaseSequence { get; set; }

    public float AnchorRetentionTime { get; set; }
    public float UnCalibratedRetentionTime { get; set; }
}