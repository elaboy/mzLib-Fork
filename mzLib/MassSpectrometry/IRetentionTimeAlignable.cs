namespace MassSpectrometry;
public interface IRetentionTimeAlignable
{
    public string FileName { get; set; }
    public float RetentionTime { get; set; }
    public string Identifier { get; }
}