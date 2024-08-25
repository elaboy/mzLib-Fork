using Easy.Common.Extensions;
using MathNet.Numerics.Statistics;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MassSpectrometry;
public class RetentionTimeAligner
{
    public List<IRetentionTimeAlignable> AllSpeciesInAllFiles { get; set; }

    public Dictionary<string, List<IRetentionTimeAlignable>> FilesInHarmonizer = new();

    public Dictionary<string, Dictionary<string, float>> HarmonizedSpecies = new();

    public RetentionTimeAligner() { }

    public RetentionTimeAligner(List<IRetentionTimeAlignable> retentionTimeAlignables)
    {
        AllSpeciesInAllFiles = retentionTimeAlignables;

        // group by FileName 
        var files = AllSpeciesInAllFiles
            .GroupBy(x => x.FileName).ToList();

        // populates FilesInHarmonizer (foreach FileName there is a List of IRetentionTimeAlignable
        foreach (var file in files)
        {
            FilesInHarmonizer.Add(file.Key, new List<IRetentionTimeAlignable>());
            FilesInHarmonizer[file.Key].AddRange(file.Select(x => x));
        }

        // order by count
        FilesInHarmonizer = FilesInHarmonizer.OrderByDescending(x => x.Value.Count)
            .ToDictionary(p => p.Key, p => p.Value);

        // Add all from the first file
        var firstLeader = FilesInHarmonizer.First();

        foreach (var identifier in firstLeader.Value)
        {
            if (HarmonizedSpecies.ContainsKey(identifier.Identifier))
                HarmonizedSpecies[identifier.Identifier].Add(firstLeader.Key, (float)identifier.RetentionTime);
            else
            {
                HarmonizedSpecies.Add(identifier.Identifier, new Dictionary<string, float>());
                HarmonizedSpecies[identifier.Identifier].Add(firstLeader.Key, (float)identifier.RetentionTime);
            }
        }

        // One iteration of PairwiseCalibration to set an initial calibration
        foreach (var file in FilesInHarmonizer.Where(x => !x.Key.Equals(firstLeader.Key)))
            InitialPairWiseCalibration(file.Key);
    }

    public Dictionary<string, double> QueryLibraryForRetentionTimes(List<string> identifiers)
    {
        List<(string, double)> matches = new(identifiers.Count);

        Parallel.For(0, matches.Count, index =>
        {
            (string, double) match = (identifiers[index], HarmonizedSpecies[identifiers[index]].Values
                .Select(x => x).Mean());
            if (!Double.IsNaN(match.Item2))
                matches[index] = (match.Item1, match.Item2);
            else
            {
                matches[index] = (match.Item1, -99);
            }
        });

        matches = matches.Where(x => !x.Item2.Equals(-99)).ToList();

        return matches.ToDictionary(p => p.Item1, p => p.Item2);
    }

    /// <summary>
    /// Copies and returns the results of the calibration.
    /// WARNING: this method will consume more memory because it creates a deep copy to avoid in-place replacements.
    /// </summary>
    /// <param name="warnings"></param>
    /// <param name="epochs"></param>
    /// <param name="minimumAnchors"></param>
    /// <returns></returns>
    //public Dictionary<string, Dictionary<string, double>> __Calibrate(int epochs = 1, int minimumAnchors = 2)
    //{
    //    var aligner = DeepCopy();

    //    for (int epoch = 0; epoch < epochs; epoch++)
    //    {
    //        foreach (var file in aligner.FilesInHarmonizer.Keys)
    //        {
    //            var anchorsAvailable = aligner.HarmonizedSpecies
    //                .Where(x => x.Value.Count >= minimumAnchors)
    //                .Select(x => x.Key).ToList();

    //            // pop out the file to re-calibrate
    //            var toCalibrate = aligner.HarmonizedSpecies
    //                .Where(x => x.Value.ContainsKey(file) &
    //                            anchorsAvailable.Contains(x.Key))
    //                .ToDictionary(p => p.Key, p => p.Value);

    //            //removes the popped out file from the Harmonized Species
    //            aligner.HarmonizedSpecies.ForEach(x => x.Value.Remove(file));

    //            // get anchors
    //            var filesInteresected = aligner.HarmonizedSpecies.Keys
    //                .Intersect(aligner.FilesInHarmonizer[file]
    //                    .Select(x => x.Identifier)).ToList();

    //            var anchors1 = filesInteresected
    //                .SelectMany(x => aligner.HarmonizedSpecies[x]
    //                    .Select(p => p.Value)).ToList();

    //            var anchors2 = filesInteresected
    //                .SelectMany(x => aligner.FilesInHarmonizer[file]
    //                    .Select(x => x.RetentionTime)).ToList();

    //            var anchors = new Dictionary<string, (float anchorRetentionTime, float retentionTime)>();

    //            for (int i = 0; i < filesInteresected.Count(); i++)
    //            {
    //                anchors.Add(filesInteresected.ElementAt(i), ((float)anchors1.ElementAt(i), (float)anchors2.ElementAt(i)));
    //            }

    //            var predictionEngine = MakePipeline(anchors);

    //            foreach (var unCalibratedFollowerSpecies in toCalibrate)
    //            {
    //                if (unCalibratedFollowerSpecies.Value.Count == 0)
    //                    continue;

    //                Calibrated prediction = predictionEngine.Predict(new PreCalibrated()
    //                {
    //                    Identifier = unCalibratedFollowerSpecies.Key,
    //                    UnCalibratedRetentionTime = (float)unCalibratedFollowerSpecies.Value.First().Value
    //                });
    //                if (aligner.HarmonizedSpecies.Keys.Contains(unCalibratedFollowerSpecies.Key))
    //                    aligner.HarmonizedSpecies[unCalibratedFollowerSpecies.Key].Add(file, prediction.CalibratedRetentionTime);
    //                else
    //                {
    //                    aligner.HarmonizedSpecies.Add(unCalibratedFollowerSpecies.Key, new Dictionary<string, double>());
    //                    aligner.HarmonizedSpecies[unCalibratedFollowerSpecies.Key].Add(file, prediction.CalibratedRetentionTime);
    //                }
    //            }
    //        }
    //    }

    //    return aligner.HarmonizedSpecies;
    //}

    public void Calibrate(int epochs = 1, int minimumAnchors = 2)
    {
        for (int epoch = 0; epoch < epochs; epoch++)
        {
            foreach (var file in FilesInHarmonizer.Keys)
            {
                var anchorsAvailable = HarmonizedSpecies
                    .Where(x => x.Value.Count >= minimumAnchors)
                    .Select(x => x.Key).ToList();

                // pop out the file to re-calibrate
                var toCalibrate = HarmonizedSpecies
                    .Where(x => x.Value.ContainsKey(file) &
                                anchorsAvailable.Contains(x.Key))
                    .ToDictionary(p => p.Key, p => p.Value);

                //removes the popped out file from the Harmonized Species
                HarmonizedSpecies.ForEach(x => x.Value.Remove(file));

                // get anchors
                var filesInteresected = HarmonizedSpecies.Keys
                    .Intersect(FilesInHarmonizer[file]
                        .Select(x => x.Identifier)).ToList();

                var anchors1 = filesInteresected
                    .SelectMany(x => HarmonizedSpecies[x]
                        .Select(p => p.Value)).ToList();

                var anchors2 = filesInteresected
                    .SelectMany(x => FilesInHarmonizer[file]
                        .Select(x => x.RetentionTime)).ToList();

                var anchors = new Dictionary<string, (float anchorRetentionTime, float retentionTime)>();

                for (int i = 0; i < filesInteresected.Count(); i++)
                {
                    anchors.Add(filesInteresected.ElementAt(i),
                        ((float)anchors1.ElementAt(i), (float)anchors2.ElementAt(i)));
                }

                var predictionEngine = MakePipeline(anchors);

                foreach (var unCalibratedFollowerSpecies in toCalibrate)
                {
                    if (unCalibratedFollowerSpecies.Value.Count == 0)
                        continue;

                    Calibrated prediction = predictionEngine.Predict(new PreCalibrated()
                    {
                        Identifier = unCalibratedFollowerSpecies.Key,
                        UnCalibratedRetentionTime = (float)unCalibratedFollowerSpecies.Value.First().Value
                    });
                    if (HarmonizedSpecies.Keys.Contains(unCalibratedFollowerSpecies.Key))
                        HarmonizedSpecies[unCalibratedFollowerSpecies.Key].Add(file, prediction.CalibratedRetentionTime);
                    else
                    {
                        HarmonizedSpecies.Add(unCalibratedFollowerSpecies.Key, new Dictionary<string, float>());
                        HarmonizedSpecies[unCalibratedFollowerSpecies.Key].Add(file, prediction.CalibratedRetentionTime);
                    }
                }
            }
        }
    }

    //private RetentionTimeAligner DeepCopy()
    //{
    //    RetentionTimeAligner aligner = (RetentionTimeAligner)this.MemberwiseClone();
    //    aligner.AllSpeciesInAllFiles = new List<IRetentionTimeAlignable>() { this.AllSpeciesInAllFiles };
    //    aligner.FilesInHarmonizer = new Dictionary<string, List<IRetentionTimeAlignable>>() { { FilesInHarmonizer } };
    //    aligner.HarmonizedSpecies = new Dictionary<string, Dictionary<string, double>>() { this.HarmonizedSpecies };

    //    return aligner;
    //}

    private PredictionEngine<PreCalibrated, Calibrated> MakePipeline(Dictionary<string, (float anchorRetentionTime, float retentionTime)> anchors)
    {
        MLContext mlContext = new MLContext();

        List<PreCalibrated> PreCalibrateds = new();

        // Prepare the data for the dataview
        foreach (var anchor in anchors)
        {
            PreCalibrateds.Add(new PreCalibrated()
            {
                Identifier = anchor.Key,
                AnchorRetentionTime = (float)anchor.Value.anchorRetentionTime,
                UnCalibratedRetentionTime = (float)anchor.Value.retentionTime
            });
        }

        var dataView = mlContext.Data.LoadFromEnumerable(PreCalibrateds.ToArray());

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

    private void InitialPairWiseCalibration(string followerFile)
    {

        var filesInteresected = HarmonizedSpecies.Keys
            .Intersect(FilesInHarmonizer[followerFile]
                .Select(x => x.Identifier)).ToList();

        var anchors1 = filesInteresected
            .SelectMany(x => HarmonizedSpecies[x]
                .Select(p => p.Value)).ToList();

        var anchors2 = filesInteresected
            .SelectMany(x => FilesInHarmonizer[followerFile]
                .Select(x => x.RetentionTime)).ToList();

        var anchors = new Dictionary<string, (float anchorRetentionTime, float retentionTime)>();

        for (int i = 0; i < filesInteresected.Count(); i++)
        {
            anchors.Add(filesInteresected.ElementAt(i),
                ((float)anchors1.ElementAt(i),
                    (float)anchors2.ElementAt(i)));
        }

        var predictionEngine = MakePipeline(anchors);

        foreach (var unCalibratedFollowerSpecies in FilesInHarmonizer[followerFile])
        {
            Calibrated prediction = predictionEngine.Predict(new PreCalibrated()
            {
                Identifier = unCalibratedFollowerSpecies.Identifier,
                UnCalibratedRetentionTime = (float)unCalibratedFollowerSpecies.RetentionTime
            });
            if (HarmonizedSpecies.ContainsKey(unCalibratedFollowerSpecies.Identifier))
                HarmonizedSpecies[unCalibratedFollowerSpecies.Identifier].Add(followerFile,
                    prediction.CalibratedRetentionTime);
            else
            {
                HarmonizedSpecies.Add(unCalibratedFollowerSpecies.Identifier, new Dictionary<string, float>());
                HarmonizedSpecies[unCalibratedFollowerSpecies.Identifier].Add(unCalibratedFollowerSpecies.FileName,
                    prediction.CalibratedRetentionTime);
            }
        }
    }
}