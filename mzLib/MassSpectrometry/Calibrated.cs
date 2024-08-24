using Microsoft.ML.Data;

namespace MassSpectrometry;
internal class Calibrated
{
    [ColumnName("Score")]
    public float CalibratedRetentionTime { get; set; }
}