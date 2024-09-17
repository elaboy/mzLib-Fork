using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using CsvHelper;
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

public class LightWeightPsm : IRetentionTimeAlignable
{
    public string FileName { get; set; }
    public float RetentionTime { get; set; }
    public float CalibratedRetentionTime { get; set; }
    public float ChronologerHI { get; set; }
    public string BaseSequence { get; set; }
    public string FullSequence { get; set; }
}

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

public class Aligner : IDisposable
{
    private Dictionary<string, List<LightWeightPsm>> _alignedSpecies { get; }
    private List<IRetentionTimeAlignable> _speciesToAlign { get; }
    public Aligner(Dictionary<string, List<LightWeightPsm>> speciesAligned, List<IRetentionTimeAlignable> setOfSpeciesToAlign)
    {
        _alignedSpecies = speciesAligned;
        _speciesToAlign = setOfSpeciesToAlign;
    }

    public void Align(bool useChronologer)
    {
        // novel species to predict RT
        List<PreCalibrated> toPredict = new();

        // new inserts

        if (useChronologer)
        {
            foreach (var species in _speciesToAlign)
            {
                if (_alignedSpecies.ContainsKey(species.FullSequence) & _alignedSpecies[species.FullSequence].Count > 1)
                {
                    _alignedSpecies[species.FullSequence].Add(new LightWeightPsm()
                    {
                        FullSequence = species.FullSequence,
                        BaseSequence = species.BaseSequence,
                        FileName = species.FileName,
                        RetentionTime = species.RetentionTime
                    });
                }
                else
                {
                    toPredict.Add(new PreCalibrated()
                    {
                        FileName = species.FileName,
                        FullSequence = species.FullSequence,
                        BaseSequence = species.BaseSequence,
                        UnCalibratedRetentionTime = species.RetentionTime
                    });
                }
            }
        }
        else
        {
            foreach (var species in _speciesToAlign)
            {
                if (_alignedSpecies.ContainsKey(species.FullSequence))
                {
                    _alignedSpecies[species.FullSequence].Add(new LightWeightPsm()
                    {
                        FullSequence = species.FullSequence,
                        BaseSequence = species.BaseSequence,
                        FileName = species.FileName,
                        RetentionTime = species.RetentionTime
                    });
                }
                else
                {
                    toPredict.Add(new PreCalibrated()
                    {
                        FileName = species.FileName,
                        FullSequence = species.FullSequence,
                        BaseSequence = species.BaseSequence,
                        UnCalibratedRetentionTime = species.RetentionTime
                    });
                }
            }
        }


        (string FileName, string FullSequence, float RetentionTime, Calibrated)[] calibrated =
            new (string FileName, string FullSequence, float RetentionTime, Calibrated)[toPredict.Count];

        var predictionEngine = GetPredictionEngine(useChronologer);

        Parallel.For(0, calibrated.Length,
            i =>
            {
                calibrated[i] = (toPredict[i].FileName, toPredict[i].FullSequence,
                    toPredict[i].UnCalibratedRetentionTime, predictionEngine.Predict(toPredict[i]));
            });

        // add them into the _alignedSpecies
        foreach (var predictions in calibrated)
        {
            if (_alignedSpecies.ContainsKey(predictions.Item1))
            {
                _alignedSpecies[predictions.Item1].Add(new LightWeightPsm()
                {
                    FullSequence = predictions.FullSequence,
                    FileName = predictions.FileName,
                    RetentionTime = predictions.RetentionTime,
                    CalibratedRetentionTime = predictions.Item4.CalibratedRetentionTime
                });
            }
            else
            {
                _alignedSpecies.Add(predictions.FileName, new List<LightWeightPsm>());
                _alignedSpecies[predictions.FileName].Add(new LightWeightPsm()
                {
                    FullSequence = predictions.FullSequence,
                    FileName = predictions.FileName,
                    RetentionTime = predictions.RetentionTime,
                    CalibratedRetentionTime = predictions.Item4.CalibratedRetentionTime
                });
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
        var anchorsBetweenBothSets = _speciesToAlign
            .Where(x => _alignedSpecies.ContainsKey(x.FullSequence))
            .DistinctBy(x => x.FullSequence);

        // Prepare the data for the dataview
        if (useChronologer)
        {
            foreach (var anchor in anchorsBetweenBothSets)
            {
                PreCalibratedList.Add(new PreCalibrated()
                {
                    FullSequence = anchor.FullSequence,
                    //AnchorRetentionTime = _alignedSpecies[anchor.FullSequence].Select(x => x).Median(),
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
                    FileName = anchor.FileName,
                    AnchorRetentionTime = _alignedSpecies[anchor.FullSequence].Select(x => x.RetentionTime).Median(),
                    UnCalibratedRetentionTime = anchor.RetentionTime
                });
            }
        }

        var dataView = mlContext.Data.LoadFromEnumerable(
            PreCalibratedList.Where(x => x.UnCalibratedRetentionTime > -1).ToArray());

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

    public List<IRetentionTimeAlignable> GetResults()
    {
        List<IRetentionTimeAlignable> alignedSpecies = new();
        foreach (var file in _alignedSpecies)
        {
            alignedSpecies.AddRange(file.Value);
        }

        return alignedSpecies;
    }



    public void Dispose()
    {
        _alignedSpecies.Clear();
        _speciesToAlign.Clear();
    }
}