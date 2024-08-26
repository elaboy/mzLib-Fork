using System.Reflection.Metadata.Ecma335;
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

    public RtLib(List<string> resultsPath, string outputPath)
    {
        ResultsPath = resultsPath;
        OutputPath = outputPath;
        Results = new Dictionary<string, List<float>>();

        Task<List<IRetentionTimeAlignable>>[] dataLoader = new Task<List<IRetentionTimeAlignable>>[ResultsPath.Count];

        for (int i = 0; i < ResultsPath.Count; i++)
        {
            dataLoader[i] = (Task<List<IRetentionTimeAlignable>>)LoadFileResultsAsync(ResultsPath[i]);
            dataLoader[i].Start();
        }

        foreach (var task in dataLoader)
        {
            task.Wait();
            var fullSequences = task.Result.GroupBy(x => x.Identifier);
            if (Results.Count > 0)
            {
                foreach (var fullSequence in fullSequences)
                {
                    if (!Results.ContainsKey(fullSequence.Key))
                    {
                        Results.Add(fullSequence.Key, new List<float>());
                        Results[fullSequence.Key].AddRange(fullSequence.Select(x => x.RetentionTime));
                    }
                    else if (Results.ContainsKey(fullSequence.Key))
                    {
                        Results[fullSequence.Key].AddRange(fullSequence.Select(x => x.RetentionTime));
                    }
                }
                // Calibrate

            }
            else
            {
                // all gets inserted, it's the first file
                foreach (var fullSequence in fullSequences)
                {
                    Results.Add(fullSequence.Key, new List<float>());
                    Results[fullSequence.Key].AddRange(fullSequence.Select(x => x.RetentionTime));
                }
            }
        }
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

    public async Task LoadFileResultsAsync(string path)
    {
        var results = new Task<List<IRetentionTimeAlignable>>(() => LoadFileResults(path));
        await results;
    }

    public void Write()
    {
        throw new NotImplementedException();
    }

    public void Load()
    {
        throw new NotImplementedException();
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
            if (alignedSpecies.ContainsKey(species.Identifier))
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
            alignedSpecies.Add(predictions.Item1,
                new List<float>(){predictions.Item2.CalibratedRetentionTime});
        }
    }

    private PredictionEngine<PreCalibrated, Calibrated> GetPredictionEngine()
    {
        MLContext mlContext = new MLContext();

        List<PreCalibrated> PreCalibratedList = new();

        // Prepare the data for the dataview
        foreach (var anchor in alignedSpecies.Where(x => x.Value.Count > 1))
        {
            PreCalibratedList.Add(new PreCalibrated()
            {
                Identifier = anchor.Key,
                AnchorRetentionTime = (float)anchor.Value.Select(x => x).Median(),
                UnCalibratedRetentionTime = (float)speciesToAlign.First(x => x.Identifier == anchor.Key).RetentionTime
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
        throw new NotImplementedException();
    }
}