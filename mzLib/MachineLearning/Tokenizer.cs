using Microsoft.ML;
using System.Data;
using Easy.Common.Extensions;
using TorchSharp;
using UsefulProteomicsDatabases;

namespace MachineLearning;

public static class Tokenizer
{
    public static void TrainTokenizer(string savingPath)
    {
        var mlContext = new MLContext();

        var unimodData =
            Loaders.LoadUnimod(Path.Combine(Directory.GetCurrentDirectory(), "unimod.xml")).ToList();
                //.Where( x => x.ModificationType == "UniProt").ToList(); //Uniprot and AAs

        var aa = new List<string>()
        {
            "A", "C", "D", "E", "F", "G", "H", "I", "K",
            "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "Y"

        };

        //List of mods/residues
        var listOfResidues = unimodData.Select(x => new ResidueData() { Residue = x.IdWithMotif }).ToList();
        listOfResidues.AddRange(aa.Select(x => new ResidueData() { Residue = x }));
        listOfResidues.Add(new ResidueData() { Residue = "PAD" }); //padding token

        //dataview is needed for the tokenization
        var dataView = mlContext.Data.LoadFromEnumerable(listOfResidues);

        var tokenization =
            mlContext.Transforms.Text.FeaturizeText("Features", "Residue");

        var pipeline = tokenization.Fit(dataView);

        //var predictionEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(pipeline);

        mlContext.Model.Save(pipeline, pipeline.GetOutputSchema(dataView.Schema), savingPath);
    }

    public static List<Token> Tokenize(string listOfInputs, string modelPath)
    {
        var listOfTokens = new List<Token>();
        var mlContext = new MLContext();

        // var data = listOfInputs.Select(x => new ResidueData() { Residue = x }).ToList();

        List<ResidueData> toBeTokens = new();

        var splitItem = listOfInputs.Split(new []{'[', ']'});

        foreach (var item in splitItem)
        {
            if (item.Contains(' '))
            {
                toBeTokens.Add(new ResidueData(){Residue = item});
            }
            else
            {
                foreach (var residue in item)
                {
                    toBeTokens.Add(new ResidueData(){Residue = residue.ToString()});
                }
            }
        }

        if (toBeTokens.Count < 60)
        {
            //padding
            var pad = 60 - toBeTokens.Count;
            for (int i = 0; i < pad; i++)
            {
                toBeTokens.Add(new ResidueData() { Residue = "PAD"});
            }
        }

        var dataView = mlContext.Data.LoadFromEnumerable(toBeTokens);

        ITransformer tokenizer = mlContext.Model.Load(modelPath, out var modelI);

        var predictionEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(tokenizer);

        var token = toBeTokens.Select(x => predictionEngine.Predict(x));

        listOfTokens.AddRange(token.Select(x => x));


        return listOfTokens;
    }

    /// <summary>
    /// Train, Validation, Testing split of dataset
    /// </summary>
    public static (List<(List<Token>, double)>, List<(List<Token>, double)>, List<(List<Token>, double)>) 
        SplitDataIntoTrainingValidationAndTesting(List<(List<Token>, double)> dataset, double trainingFraction = 0.8,
            double testingFraction = 0.1, double validationFraction = 0.1)
    {
        var randomizedData = dataset.Randomize().ToList();

        var trainingSet = randomizedData.Take((int)(randomizedData.Count * (1 - validationFraction))).ToList();

        var validationSet = 
            randomizedData.Skip((int)(randomizedData.Count * (1 - validationFraction)))
            .Take((int)(randomizedData.Count * (1 - testingFraction))).ToList();

        var testSet = randomizedData.Skip((int)(randomizedData.Count * (1 - validationFraction))).ToList();

        return (trainingSet, validationSet, testSet);
    }

    public class ResidueData
    {
        public string Residue { get; set; }
    }

    public class Token : ResidueData
    {
        public float[] Features { get; set; }
    }
}