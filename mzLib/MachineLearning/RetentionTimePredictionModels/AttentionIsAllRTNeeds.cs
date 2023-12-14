﻿using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MathNet.Numerics;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using TorchSharp;
using TorchSharp.Modules;

namespace MachineLearning.RetentionTimePredictionModels
{
    public class AttentionIsAllRTNeeds : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public AttentionIsAllRTNeeds() : base(nameof(AttentionIsAllRTNeeds))
        {

        }



        public override torch.Tensor forward(torch.Tensor input)
        {
            throw new NotImplementedException();
        }

        
    }

    internal class PositionalEncoder : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public PositionalEncoder(int dModel, int sequenceLenth, double dropout) : base(nameof(PositionalEncoder))
        {
            //Create vector of shape (sequenceLenth, dModel)
            var positionalEncoding = torch.zeros(sequenceLenth, dModel);
            //Create tensor of shape (sequenceLenth, 1)
            var position = torch.arange(0, sequenceLenth).unsqueeze(1);
            //Division term
            var divTerm = torch.exp(
                torch.arange(0, dModel, 2) * (-Math.Log(10000.0) / dModel));

            //Apply the sin to even indices and cos to odd indices
            var counter = 0;
            for (int i = 0; i < dModel; i += 2)
            {
                if(i.IsEven())
                    positionalEncoding[counter, i] = torch.sin(position * divTerm);
                else if (i.IsOdd())
                    positionalEncoding[counter, i] = torch.cos(position * divTerm);

                counter = counter + 1;
            }
            _positionalEncoding = positionalEncoding.unsqueeze(0); //becomes (1, sequenceLenth, dModel)
            this.register_buffer("positionalEncoding", _positionalEncoding);
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            input = input + this._positionalEncoding.requires_grad_(false);
            return _dropout.forward(input);
        }

        private Dropout _dropout = torch.nn.Dropout(0.1);
        private torch.Tensor _positionalEncoding;
    }

    internal class LayerNormalization : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public LayerNormalization(double eps = 1e-6) : base(nameof(LayerNormalization))
        {
            _eps = eps;
            _alpha = torch.nn.Parameter(torch.ones(1)); //multiplied
            _beta = torch.nn.Parameter(torch.zeros(1)); //addded
            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var mean = input.mean(new long[]{-1}, true);
            var std = input.std(-1, true);
            var norm = (input - mean) / (std + _eps);
            return _alpha * norm + _beta;
        }

        private double _eps;
        private Parameter _alpha;
        private Parameter _beta;
    }

    internal class FeedForwardBlock : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public FeedForwardBlock(int dModel, int dFF, double dropout = 0.1) : base(nameof(FeedForwardBlock))
        {
            _linear1 = torch.nn.Linear(dModel, dFF); //W1 and B1
            _linear2 = torch.nn.Linear(dFF, dModel); //W2 and B2
            _dropout = torch.nn.Dropout(dropout);

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            // (batchSize, sequenceLength, dModel) to (batchSize, sequenceLength, dFF) to (batchSize, sequenceLength, dModel)
            return _linear2.forward(_dropout.forward(torch.nn.functional.relu(_linear1.forward(input))));
        }

        private Linear _linear1;
        private Linear _linear2;
        private Dropout _dropout;
    }

    internal class MultiHeadAttentionBlock : torch.nn.Module<torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor>
    {
        public MultiHeadAttentionBlock(int dModel, int numHeads, int? mask, double dropout = 0.1) : base(nameof(MultiHeadAttentionBlock))
        {
            _dModel = dModel;
            _numHeads = numHeads;
            _dropout = torch.nn.Dropout(dropout);

            Debug.Assert(dModel % numHeads == 0);

            _dK = dModel / numHeads;

            _linearQ = torch.nn.Linear(dModel, dModel); //Wq
            _linearK = torch.nn.Linear(dModel, dModel); //Wk
            _linearV = torch.nn.Linear(dModel, dModel); //Wv
            _linearO = torch.nn.Linear(dModel, dModel); //Wo

            _mask = mask;

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor query, torch.Tensor key, torch.Tensor value, torch.Tensor mask)
        {
            // (BatchSize, SequenceLength, dModel) to (BatchSize, SequenceLength, dModel)
            // to (BatchSize, SequenceLength, numHeads, dK) to (BatchSize, numHeads, SequenceLength, dK)
            var q = _linearQ.forward(query).view(query.shape[0], query.shape[1], _numHeads, _dK).transpose(1, 2);
            var k = _linearK.forward(key).view(key.shape[0], key.shape[1], _numHeads, _dK).transpose(1, 2);
            var v = _linearV.forward(value).view(value.shape[0], value.shape[1], _numHeads, _dK).transpose(1, 2);

            var (x, attention) = Attention(q, k, v, _mask, _dropout);

            //(BatchSize, numHeads, SequenceLength, dK) to (BatchSize, SequenceLength, numHeads, dK) to (BatchSize, SequenceLength, dModel)
            x = x.transpose(1, 2).contiguous().view(x.shape[0], -1, _numHeads, _dK);

            //(Batch, SequenceLength, dModel) to (Batch, SequenceLength, dModel)
            return _linearO.forward(x);
        }

        private static (torch.Tensor, torch.Tensor) Attention(torch.Tensor q, torch.Tensor k, torch.Tensor v, torch.Tensor? mask, Dropout dropout = null)
        {
            var dK = q.shape[-1];
            //(BatchSize, numHeads, SequenceLength, dK) to (BatchSize, numHeads, SequenceLength, SequenceLength)
            var scores = torch.matmul(q, k.transpose(-2, -1)) / Math.Sqrt(dK);

            if(mask.shape != null)
                scores.masked_fill(mask == 0, -1e9);

            //(Batch, numHeads, SequenceLength, SequenceLength)
            var attention = torch.nn.functional.softmax(scores, -1);

            if (dropout != null)
                attention = dropout.forward(attention);

            return (torch.matmul(attention, v), scores);
        }

        private int _dModel;
        private int _numHeads;
        private int _dK;
        private Dropout _dropout;
        private Linear _linearQ;
        private Linear _linearK;
        private Linear _linearV;
        private Linear _linearO;
        private int? _mask;
    }

    internal class ResidualConnection : torch.nn.Module<torch.Tensor, Func<torch.Tensor, torch.Tensor>, torch.Tensor>
    {
        public ResidualConnection(Dropout dropout) : base(nameof(ResidualConnection))
        {
            _dropout = dropout;

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input, Func<torch.Tensor, torch.Tensor> subLayer)
        {
            return input + _dropout.forward(subLayer.Invoke((_norm.forward(input))));
        }

        private Dropout _dropout;
        private LayerNormalization _norm = new LayerNormalization();
    }

    internal class EncoderBlock : torch.nn.Module<torch.Tensor, torch.Tensor, torch.Tensor>
    {
        public EncoderBlock(MultiHeadAttentionBlock selfAttentionBlock, FeedForwardBlock feedForwardBlock, double dropout) : base(nameof(EncoderBlock))
        {
            _selfAttentionBlock = selfAttentionBlock;
            _feedForwardBlock = feedForwardBlock;
            _residualConnection = new ModuleList<ResidualConnection>(new ResidualConnection[] 
                { new ResidualConnection(torch.nn.Dropout(dropout)),
                    new ResidualConnection(torch.nn.Dropout(dropout))
                });

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input, torch.Tensor sourceMask)
        {
            Func<torch.Tensor, torch.Tensor> subLayer = x => _selfAttentionBlock.forward(x, x, x, sourceMask);

            input = _residualConnection[0].forward(input, subLayer);
            input = _residualConnection[1].forward(input, _feedForwardBlock.forward);
            return input;
        }

        private MultiHeadAttentionBlock _selfAttentionBlock;
        private FeedForwardBlock _feedForwardBlock;
        private ModuleList<ResidualConnection> _residualConnection;
    }

    internal class Encoder : torch.nn.Module<torch.Tensor, torch.Tensor, torch.Tensor>
    {
        public Encoder(ModuleList<EncoderBlock> layers) : base(nameof(Encoder))
        {
            _layers = layers;

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input, torch.Tensor mask)
        {
            foreach (var layer in _layers)
                input = layer.forward(input, mask);

            return _norm.forward(input);
        }


        private ModuleList<EncoderBlock> _layers;
        private LayerNormalization _norm = new LayerNormalization();
    }
}
