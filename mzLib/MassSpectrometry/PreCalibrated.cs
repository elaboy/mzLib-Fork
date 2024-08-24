using Microsoft.ML.Data;

namespace MassSpectrometry;
public class PreCalibrated
{
    [NoColumn]
    public string Identifier { get; set; }
    public float AnchorRetentionTime { get; set; }
    public float UnCalibratedRetentionTime { get; set; }
}