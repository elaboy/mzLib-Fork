using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace MassSpectrometry;
public static class RetentionTimeAlignerExtensionMethods
{
    public static void SaveAlignerResultAsTsv()
    {

    }
    public static void SaveResults(RetentionTimeAligner aligner, string path)
    {
        string jsonString = JsonSerializer.Serialize(aligner.HarmonizedSpecies);

        File.WriteAllText(path, jsonString);
    }

    public static Dictionary<string, Dictionary<string, double>> LoadResults(string path)
    {
        string jsonString = File.ReadAllText(path);
        var aligner = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(jsonString);
        return aligner;
    }
}