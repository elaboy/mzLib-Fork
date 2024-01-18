﻿using MachineLearning.Transformer;
using MachineLearning.TransformerComponents;
using TorchSharp;
using TorchSharp.Modules;

namespace MachineLearning.RetentionTimePredictionModels
{
    public static class AARTN
    {
        public static TransformerComponents.Transformer EnsambleModel(int sourceVocabSize, int targetVocabSize,
            int sourceSequenceLength, int targetSequenceLength, int dModel = 2708, int N = 6, int h = 2,
            double dropout=0.1, int dFF = 2048)
        {
            //Create the embedding layers
            var sourceEmbedding = new InputEmbeddings(dModel, sourceVocabSize);
            var targetEmbedding = new InputEmbeddings(dModel, targetVocabSize);

            //Create the positional encoding layers
            var sourcePosition = new PositionalEncoder(dModel, sourceSequenceLength,dropout);
            var targetPosition = new PositionalEncoder(dModel, targetSequenceLength,dropout);

            //Create the encoder blocks
            var encoderBlocks = new List<EncoderBlock>();
            for (int i = 0; i < N; i++)
            {
                var decoderSelfAttentionBlock = new MultiHeadAttentionBlock(dModel, h, dropout);
                var feedForwardBlock = new FeedForwardBlock(dModel, dFF, dropout);
                var encoderBlock = new EncoderBlock(decoderSelfAttentionBlock, feedForwardBlock, dropout);
                encoderBlocks.Add(encoderBlock);
            }

            //Create the decoder blocks
            var decoderBlocks = new List<DecoderBlock>();
            for (int i = 0; i < N; i++)
            {
                var decoderSelfAttentionBlock = new MultiHeadAttentionBlock(dModel, h, dropout);
                var encoderDecoderAttentionBlock = new MultiHeadAttentionBlock(dModel, h, dropout);
                var feedForwardBlock = new FeedForwardBlock(dModel, dFF, dropout);
                var decoderBlock = new DecoderBlock(decoderSelfAttentionBlock, encoderDecoderAttentionBlock,
                                       feedForwardBlock, dropout);
                decoderBlocks.Add(decoderBlock);
            }

            //Create the enconder and the decorder 
            var encoder = new Encoder(torch.nn.ModuleList<EncoderBlock>(encoderBlocks
                .Select(x => x).ToArray()));
            var decoder = new Decoder(torch.nn.ModuleList<DecoderBlock>(decoderBlocks
                .Select(x => x).ToArray()));

            //Create the projection layer
            var projectionLayer = new ProjectionLayer(dModel, targetVocabSize);

            //Create the transformer
            var transformer = new TransformerComponents.Transformer(encoder, decoder, sourceEmbedding, targetEmbedding, sourcePosition,
                               targetPosition, projectionLayer);

            return transformer;
        }
    }
}
