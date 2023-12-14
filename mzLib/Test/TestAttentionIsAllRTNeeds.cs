using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MachineLearning;
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
            Tokenizer.TrainTokenizer(@"D:/AI_Datasets/tokenizer.zip");
        }

        [Test]
        public void TestTokenize()
        {
            var listOfInputs = new List<string>()
            {
                "A", "C", "D", "E", "F", "G", "H", "I", "K",
                "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "Y"
            };

            var tokens = Tokenizer.Tokenize(listOfInputs, @"D:/AI_Datasets/Tokenizer.zip");
        }

        [Test]
        public void TestAndTokenizeHela1()
        {
            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                               @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            List<(List<Tokenizer.Token>, double)> tokens = new ();

            Parallel.ForEach(psms, psm =>
            {
                var psmTokens = (psm.FullSequence.Select(x => Tokenizer.Tokenize(new() { x.ToString() },
                    @"D:/AI_Datasets/Tokenizer.zip")));
                var token = (psmTokens.Select(x => x.First()).ToList(), psm.RetentionTime.Value);

                tokens.Add(token);
            });

        }
    }
}
