using System.Diagnostics;
using System.Runtime.InteropServices;
using Easy.Common.Extensions;
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
            var rangeOfTokensIntegersToMask = Enumerable.Range(6, 18);

            //Encoder Mask
            var encoderMaskWithoutRT = maskedSourceTarget.Skip(3).ToList();
            encoderMaskWithoutRT.InsertRange(0, new List<int>(3){{5}});

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

            while (target.Count < 5)
            {
                target.Add(6);
            }

            var decoderInput = torch.from_array(target.ToArray());

            //Decoder Mask
            var decoderMaskArray = sourceTarget
                .TakeWhile(x => !x.Equals(4))
                .Take(new Range(1,6))
                .ToList();

            //while (decoderMaskArray.Count < 32)
            //{
            //    decoderMaskArray.Add(0);
            //}

            var decoderMask = torch.from_array(decoderMaskArray.ToArray());
            //Debug.Assert(encoderInput.shape[0] == 200);
            //Debug.Assert(decoderMask.shape[0] == 32);
            //Debug.Assert(encoderMask.shape[0] == 200);
            //Debug.Assert(decoderInput.shape[0] == 32);

            //label
            var rawLabel = sourceTarget
                .Skip(1)
                .TakeWhile(x => !x.Equals(2))
                .ToArray();

            var label = torch.from_array(rawLabel);

            Debug.WriteLine(encoderInput.ToString(TensorStringStyle.Julia));
            Debug.WriteLine(decoderInput.ToString(TensorStringStyle.Julia));
            Debug.WriteLine(encoderMask.ToString(TensorStringStyle.Julia));
            Debug.WriteLine(decoderMask.ToString(TensorStringStyle.Julia));
            Debug.WriteLine(label.ToString(TensorStringStyle.Julia));

            return new Dictionary<string, torch.Tensor>()
            {
                {"EncoderInput", encoderInput},
                {"DecoderInput", decoderInput},
                {"EncoderMask", encoderMask},
                {"DecoderMask", decoderMask},
                {"Label", torch.from_array(_integerDataset.ElementAt((int)index).ToArray())}
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
