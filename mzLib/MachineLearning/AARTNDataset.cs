using Microsoft.ML;
using TorchSharp;
using static MachineLearning.Tokenizer;

namespace MachineLearning
{
    public class AARTNDataset : torch.utils.data.Dataset<(torch.Tensor, double)>
    {
        public override long Count => _dataset.Count;

        public override (torch.Tensor, double) GetTensor(long index)
        {
            var sourceTargetPair = _dataset.ElementAt((int)index);
            var sequence = sourceTargetPair.Item1;
            var target = sourceTargetPair.Item2;
        }

        public AARTNDataset(List<(List<Token>, double)> dataset,
            string tokenizerPath, int sequenceLength = 60)
        {
            MLContext mlContext = new MLContext();
            
            _dataset = dataset;

            //Make engine and save to object
            var tokenizerEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(
                mlContext.Model.Load(tokenizerPath, out var modelI));
            _tokenizer = tokenizerEngine;

            //Save padding token
            _paddingToken = _tokenizer.Predict(new Token() { Residue = "PAD" });
        }

        private List<(List<Token>, double)> _dataset;
        private PredictionEngine<ResidueData, Token> _tokenizer;
        private Token _paddingToken;
    }
}
