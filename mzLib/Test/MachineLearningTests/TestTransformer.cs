using System;
using MachineLearning;
using MachineLearning.RetentionTimePredictionModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using TorchSharp;
using TorchSharp.Modules;
using UsefulProteomicsDatabases;
using static TorchSharp.torch;
using TensorStringStyle = TorchSharp.TensorStringStyle;

namespace Test.MachineLearningTests
{
    public class TestTransformer
    {
        [Test]
        public void TestTrainTokenizer()
        {
            Tokenizer.TrainTokenizer(@"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip",
                TokenizerModType.CommonBiologicalAndArtifacts);
        }

        [Test]
        public void TestTokenizer()
        {
            var listOfInputs = new List<string>()
            {
                "A", "C", "D", "E", "F", "G", "H", "I", "K",
                "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "Y"
            };

            //var tokens = Tokenizer.Tokenize(listOfInputs, @"D:/AI_Datasets/Tokenizer.zip");
        }

        [Test]
        public void TestAndTokenizeHela1()
        {
            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                               @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            List<(List<Tokenizer.Token>, double)> tokens = new();

            foreach (var psm in psms)
            {

                var psmTokens = Tokenizer.Tokenize(psm.FullSequence,
                    @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip");

                var token = (psmTokens, psm.RetentionTime.Value);

                tokens.Add(token);
                Debug.WriteLine(psm.FullSequence + " " + tokens.Count);
            }

        }

        //[Test]
        //public void TestTraining()
        //{
        //    var psms =
        //        Readers.SpectrumMatchTsvReader
        //            .ReadPsmTsv(@"D:/AI_Datasets/Hela1_AllPSMs.psmtsv",
        //                out var warnings);

        //    var tokens = new List<(List<Tokenizer.Token>, double)>();

        //    Parallel.ForEach(psms, psm =>
        //    {

        //        var psmTokens = Tokenizer.Tokenize(psm.FullSequence,
        //            @"D:/AI_Datasets/Tokenizer.zip");

        //        var token = (psmTokens, psm.RetentionTime.Value);

        //        tokens.Add(token);
        //        Debug.WriteLine(psm.FullSequence + " " + tokens.Count);
        //    });

        //    var model = Transformer.BuildAARTN(tokens.First().Item1.First().Features.Length,
        //        1, 60, 1);

        //}

        [Test]
        public void TestEnsambleModel()
        {
            var model = AARTN.EnsambleModel(21, 21,
                25, 25);
        }

        [Test]
        public void TestTrainTransformer()
        {
            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            var model = AARTN.EnsambleModel(44, 1, 44,
                1);
            AARTNHelperFunctions.TrainTransformer(model, psms);
        }

        [Test]
        public void TestSwitch()
        {
            TokenizerModType modificationType = TokenizerModType.CommonBiologicalAndArtifacts;
            var unimodData =
                Loaders.LoadUnimod(Path.Combine(Directory.GetCurrentDirectory(), "unimod.xml")).ToList();
            //.Where( x => x.ModificationType == "UniProt").ToList(); //Uniprot and AAs

            switch (modificationType)
            {
                case TokenizerModType.Everything:
                    break;
                case TokenizerModType.Uniprot:
                    unimodData = unimodData.Where(x => x.ModificationType == "UniProt").ToList();
                    break;
                case TokenizerModType.CommonBiological:
                    unimodData = unimodData.Where(x => x.ModificationType == "Common Biological").ToList();
                    break;
                case TokenizerModType.CommonArtifacts:
                    unimodData = unimodData.Where(x => x.ModificationType == "Common Artifact").ToList();
                    break;
                case TokenizerModType.LessCommonMods:
                    unimodData = unimodData.Where(x => x.ModificationType == "Less Common").ToList();
                    break;
                case TokenizerModType.Metals:
                    unimodData = unimodData.Where(x => x.ModificationType == "Metals").ToList();
                    break;
                case TokenizerModType.CommonBiologicalAndArtifacts:
                    unimodData = unimodData.Where(x => x.ModificationType == "Common Biological" ||
                                                       x.ModificationType == "Common Artifact").ToList();
                    break;
            }

            int zxero = 1;
        }

        [Test]
        public void TestTokenLoadFromCSVFile()
        {
            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Tokens>().ToList();
            }

        }

        [Test]
        public void TestTokenLoadFromCSVFileV2()
        {
            List<Tokens> tokens = new List<Tokens>();
            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod_V2.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                tokens.AddRange(csv.GetRecords<Tokens>().ToList());
            }

            var embeddingLayer = torch.nn.Embedding(tokens.Count(), 512, padding_idx: 4);
            Debug.WriteLine(embeddingLayer.forward(torch.tensor(tokens.First().Id))
                .ToString(TensorStringStyle.Julia));
        }

        [Test]
        public void TestTokenLoadFromCSVFileV2WithRealExample()
        {
            List<Tokens> tokens = new List<Tokens>();
            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod_V2.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                tokens.AddRange(csv.GetRecords<Tokens>().ToList());
            }

            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            var tokenizedPsm = 
                TokenGeneration.TokenizeRetentionTimeWithFullSequence(psms.First(), 160);

            var tokenizedPsmIds = new List<int>();

            foreach (var token in tokenizedPsm)
            {
                if (int.TryParse(token[0].ToString(), out var result))
                {
                    foreach (var subString in token)
                        tokenizedPsmIds.Add(tokens.Find(x => x.Token == subString.ToString()).Id);
                }
                else
                    tokenizedPsmIds.Add(tokens.Find(x => x.Token == token).Id);
            }

            var embeddingLayer = torch.nn.Embedding(tokens.Count(), 512, padding_idx: 0);

            var tensor = TokenGeneration.PaddingTensor(torch.tensor(tokenizedPsmIds), 200);

            Debug.WriteLine(embeddingLayer.forward(tensor)
                .ToString(TensorStringStyle.Julia));
            tokenizedPsm.ForEach(x => Debug.Write(x+" "));
        }

        [Test]
        public void TestTokensFeaturizer()
        {
            var tokens = new List<Tokens>();

            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                tokens.AddRange(csv.GetRecords<Tokens>().ToList());
            }

            var mlContext = new MLContext();

            var options = new TextFeaturizingEstimator.Options()
            {
                Norm = TextFeaturizingEstimator.NormFunction.None,
                //WordFeatureExtractor = 
                //WordFeatureExtractor = new WordBagEstimator.Options()
                //{
                //    NgramLength = 1,
                //    UseAllLengths = false,
                //    SkipLength = 0,
                //    Weighting = NgramExtractingEstimator.WeightingCriteria.Idf
                //    //MaximumNgramsCount = 1000000,
                //    //Weighting = NgramExtractingEstimator.WeightingCriteria./*TfIdf*/
                //},
            };

            var textPipeline = mlContext.Transforms.Text
                .FeaturizeText("Features", options, "Token");

            var dataView = mlContext.Data.LoadFromEnumerable(tokens);

            var transformer = textPipeline.Fit(mlContext.Data.LoadFromEnumerable(tokens));

            var predictionEngine = mlContext.Model.CreatePredictionEngine<Tokens, FeaturizedTokens>(transformer);

            var featurizedTokens = new List<FeaturizedTokens>();

            foreach (var token in tokens)
            {
                featurizedTokens.Add(predictionEngine.Predict(token));
            }

            mlContext.Model.Save(transformer, transformer.GetOutputSchema(dataView.Schema),
                @"D:/AI_Datasets/testingTokenizerUnimod.zip");
        }

        [Test]
        public void TestTransformerRun()
        {
            var mlContext = new MLContext();

            var featurizer = mlContext.Model
                .Load(@"D:\AI_Datasets\testingTokenizerUnimod.zip", out var inputSchema);

            var predictionEngine = mlContext.Model.CreatePredictionEngine<Tokens, FeaturizedTokens>(featurizer);

            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            var featuresWithRetentionTime = new List<(FeaturizedTokens[], double)>();

            var vocab = new List<Tokens>();

            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                vocab.AddRange(csv.GetRecords<Tokens>().Distinct().ToList());
            }

            var dictionary = new Dictionary<string, int>();
            foreach (var item in vocab)
            {
                dictionary.Add(item.Token, item.Id);
            }

            psms = psms.Take(100).ToList(); //small test

            foreach (var psm in psms)
            {
                var splitItem = psm.FullSequence
                    .Split(new[] { '[', ']' });
                var psmTokenized = new List<FeaturizedTokens>();

                foreach (var subItem in splitItem)
                {
                    if (dictionary.ContainsKey(subItem))
                    {
                        var features = predictionEngine.Predict(new Tokens()
                        { Id = dictionary[subItem], Token = subItem });
                        psmTokenized.Add(features);
                    }
                }

                while (psmTokenized.Count < 150)
                {
                    psmTokenized.Add(predictionEngine.Predict(new Tokens() { Id = 0, Token = "PAD" }));
                }

                featuresWithRetentionTime.Add((psmTokenized.ToArray(), psm.RetentionTime.Value));
            }

            var model = AARTN.EnsambleModel(2708, 1, 150, 1);
            model.save(@"D:/AI_Datasets/TransformerModel1182024.zip");

            var (trainingSet, validationSet, testSet) =
                Tokenizer.SplitDataIntoTrainingValidationAndTesting(featuresWithRetentionTime);

            var trainingDataset = new AARTNDataset(trainingSet);
            var validationDataset = new AARTNDataset(validationSet);
            var testingDataset = new AARTNDataset(testSet);

            var trainingDataLoader = new DataLoader(trainingDataset, 1, shuffle: true, null, 1, 1, true);

            var optimizer = optim.Adam(model.parameters(), 0.001);

            var lossFunction = nn.CrossEntropyLoss(
                               ignore_index: dictionary["PaddingToken"]);

            foreach (var batch in trainingDataLoader)
            {
                var encoderInput = batch["EncoderInput"];
                var decoderInput = batch["DecoderInput"];
                var encoderMask = batch["EncoderMask"];
                var decoderMask = batch["DecoderMask"];

                var encoderOutput = model.Encode(encoderInput, encoderMask);
                var decoderOutput = model.Decode(encoderOutput, encoderMask,
                    decoderInput, decoderMask);
                var projectionOutput = model.Project(decoderOutput);

                var label = batch["DecoderInput"];

            }

            int zero = 1;
        }
    }
}
