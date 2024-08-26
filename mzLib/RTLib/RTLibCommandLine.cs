using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using MassSpectrometry;
using MathNet.Numerics.Statistics;
using Microsoft.ML;
using Nett;

namespace RTLib;
public class RtLibCommandLine
{
    public static void Main(string[] args)
    {
        (List<string> filePaths, string outputPath) = CommandLineParser(args);

        RtLib rtLib = new RtLib(filePaths, outputPath);
    }

    private static (List<string> filePaths, string outputPath) CommandLineParser(string[] args)
    {
        List<string> filePaths = new List<string>();
        string outputPath = "";

        var argument = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].StartsWith("-"))
            {
                argument = args[i];
            }
            else
            {
                switch (argument)
                {
                    case "--files":
                        filePaths.Add(args[i]);
                        break;
                    case "--output":
                        outputPath = args[i];
                        break;
                    default:
                        Console.WriteLine("Argument error: this is not an option in the program. Revise your input.");
                        break;
                }
            }
        }

        return (filePaths, outputPath);
    }

}

public class RtLib
{
    private List<string> ResultsPath { get; }
    private string OutputPath { get;}
    public Dictionary<string, List<float>> Results { get; }

    public RtLib(string rtLibPath)
    {
        Results = Load(rtLibPath);
    }

    public RtLib(List<string> resultsPath, string outputPath)
    {
        ResultsPath = resultsPath;
        OutputPath = outputPath;
        Results = new Dictionary<string, List<float>>();

        Task<List<IRetentionTimeAlignable>>[] dataLoader = new Task<List<IRetentionTimeAlignable>>[ResultsPath.Count];

        for (int i = 0; i < ResultsPath.Count; i++)
        {
            dataLoader[i] = LoadFileResultsAsync(ResultsPath[i]);
        }
        dataLoader[0].Start();

        for(int i = 0; i < ResultsPath.Count; i++) 
        {
            dataLoader[i].Wait();
            var fullSequences = dataLoader[i].Result.GroupBy(x => x.Identifier);
            if (Results.Count > 0)
            {
                List<IRetentionTimeAlignable> newSpecies = new();

                foreach (var fullSequence in fullSequences)
                {
                    if (!Results.ContainsKey(fullSequence.Key))
                    {
                        Results.Add(fullSequence.Key, new List<float>());
                        Results[fullSequence.Key].AddRange(fullSequence.Select(x => x.RetentionTime));
                    }
                    else if (Results.ContainsKey(fullSequence.Key))
                    {
                        foreach (var species in fullSequence)
                        {
                            newSpecies.Add(species);
                        }
                    }
                }
                // Calibrate
                Aligner aligner = new Aligner(Results, newSpecies);
                // Start next loading task
                if (i < ResultsPath.Count - 1)
                {
                    dataLoader[i+1].Start();
                }
                aligner.Align();

                Results = aligner.GetResults();
                aligner.Dispose();
            }
            else
            {
                if (i < ResultsPath.Count - 1)
                {
                    dataLoader[i + 1].Start();
                }
                // all gets inserted, it's the first file
                foreach (var fullSequence in fullSequences)
                {
                    Results.Add(fullSequence.Key, new List<float>());
                    Results[fullSequence.Key].AddRange(fullSequence.Select(x => x.RetentionTime));
                }
            }
            Debug.WriteLine($"file: {i} of {Results.Count}");
            //dataLoader[i].Result.Clear();
        }
        Write();
    }

    public List<IRetentionTimeAlignable> LoadFileResults(string path)
    {
        var file = new Readers.PsmFromTsvFile(path);
        file.LoadResults();

        return file.Results
            .Where(item => item.AmbiguityLevel == "1")
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
        string jsonString = JsonSerializer.Serialize(Results);

        File.WriteAllText(OutputPath, jsonString);
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

public class Aligner : IDisposable
{
    private Dictionary<string, List<float>> alignedSpecies { get; }
    private List<IRetentionTimeAlignable> speciesToAlign { get; }
    public Aligner(Dictionary< string, List<float>> speciesAligned, List<IRetentionTimeAlignable> setOfSpeciesToAlign)
    {
        alignedSpecies = speciesAligned;
        speciesToAlign = setOfSpeciesToAlign;
    }

    public void Align()
    {
        // novel species to predict RT
        List<PreCalibrated> toPredict = new();

        // new inserts
        foreach (var species in speciesToAlign)
        {
            if (alignedSpecies.ContainsKey(species.Identifier) & alignedSpecies[species.Identifier].Count > 1)
            {
                alignedSpecies[species.Identifier].Add(species.RetentionTime);
            }
            else
            {
                toPredict.Add(new PreCalibrated()
                {
                    Identifier = species.Identifier,
                    UnCalibratedRetentionTime = species.RetentionTime
                });
            }
        }

        (string, Calibrated)[] calibrated = new (string, Calibrated)[toPredict.Count];

        var predictionEngine = GetPredictionEngine();

        Parallel.For(0, calibrated.Length, i =>
        {
            calibrated[i] = (toPredict[i].Identifier, predictionEngine.Predict(toPredict[i]));
        });

        // add them into the alignedSpecies
        foreach (var predictions in calibrated)
        {
            if (alignedSpecies.ContainsKey(predictions.Item1))
            {
                alignedSpecies[predictions.Item1].Add(predictions.Item2.CalibratedRetentionTime);
            }
            else
            {
                alignedSpecies.Add(predictions.Item1,
                    new List<float>() { predictions.Item2.CalibratedRetentionTime });
            }
        }
        Dispose();
    }

    private PredictionEngine<PreCalibrated, Calibrated> GetPredictionEngine()
    {
        MLContext mlContext = new MLContext();

        List<PreCalibrated> PreCalibratedList = new();

        // need to separate the anchors from the novel species to the library, the next loop crashes. Need to bring the Intersects lines from previos code iteration (it worked)
        // get anchor available
        var anchorsBetweenBothSets = speciesToAlign.Where(x => alignedSpecies.ContainsKey(x.Identifier)).DistinctBy(x => x.Identifier);

        // Prepare the data for the dataview
        foreach (var anchor in anchorsBetweenBothSets)
        {
            PreCalibratedList.Add(new PreCalibrated()
            {
                Identifier = anchor.Identifier,
                AnchorRetentionTime = alignedSpecies[anchor.Identifier].Select(x => x).Median(),
                UnCalibratedRetentionTime = anchor.RetentionTime
            });
        }

        var dataView = mlContext.Data.LoadFromEnumerable(PreCalibratedList.ToArray());

        // Make the model pipeline
        var pipeline = mlContext.Transforms
            .CopyColumns("Label",
                nameof(PreCalibrated.AnchorRetentionTime))
            .Append(mlContext.Transforms
                .Concatenate("Features",
                    nameof(PreCalibrated.UnCalibratedRetentionTime)))
            .Append(mlContext.Regression.Trainers.Ols("Label", "Features"));

        // train the model
        var model = pipeline.Fit(dataView);

        // makes the prediction engine to predict the follower retention times
        var predictionEngine = mlContext.Model.CreatePredictionEngine<PreCalibrated, Calibrated>(model);

        return predictionEngine;
    }

    public Dictionary<string, List<float>> GetResults() => alignedSpecies;

    public void Dispose()
    {
        alignedSpecies.Clear();
        speciesToAlign.Clear();
    }
}