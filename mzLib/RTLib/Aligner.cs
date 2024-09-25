using MassSpectrometry;
using MathNet.Numerics.Statistics;
using Microsoft.ML;

namespace RTLib;

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