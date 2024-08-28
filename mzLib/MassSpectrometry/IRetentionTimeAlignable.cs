namespace MassSpectrometry;
public interface IRetentionTimeAlignable
{
    public string FileName { get; set; }
    public float RetentionTime { get; set; }
    public float ChronologerHI { get; set; }
    public string BaseSequence { get; set; }
    public string FullSequence { get; set; }
}