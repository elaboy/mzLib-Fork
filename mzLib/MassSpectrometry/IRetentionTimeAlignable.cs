namespace MassSpectrometry;
public interface IRetentionTimeAlignable
{
    public string FileName { get; set; }
    public double RetentionTime { get; set; }
    public string Identifier { get; }
}
