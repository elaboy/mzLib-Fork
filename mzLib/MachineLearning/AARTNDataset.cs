using Microsoft.ML;
using TorchSharp;
using TorchSharp.Modules;
using static MachineLearning.Tokenizer;

namespace MachineLearning
{
    public class AARTNDataset : torch.utils.data.Dataset
    {
        public override long Count => _dataset.Count;

        public override Dictionary<string, torch.Tensor> GetTensor(long index)
        {
            var sourceTargetPair = _dataset.ElementAt((int)index);
            var sequence = sourceTargetPair.Item1.Select(x => x.Features);
            var target = sourceTargetPair.Item2;

            var tensorizedSequence = torch.from_array(sequence.ToArray());

            return new Dictionary<string, torch.Tensor>(){{target.ToString(), tensorizedSequence}};
        }

        public AARTNDataset(List<(List<Token>, double)> dataset,
            string tokenizerPath, int sequenceLength = 60) : base()
        {
            MLContext mlContext = new MLContext();
            
            _dataset = dataset;

            //Make engine and save to object
            var tokenizerEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(
                mlContext.Model.Load(tokenizerPath, out var modelI));
            
            _tokenizer = tokenizerEngine;

            //Save padding token
            PaddingToken = _tokenizer.Predict(new Token() { Residue = "PAD", Id = 0 });
        }

        private List<(List<Token>, double)> _dataset;
        private PredictionEngine<ResidueData, Token> _tokenizer;
        public Token PaddingToken;
    }
}
