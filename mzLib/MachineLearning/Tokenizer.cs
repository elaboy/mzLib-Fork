using Microsoft.ML;
using System.Data;
using UsefulProteomicsDatabases;

namespace MachineLearning;

public static class Tokenizer
{
    public static void TrainTokenizer(string savingPath)
    {
        var mlContext = new MLContext();

        var unimodData = 
            Loaders.LoadUnimod(Path.Combine(Directory.GetCurrentDirectory(), "unimod.xml"));

        var aa = new List<string>()
        {
            "A", "C", "D", "E", "F", "G", "H", "I", "K",
            "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "Y"

        };

        //List of mods/residues
        var listOfResidues = unimodData.Select(x => new ResidueData() { Residue = x.IdWithMotif }).ToList();
        listOfResidues.AddRange(aa.Select(x => new ResidueData() { Residue = x }));

        //dataview is needed for the tokenization
        var dataView = mlContext.Data.LoadFromEnumerable(listOfResidues);

        var tokenization =
            mlContext.Transforms.Text.FeaturizeText("Features", "Residue");

        var pipeline = tokenization.Fit(dataView);

        //var predictionEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(pipeline);

        mlContext.Model.Save(pipeline, pipeline.GetOutputSchema(dataView.Schema), savingPath);
    }

    public static List<Token> Tokenize(List<string> listOfInputs, string modelPath)
    {
        var listOfTokens = new List<Token>();
        var mlContext = new MLContext();

        var data = listOfInputs.Select(x => new ResidueData() { Residue = x }).ToList();

        var dataView = mlContext.Data.LoadFromEnumerable(data);

        ITransformer tokenizer = mlContext.Model.Load(modelPath, out var modelI);

        var predictionEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(tokenizer);

        var token = data.Select(x => predictionEngine.Predict(x));

        listOfTokens.AddRange(token.Select(x => x));


        return listOfTokens;
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