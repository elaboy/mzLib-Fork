using System.Collections.Generic;
using MachineLearning.RetentionTimePredictionModels;
using NUnit.Framework;
using static TorchSharp.torch.utils;

namespace Test
{
    public class TestAttentionIsAllRTNeeds
    {
        [Test]
        public void TestTokenizer()
        {
            var model = new AttentionIsAllRTNeeds();

        }

        [Test]
        public void TestTrainTokenizer()
        {
            Tokenizer.TrainTokenizer(@"F:/Research/Data/AttentionIsAllRTNeeds/tokenizer.zip");
        }

        [Test]
        public void TestTokenize()
        {
            var listOfInputs = new List<string>()
            {
                "A", "C", "D", "E", "F", "G", "H", "I", "K",
                "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "Y"
            };

            var tokens = Tokenizer.Tokenize(listOfInputs, @"F:\Research\Data\AttentionIsAllRTNeeds/Tokenizer.zip");
        }
    }
}
