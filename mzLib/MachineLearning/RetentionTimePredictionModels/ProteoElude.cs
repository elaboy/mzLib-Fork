using System.Xml.Serialization;
using Proteomics.PSM;
using TorchSharp;
using TorchSharp.Modules;

namespace MachineLearning.RetentionTimePredictionModels
{
    public class ProteoElude : DeepTorch
    {
        public override TorchDataset? TrainingDataset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override TorchDataset? TestingDataset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override TorchDataset? ValidationDataset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ProteoElude()
        {
            
        }

        public override void CreateDataSet(List<PsmFromTsv> data, float validationFraction, float testingFraction, int batchSize)
        {
            throw new NotImplementedException();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            throw new NotImplementedException();
        }

        public override torch.Tensor Tensorize(object toTensoize)
        {
            throw new NotImplementedException();
        }

        public override void Train(string modelSavingPath, List<PsmFromTsv> trainingData, Dictionary<(char, string), int> dictionary, DeviceType device, float validationFraction, float testingFraction, int batchSize, int epochs, int patience)
        {
            throw new NotImplementedException();
        }

        protected override void CreateDataLoader(int batchSize)
        {
            throw new NotImplementedException();
        }

        protected override (double, List<double>, List<double>) Test(DataLoader? testingDataLoader, torch.nn.Module<torch.Tensor, torch.Tensor, torch.Tensor> criterion, DeviceType device)
        {
            throw new NotImplementedException();
        }

        protected override double Validate(DataLoader? validationDataLoader, torch.nn.Module<torch.Tensor, torch.Tensor, torch.Tensor> criterion, DeviceType device)
        {
            throw new NotImplementedException();
        }

        private Embedding embeddingLayer = torch.nn.Embedding(1, 1, 0);
    }
}
