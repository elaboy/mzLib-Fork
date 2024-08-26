using Microsoft.ML.Data;

namespace MassSpectrometry;
public class Calibrated
{
    [ColumnName("Score")]
    public float CalibratedRetentionTime { get; set; }
}