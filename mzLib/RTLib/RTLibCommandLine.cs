using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Easy.Common.Extensions;
using MassSpectrometry;
using MathNet.Numerics.Statistics;
using Microsoft.ML;
using Nett;
using Proteomics.RetentionTimePrediction.Chronologer;
using Readers;
using TorchSharp;

namespace RTLib;
public class RtLibCommandLine
{
    public static void Main(string[] args)
    {
        (List<string> filePaths, string outputPath) = CommandLineParser(args);

        RtLib rtLib = new RtLib(filePaths, outputPath, false);
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

public class LightPsm : IRetentionTimeAlignable
{
    public string FileName { get; set; }
    public float RetentionTime { get; set; }
    public float ChronologerHI { get; set; }
    public string BaseSequence { get; set; }
    public string FullSequence { get; set; }
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

    public RtLib(List<string> resultsPath, string outputPath, bool useChronologer)
    {
        ResultsPath = resultsPath;
        OutputPath = outputPath;
        Results = new Dictionary<string, List<float>>();

        Task<List<LightPsm>>[] dataLoader = new Task<List<LightPsm>>[ResultsPath.Count];

        for (int i = 0; i < ResultsPath.Count; i++)
        {
            dataLoader[i] = LoadFileResultsAsync(ResultsPath[i]);
        }
        dataLoader[0].Start();

        for(int i = 0; i < ResultsPath.Count; i++) 
        {
            dataLoader[i].Wait();
            
            // do we want to use the chronologer HI instead of the scan reported retention time 
            if (useChronologer)
            {
                string[] baseSequencesArray = dataLoader[i].Result.Select(x => x.BaseSequence).ToArray();
                string[] fullSequencesArray = dataLoader[i].Result.Select(x => x.FullSequence).ToArray();
                var predictions = ChronologerEstimator.PredictRetentionTime(baseSequencesArray, fullSequencesArray, true);
                Parallel.For(0, predictions.Length, predictionIndex =>
                {
                    dataLoader[i].Result[predictionIndex].ChronologerHI = predictions[predictionIndex].IsDefault()
                        ? -1
                        : predictions[predictionIndex];
                });
            }
            var fullSequences = dataLoader[i].Result.GroupBy(x => x.FullSequence);
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
                // Start next file loading task
                if (i < ResultsPath.Count - 1)
                {
                    dataLoader[i+1].Start();
                }
                aligner.Align(useChronologer);

                Results = aligner.GetResults();
                aligner.Dispose();
                dataLoader[i].Dispose();
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
                dataLoader[i].Dispose();
            }
            Debug.WriteLine($"file: {i} of {ResultsPath.Count}");
            //dataLoader[i].Result.Clear();
        }
        Write();
    }

    public List<LightPsm> LoadFileResults(string path)
    {
        var file = new Readers.PsmFromTsvFile(path);
        file.LoadResults();
        
        var results =  file.Results
                .Where(item => item.AmbiguityLevel == "1")
                .ToList();
        
        List<LightPsm> lightPsms = new List<LightPsm>();
        foreach (var item in results)
        {
            lightPsms.Add(new LightPsm(){BaseSequence = item.BaseSequence,
                ChronologerHI = item.ChronologerHI, 
                FileName = item.FileName, 
                FullSequence = item.FullSequence, 
                RetentionTime = (float)item.RetentionTime.Value});
        }

        return lightPsms;

    }

    public Task<List<LightPsm>> LoadFileResultsAsync(string path)
    {
        var results = new Task<List<LightPsm>>(() => LoadFileResults(path));
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

    public void Align(bool useChronologer)
    {
        // novel species to predict RT
        List<PreCalibrated> toPredict = new();

        // new inserts

        if (useChronologer)
        {
            foreach (var species in speciesToAlign)
            {
                if (alignedSpecies.ContainsKey(species.FullSequence) & alignedSpecies[species.FullSequence].Count > 1)
                {
                    alignedSpecies[species.FullSequence].Add(species.RetentionTime);
                }
                else
                {
                    toPredict.Add(new PreCalibrated()
                    {
                        FullSequence = species.FullSequence,
                        UnCalibratedRetentionTime = species.ChronologerHI
                    });
                }
            }
        }
        else
        {
            foreach (var species in speciesToAlign)
            {
                if (alignedSpecies.ContainsKey(species.FullSequence) & alignedSpecies[species.FullSequence].Count > 1)
                {
                    alignedSpecies[species.FullSequence].Add(species.RetentionTime);
                }
                else
                {
                    toPredict.Add(new PreCalibrated()
                    {
                        FullSequence = species.FullSequence,
                        UnCalibratedRetentionTime = species.RetentionTime
                    });
                }
            }
        }


        (string, Calibrated)[] calibrated = new (string, Calibrated)[toPredict.Count];

        var predictionEngine = GetPredictionEngine(useChronologer);

        Parallel.For(0, calibrated.Length, i =>
        {
            calibrated[i] = (toPredict[i].FullSequence, predictionEngine.Predict(toPredict[i]));
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

    private PredictionEngine<PreCalibrated, Calibrated> GetPredictionEngine(bool useChronologer)
    {
        MLContext mlContext = new MLContext();

        List<PreCalibrated> PreCalibratedList = new();

        // need to separate the anchors from the novel species to the library, the next loop crashes. Need to bring the Intersects lines from previos code iteration (it worked)
        // get anchor available
        var anchorsBetweenBothSets = speciesToAlign.Where(x => alignedSpecies.ContainsKey(x.FullSequence)).DistinctBy(x => x.FullSequence);

        // Prepare the data for the dataview
        if (useChronologer)
        {
            foreach (var anchor in anchorsBetweenBothSets)
            {
                PreCalibratedList.Add(new PreCalibrated()
                {
                    FullSequence = anchor.FullSequence,
                    AnchorRetentionTime = alignedSpecies[anchor.FullSequence].Select(x => x).Median(),
                    UnCalibratedRetentionTime = anchor.ChronologerHI
                });
            }
        }
        else
        {
            foreach (var anchor in anchorsBetweenBothSets)
            {
                PreCalibratedList.Add(new PreCalibrated()
                {
                    FullSequence = anchor.FullSequence,
                    AnchorRetentionTime = alignedSpecies[anchor.FullSequence].Select(x => x).Median(),
                    UnCalibratedRetentionTime = anchor.RetentionTime
                });
            }
        }

        var dataView = mlContext.Data.LoadFromEnumerable(PreCalibratedList.Where(x => x.UnCalibratedRetentionTime > -1).ToArray());

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