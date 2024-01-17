using System.Diagnostics;
using Microsoft.ML.Data;
using Proteomics.PSM;
using TorchSharp;
using TorchSharp.Modules;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace MachineLearning
{
    public static class AARTNHelperFunctions
    {
        public static void TrainTransformer(TransformerComponents.Transformer transformerModel, List<PsmFromTsv> psms)
        {
            //Define Device 
            var device = torch.cuda.is_available()
                ? //if true, use GPU, else CPU
                torch.device(DeviceType.CUDA)
                : torch.device(DeviceType.CPU);

            transformerModel.to(device);

            //Parameters
            var options = Configuration();

            //Define Optimizer
            var optimizer = torch.optim.Adam(transformerModel.parameters(), options["learningRate"]);

            //Tokenize psms
            var tokensAndRetentioTimes = new List<(List<Tokenizer.Token>, double)>();

            foreach (var psm in psms)
            {
                var tokenizedPsm = Tokenizer.Tokenize(psm.FullSequence,
                    @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip");

                tokensAndRetentioTimes.Add((tokenizedPsm, psm.RetentionTime.Value));

                if (tokensAndRetentioTimes.Count == 100)
                    break;
            }

            //Define data sets and loaders (train, validation, test)
            //datasets
            var (trainingData, validationData, testingData) =
                Tokenizer.SplitDataIntoTrainingValidationAndTesting(tokensAndRetentioTimes);

            var trainingDataset =
            new AARTNDataset(trainingData, @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip");
            var validationDataset =
                new AARTNDataset(validationData, @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip");
            var testingDataset =
                new AARTNDataset(testingData, @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip");

            //dataloaders
            var trainingDataLoader = new DataLoader(trainingDataset, (int)options["batchSize"],
                true, null, null, 1, true);
            var validationDataLoader = new DataLoader(validationDataset, (int)options["batchSize"],
                true, null, null, 1, true);
            var testingDataLoader = new DataLoader(testingDataset, (int)options["batchSize"],
                true, null, null, 1, true);

            var intialEpoch = 0;
            var globalStep = 0;
            var lossFunction = torch.nn.CrossEntropyLoss(
                ignore_index: trainingDataset.PaddingToken.Id).to(device);

            for (int currentEpoch = 0; currentEpoch < (int)options["epochs"]; currentEpoch++)
            {
                transformerModel.train();
                foreach (var batch in trainingDataLoader)
                {
                    var encoderInput = batch["EncoderInput"].to(device);
                    var decoderInput = batch["DecoderInput"].to(device);
                    var encoderMask = batch["EncoderMask"].to(device);
                    var decoderMask = batch["DecoderMask"].to(device);

                    //Run tensors through the transformer
                    var encoderOutput = transformerModel.Encode(encoderInput, encoderMask).to(device);
                    var decoderOutput = transformerModel.Decode(encoderOutput, encoderMask, 
                        decoderInput, decoderMask).to(device);
                    var projectionOutput = transformerModel.Project(decoderOutput).to(device);

                    var label = batch["DecoderInput"].to(device);

                    var loss = lossFunction.forward(projectionOutput.view(-1, tokensAndRetentioTimes
                            .First().Item1.First().Features.Length),
                        label.view(-1));

                    loss.backward();
                    optimizer.step();
                    optimizer.zero_grad();

                    Debug.WriteLine("Epoch: " + currentEpoch + " Loss: " + loss.item<float>());
                }
            }
        }

        public static Dictionary<string, double> Configuration()
        {
            return new Dictionary<string, double>()
            {{"batchSize", 32},
            {"epochs", 10},
            {"learningRate", 0.0001},
            {"sequenceLength", 150},
            {"dModel", 2708}};
        }
    }

    public class Tokens
    {
        [Name("Token")]
        public string Token { get; set; }
        [Name("Id")]
        public int Id { get; set; }
    }

    public class FeaturizedTokens : Tokens 
    {
        public float[] Features { get; set; }
    }
}
