using CsvHelper.Configuration.Attributes;

namespace Analyzer;

public class ExperimentalDesign
{
    [Index(0)]
    public string FileName { get; set; }
    [Index(1)]
    public int Condition { get; set; }
    [Index(2)]
    public int BioRep { get; set; }
    [Index(3)]
    public int Fraction { get; set; }
    [Index(4)]
    public int TechRep { get; set; }
}