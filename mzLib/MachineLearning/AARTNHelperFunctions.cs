using System.Diagnostics;
using Proteomics.PSM;
using TorchSharp;
using TorchSharp.Modules;

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

            //Parameters
            var options = Configuration();

            //Define Optimizer
            var optimizer = torch.optim.Adam(transformerModel.parameters(), (double)options["learningRate"]);

            //Tokenize psms
            var tokensAndRetentioTimes = new List<(List<Tokenizer.Token>, double)>();

            foreach (var psm in psms)
            {
                var tokenizedPsm = Tokenizer.Tokenize(psm.FullSequence,
                    @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtefacts.zip");

                tokensAndRetentioTimes.Add((tokenizedPsm, psm.RetentionTime.Value));
            }

            //Define data sets and loaders (train, validation, test)
            //datasets
            var (trainingData, validationData, testingData) =
                Tokenizer.SplitDataIntoTrainingValidationAndTesting(tokensAndRetentioTimes);

            var trainingDataset =
                new AARTNDataset(trainingData, @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtefacts.zip");
            var validationDataset =
                new AARTNDataset(validationData, @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtefacts.zip");
            var testingDataset =
                new AARTNDataset(testingData, @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtefacts.zip");

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
                    var encoderInput = batch.
                }
            }
        }

        public static Dictionary<string, object> Configuration()
        {
            return new Dictionary<string, object>()
            {{"batchSize", 8},
            {"epochs", 10},
            {"learningRate", 10^-4},
            {"sequenceLength", 150},
            {"dModel", 512}};
        }
    }
}
