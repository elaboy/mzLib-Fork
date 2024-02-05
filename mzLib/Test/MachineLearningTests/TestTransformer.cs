using System;
using MachineLearning;
using MachineLearning.RetentionTimePredictionModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
                TokenGeneration.TokenizeRetentionTimeWithFullSequence(psms.First());

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
            tokenizedPsm.ForEach(x => Debug.Write(x + " "));
        }

        [Test]
        public void TestDatasetCreationForAARTNDatasetClass()
        {
            List<Tokens> tokens = new List<Tokens>();
            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod_V3.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                tokens.AddRange(csv.GetRecords<Tokens>().ToList());
            }

            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                    @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings)
                .Where(x => x.AmbiguityLevel == "1")
                //.Take(50000)
                .ToList();

            var tokenizedPsmsList = new List<List<string>>();

            foreach (var psm in psms)
            {
                tokenizedPsmsList.Add(TokenGeneration.TokenizeRetentionTimeWithFullSequence(psm));
            }

            var tokenizedPsmsIds = new List<List<int>>();

            //get integers id for tokens to mask
            var rangeOfPositionIndicativeTokens = Enumerable.Range(1, 5);

            foreach (var tokenList in tokenizedPsmsList)
            {
                List<int> tokenIdList = new();

                foreach (var token in tokenList)
                {
                    if (tokens.Find(x => x.Token == token) is null &&
                        !token.Contains('_')) //Checks if token is a number, if not clear list and break without adding to main list
                    {
                        tokenIdList.Clear();
                        break;
                    }
                    if (int.TryParse(token[0].ToString(), out var result)) //Takes care of retention time numbers and array positions
                    {
                        foreach (var subString in token)
                            tokenIdList.Add(tokens.Find(x => x.Token == subString.ToString()).Id);
                    }
                    else
                    {
                        if (tokens.Any(x => x.Token == token)) //takes all the other non numerical tokens and adds their id to the list
                        {
                            tokenIdList.Add(tokens.Find(x => x.Token == token).Id);
                        }
                    }
                }
                if (tokenIdList.Count != 0) //Empty list is not added to main list
                    tokenIdList = TokenGeneration.PaddingIntegerList(tokenIdList, 0, 100); //makes sure padding is done right with desired length
                else
                    continue;

                tokenizedPsmsIds.Add(tokenIdList);
            }

            //var dataset = new AARTNDataset(tokenizedPsmsIds);

            var (train, validate, test) = TokenGeneration.TrainValidateTestSplit(tokenizedPsmsIds);

            //datasets
            var trainingDataset = new AARTNDataset(train);
            var validationDataset = new AARTNDataset(validate);
            var testingDataset = new AARTNDataset(test);

            //dataloaders
            var trainingDataLoader = new DataLoader(trainingDataset, 10, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            var validationDataLoader = new DataLoader(validationDataset, 10, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            var testingDataLoader = new DataLoader(testingDataset, 10, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            //var dataLoader = new DataLoader(dataset, 32, shuffle: true, null, 1, 1, true);

            var model = AARTN.EnsambleModel(2729, 100, 100, 100);

            AARTNHelperFunctions.TrainTransformer(model, trainingDataLoader, validationDataLoader, testingDataLoader);

            model.save(@"D:\AI_Datasets\transformerWarmup01242024.dat");

            //foreach (var batch in dataLoader)
            //{
            //    var encoderInput = batch["EncoderInput"];
            //    var decoderInput = batch["DecoderInput"];
            //    var encoderMask = batch["EncoderMask"];
            //    var decoderMask = batch["DecoderMask"];

            //    Debug.WriteLine("---------------------------------------------------");
            //    Debug.WriteLine("EncoderInput: ");
            //    Debug.WriteLine(encoderInput.ToString(TensorStringStyle.Julia));
            //    Debug.WriteLine("DecoderInput: ");
            //    Debug.WriteLine(decoderInput.ToString(TensorStringStyle.Julia));
            //    Debug.WriteLine("EncoderMask: ");
            //    Debug.WriteLine(encoderMask.ToString(TensorStringStyle.Julia));
            //    Debug.WriteLine("DecoderMask: ");
            //    Debug.WriteLine(decoderMask.ToString(TensorStringStyle.Julia));
            //}



        }

        [Test]
        public void CarbamidoMethylOnlyTransformer()
        {
            List<Tokens> tokens = new List<Tokens>();
            using (var reader = new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod_CarbamidomethylOnCOnly.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                tokens.AddRange(csv.GetRecords<Tokens>().ToList());
            }

            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                    @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings)
                .Where(x => x.AmbiguityLevel == "1" && x.QValue <= 0.01 &&
                            x.FullSequence.Contains("Carbamidomethyl on C") ||
                            x.FullSequence.Contains("Oxidation on M") ||
                            x.FullSequence.Contains("Phosphoserine on S") ||
                            !x.FullSequence.Contains("["))
                //.Take(50000)
                .ToList();

            var tokenizedPsmsList = new List<List<string>>();

            foreach (var psm in psms)
            {
                tokenizedPsmsList.Add(TokenGeneration.TokenizeRetentionTimeWithFullSequence(psm));
            }

            var tokenizedPsmsIds = new List<List<int>>();

            //get integers id for tokens to mask
            var rangeOfPositionIndicativeTokens = Enumerable.Range(1, 5);

            foreach (var tokenList in tokenizedPsmsList)
            {
                List<int> tokenIdList = new();

                foreach (var token in tokenList)
                {
                    if (tokens.Find(x => x.Token == token) is null &&
                        !token.Contains('_')) //Checks if token is a number, if not clear list and break without adding to main list
                    {
                        tokenIdList.Clear();
                        break;
                    }
                    if (int.TryParse(token[0].ToString(), out var result)) //Takes care of retention time numbers and array positions
                    {
                        foreach (var subString in token)
                            tokenIdList.Add(tokens.Find(x => x.Token == subString.ToString()).Id);
                    }
                    else
                    {
                        if (tokens.Any(x => x.Token == token)) //takes all the other non numerical tokens and adds their id to the list
                        {
                            tokenIdList.Add(tokens.Find(x => x.Token == token).Id);
                        }
                    }
                }
                if (tokenIdList.Count != 0) //Empty list is not added to main list
                    tokenIdList = TokenGeneration.PaddingIntegerList(tokenIdList, 0, 60); //makes sure padding is done right with desired length
                else
                    continue;

                tokenizedPsmsIds.Add(tokenIdList);
            }

            //var dataset = new AARTNDataset(tokenizedPsmsIds);

            var (train, validate, test) = TokenGeneration.TrainValidateTestSplit(tokenizedPsmsIds);

            //datasets
            var trainingDataset = new AARTNDataset(train);
            var validationDataset = new AARTNDataset(validate);
            var testingDataset = new AARTNDataset(test);

            //dataloaders
            var trainingDataLoader = new DataLoader(trainingDataset, 64, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            var validationDataLoader = new DataLoader(validationDataset, 64, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            var testingDataLoader = new DataLoader(testingDataset, 64, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            //var dataLoader = new DataLoader(dataset, 32, shuffle: true, null, 1, 1, true);

            Debug.WriteLine(trainingDataLoader.Count);
            Debug.WriteLine(validationDataLoader.Count);
            Debug.WriteLine(testingDataLoader.Count);

            var model = AARTN.EnsambleModel(60,60,60,60);

            AARTNHelperFunctions.TrainTransformer(model, trainingDataLoader, validationDataLoader, testingDataLoader);

            model.save(@"D:\AI_Datasets\transformerCarbamidomethyl.dat");

            //foreach (var batch in dataLoader)
            //{
            //    var encoderInput = batch["EncoderInput"];
            //    var decoderInput = batch["DecoderInput"];
            //    var encoderMask = batch["EncoderMask"];
            //    var decoderMask = batch["DecoderMask"];

            //    Debug.WriteLine("---------------------------------------------------");
            //    Debug.WriteLine("EncoderInput: ");
            //    Debug.WriteLine(encoderInput.ToString(TensorStringStyle.Julia));
            //    Debug.WriteLine("DecoderInput: ");
            //    Debug.WriteLine(decoderInput.ToString(TensorStringStyle.Julia));
            //    Debug.WriteLine("EncoderMask: ");
            //    Debug.WriteLine(encoderMask.ToString(TensorStringStyle.Julia));
            //    Debug.WriteLine("DecoderMask: ");
            //    Debug.WriteLine(decoderMask.ToString(TensorStringStyle.Julia));
            //}



        }

        [Test]
        public void TestCNNWithAttention()
        {
            List<Tokens> tokens = new List<Tokens>();
            using (var reader =
                   new StreamReader(@"D:\AI_Datasets\VocabularyForTransformerUnimod_CarbamidomethylOnCOnly.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                tokens.AddRange(csv.GetRecords<Tokens>().ToList());
            }

            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                    @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings)
                .Where(x => x.AmbiguityLevel == "1" && x.QValue <= 0.01 &&
                            x.FullSequence.Contains("Carbamidomethyl on C") ||
                            x.FullSequence.Contains("Oxidation on M") ||
                            x.FullSequence.Contains("Phosphoserine on S") ||
                            !x.FullSequence.Contains("["))
                //.Take(50000)
                .ToList();

            var tokenizedPsmsList = new List<List<string>>();

            foreach (var psm in psms)
            {
                tokenizedPsmsList.Add(TokenGeneration.TokenizeRetentionTimeWithFullSequence(psm));
            }

            var tokenizedPsmsIds = new List<List<int>>();

            //get integers id for tokens to mask
            var rangeOfPositionIndicativeTokens = Enumerable.Range(1, 5);

            foreach (var tokenList in tokenizedPsmsList)
            {
                List<int> tokenIdList = new();

                foreach (var token in tokenList)
                {
                    if (tokens.Find(x => x.Token == token) is null &&
                        !token.Contains(
                            '_')) //Checks if token is a number, if not clear list and break without adding to main list
                    {
                        tokenIdList.Clear();
                        break;
                    }

                    if (int.TryParse(token[0].ToString(),
                            out var result)) //Takes care of retention time numbers and array positions
                    {
                        foreach (var subString in token)
                            tokenIdList.Add(tokens.Find(x => x.Token == subString.ToString()).Id);
                    }
                    else
                    {
                        if (tokens.Any(x =>
                                x.Token ==
                                token)) //takes all the other non numerical tokens and adds their id to the list
                        {
                            tokenIdList.Add(tokens.Find(x => x.Token == token).Id);
                        }
                    }
                }

                if (tokenIdList.Count != 0) //Empty list is not added to main list
                    tokenIdList =
                        TokenGeneration.PaddingIntegerList(tokenIdList, 0,
                            60); //makes sure padding is done right with desired length
                else
                    continue;

                tokenizedPsmsIds.Add(tokenIdList);
            }

            //var dataset = new AARTNDataset(tokenizedPsmsIds);

            var (train, validate, test) = TokenGeneration.TrainValidateTestSplit(tokenizedPsmsIds);

            //datasets
            var trainingDataset = new AARTNDataset(train);
            var validationDataset = new AARTNDataset(validate);
            var testingDataset = new AARTNDataset(test);

            //dataloaders
            var trainingDataLoader =
                new DataLoader(trainingDataset, 64, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            var validationDataLoader = new DataLoader(validationDataset, 64, shuffle: true, new Device(DeviceType.CPU),
                1, 1, true);
            var testingDataLoader =
                new DataLoader(testingDataset, 64, shuffle: true, new Device(DeviceType.CPU), 1, 1, true);
            //var dataLoader = new DataLoader(dataset, 32, shuffle: true, null, 1, 1, true);

            Debug.WriteLine(trainingDataLoader.Count);
            Debug.WriteLine(validationDataLoader.Count);
            Debug.WriteLine(testingDataLoader.Count);

            var model = torch.nn.Sequential();
            model.add_module("EmbeddingLayer", nn.Embedding(60, 256, padding_idx:0));
            model.add_module("Conv1", torch.nn.Conv2d(256, 512, 3));
            model.add_module("Conv2", torch.nn.Conv2d(512, 256, 3));
            model.add_module("MaxPooling1", torch.nn.MaxPool2d(3));
            model.add_module("AttentionHeads", torch.nn.MultiheadAttention(256, 8));
            model.add_module("Flatten", torch.nn.Flatten());
            model.add_module("Linear1", torch.nn.Linear(256, 128));
            model.add_module("Linear2", torch.nn.Linear(128, 60));

            var optimizer = optim.Adam(model.parameters(), 0.001);
            var lossFunction = nn.MSELoss();

            for (int i = 0; i < 10; i++)
            {
                Debug.WriteLine(i);
                foreach (var batch in trainingDataLoader)
                {
                    var encoderInput = batch["EncoderInput"].@float();
                    var decoderInput = batch["DecoderInput"].@float();

                    encoderInput.requires_grad = true;
                    decoderInput.requires_grad = true;

                    var prediction = model.forward(encoderInput);
                    var loss = lossFunction.forward(prediction, decoderInput);

                    Debug.WriteLine(loss.item<float>());

                    optimizer.zero_grad();
                    loss.backward();
                    optimizer.step();

                    //lossTracker.Add(loss.item<float>());


                }
            }
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
