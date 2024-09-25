using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using MassSpectrometry;
using MathNet.Numerics.Statistics;
using Readers;

namespace RTLib;

public class RtLib
{
    private List<string> ResultsPath { get; }
    private string OutputPath { get; }
    public Dictionary<string, List<LightWeightPsm>> FileNamesLightWeightPsms { get; }

    public List<IRetentionTimeAlignable> AlignedPsms = new();

    public RtLib(List<string> resultsPath, string outputPath, bool useChronologer)
    {
        ResultsPath = resultsPath;
        OutputPath = outputPath;
        FileNamesLightWeightPsms = new Dictionary<string, List<LightWeightPsm>>();

        var dataLoader = Readers.FileReader.ReadFile<PsmFromTsvFile>(ResultsPath[0]);
        dataLoader.LoadResults();

        var dataLoadedAsIRetentionTimeAlignable = dataLoader
            .Where(item => item.AmbiguityLevel == "1" & item.DecoyContamTarget == "T")
            .Cast<IRetentionTimeAlignable>()
            .ToList();

        var distinctFilesWithinResults = dataLoadedAsIRetentionTimeAlignable
            .GroupBy(x => x.FileName).ToList();

        for (int i = 0; i < distinctFilesWithinResults.Count(); i++)
        {
            if (FileNamesLightWeightPsms.Count > 0)
            {
                List<IRetentionTimeAlignable> newSpecies = new();

                foreach (var file in distinctFilesWithinResults)
                {
                    foreach (var psm in file)
                    {

                        if (!FileNamesLightWeightPsms.ContainsKey(psm.FullSequence))
                        {
                            FileNamesLightWeightPsms.Add(file.Key, new List<LightWeightPsm>());
                            FileNamesLightWeightPsms[file.Key].AddRange(file
                                .Select(x => new LightWeightPsm()
                                {
                                    BaseSequence = x.BaseSequence,
                                    FileName = x.FileName,
                                    FullSequence = x.FullSequence,
                                    RetentionTime = x.RetentionTime
                                }));
                        }
                        else
                        {
                            newSpecies.Add(psm);
                        }
                    }
                }
                // Calibrate
                Aligner aligner = new Aligner(FileNamesLightWeightPsms, newSpecies);
                aligner.Align(useChronologer);

                AlignedPsms = aligner.GetResults();
                aligner.Dispose();
            }
            else
            {
                var file = distinctFilesWithinResults.First();
                FileNamesLightWeightPsms.Add(file.Key, new List<LightWeightPsm>());
                FileNamesLightWeightPsms[file.Key].AddRange(file.Select(x => new LightWeightPsm()
                {
                    BaseSequence = x.BaseSequence,
                    FileName = x.FileName,
                    FullSequence = x.FullSequence,
                    RetentionTime = x.RetentionTime
                }));

            }
            Debug.WriteLine($"file: {i} of {FileNamesLightWeightPsms.Count}");
            //dataLoader[i].Result.Clear();
        }
        WriteTsv();
    }

    public List<IRetentionTimeAlignable> LoadFileResults(string path)
    {
        var file = new Readers.PsmFromTsvFile(path);
        file.LoadResults();

        return file.Results
            .Where(item => item.AmbiguityLevel == "1" & item.DecoyContamTarget == "T")
            .Cast<IRetentionTimeAlignable>()
            .ToList();

    }

    public Task<List<IRetentionTimeAlignable>> LoadFileResultsAsync(string path)
    {
        var results = new Task<List<IRetentionTimeAlignable>>(() => LoadFileResults(path));
        return results;
    }

    public void Write()
    {
        string jsonString = JsonSerializer.Serialize(FileNamesLightWeightPsms);

        File.WriteAllText(OutputPath, jsonString);
    }

    public void WriteTsv()
    {
        var fileNameGroups = FileNamesLightWeightPsms.GroupBy(x => x.Key);
        using (var writer = new StreamWriter(OutputPath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            foreach (var file in fileNameGroups)
            {
                var fileTimeShift = file
                    .SelectMany(x => x.Value)
                    .Select(i => i.RetentionTime - i.CalibratedRetentionTime).Mean();

                writer.WriteLine($"{file.Key},{fileTimeShift}");
            }
        }

    }

    public static Dictionary<string, List<float>> Load(string rtLibPath)
    {
        string jsonString = File.ReadAllText(rtLibPath);
        var aligner = JsonSerializer.Deserialize<Dictionary<string, List<float>>>(jsonString);
        return aligner;
    }
    public List<string> GetFilePaths() => ResultsPath;
    public string GetOutputPath() => OutputPath;
}