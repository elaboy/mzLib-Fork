using System.Diagnostics;
using Microsoft.ML;
using Tensorboard;
using TorchSharp;
using static MachineLearning.Tokenizer;

namespace MachineLearning
{
    public class AARTNDataset : torch.utils.data.Dataset
    {
        public override long Count => _integerDataset.Count;

        public override Dictionary<string, torch.Tensor> GetTensor(long index)
        {

            //Get source  and target

            var sourceTarget = new List<int>(_integerDataset.ElementAt((int)index));

            var maskedSourceTarget = new List<int>(_integerDataset.ElementAt((int)index));
            //Encoder Input
            var encoderInput = torch.from_array(sourceTarget.ToArray());

            //get integers id for tokens to mask
            var rangeOfTokensIntegersToMask = Enumerable.Range(6, 13);

            //Encoder Mask
            for (int i = 0; i < sourceTarget.Count; i++)
            {
                if (maskedSourceTarget[i] == 2)
                {
                    break;
                }

                if (rangeOfTokensIntegersToMask.Contains(maskedSourceTarget[i]))
                {
                    maskedSourceTarget[i] = 5; //masks retention time numbers
                }
            }

            var encoderMask = torch.from_array(maskedSourceTarget.ToArray());

            //DecoderInput
            var target = new List<int>();

            foreach (var item in sourceTarget)
            {
                if (rangeOfTokensIntegersToMask.Any(x => x.Equals(item)))
                {
                    target.Add(item);
                }
            }

            while (target.Count < 32)
            {
                target.Add(0);
            }

            var decoderInput = torch.from_array(target.ToArray());

            //Decoder Mask
            var decoderMaskArray = sourceTarget
                .TakeWhile(x => !x.Equals(4)).ToList();

            while (decoderMaskArray.Count < 32)
            {
                decoderMaskArray.Add(0);
            }

            var decoderMask = torch.from_array(decoderMaskArray.ToArray());

            Debug.Assert(encoderInput.shape[0] == 200);
            Debug.Assert(decoderMask.shape[0] == 32);
            Debug.Assert(encoderMask.shape[0] == 200);
            Debug.Assert(decoderInput.shape[0] == 32);

            return new Dictionary<string, torch.Tensor>()
            {
                {"EncoderInput", encoderInput},
                {"DecoderInput", decoderInput},
                {"EncoderMask", encoderMask},
                {"DecoderMask", decoderMask}
            };
        }

        public AARTNDataset(List<(List<Token>, double)> dataset,
            string tokenizerPath, int sequenceLength = 60) : base()
        {
            MLContext mlContext = new MLContext();

            //_dataset = dataset;

            //Make engine and save to object
            var tokenizerEngine = mlContext.Model.CreatePredictionEngine<ResidueData, Token>(
                mlContext.Model.Load(tokenizerPath, out var modelI));

            _tokenizer = tokenizerEngine;

            //Save padding token
            PaddingToken = _tokenizer.Predict(new Token() { Residue = "PAD", Id = 0 });
        }

        public AARTNDataset(List<(FeaturizedTokens[], double)> dataset) : base()
        {
            _dataset = dataset;

            //_tokenizer = tokenizer;

            ////Save padding token
            //PaddingToken = _tokenizer.Predict(new Token() { Residue = "PAD", Id = 0 });
        }

        public AARTNDataset(List<List<int>> dataset) : base()
        {
            _integerDataset = dataset;
        }

        //private List<(List<Token>, double)>? _dataset;
        private List<(FeaturizedTokens[], double)>? _dataset;
        private List<List<int>>? _integerDataset;
        private PredictionEngine<ResidueData, Token>? _tokenizer;
        public Token? PaddingToken;
    }
}
