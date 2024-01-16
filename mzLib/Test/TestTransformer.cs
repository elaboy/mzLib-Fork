using MachineLearning;
using MachineLearning.RetentionTimePredictionModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Test
{
    public class TestTransformer
    {
        [Test]
        public void TestTrainTokenizer()
        {
            Tokenizer.TrainTokenizer(@"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip",
                TokenizerModType.CommonBiologicalAndArtifacts);
        }

        [Test]
        public void TestTokenizer()
        {
            var listOfInputs = new List<string>()
            {
                "A", "C", "D", "E", "F", "G", "H", "I", "K",
                "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "Y"
            };

            //var tokens = Tokenizer.Tokenize(listOfInputs, @"D:/AI_Datasets/Tokenizer.zip");
        }

        [Test]
        public void TestAndTokenizeHela1()
        {
            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                               @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            List<(List<Tokenizer.Token>, double)> tokens = new();

            foreach(var psm in psms)
            {

                var psmTokens = Tokenizer.Tokenize(psm.FullSequence,
                    @"D:/AI_Datasets/tokenizerCommonBiologicalAndArtifacts.zip");

                var token = (psmTokens, psm.RetentionTime.Value);

                tokens.Add(token);
                Debug.WriteLine(psm.FullSequence + " " + tokens.Count);
            }

        }

        //[Test]
        //public void TestTraining()
        //{
        //    var psms =
        //        Readers.SpectrumMatchTsvReader
        //            .ReadPsmTsv(@"D:/AI_Datasets/Hela1_AllPSMs.psmtsv",
        //                out var warnings);

        //    var tokens = new List<(List<Tokenizer.Token>, double)>();

        //    Parallel.ForEach(psms, psm =>
        //    {

        //        var psmTokens = Tokenizer.Tokenize(psm.FullSequence,
        //            @"D:/AI_Datasets/Tokenizer.zip");

        //        var token = (psmTokens, psm.RetentionTime.Value);

        //        tokens.Add(token);
        //        Debug.WriteLine(psm.FullSequence + " " + tokens.Count);
        //    });

        //    var model = Transformer.BuildAARTN(tokens.First().Item1.First().Features.Length,
        //        1, 60, 1);

        //}

        [Test]
        public void TestEnsambleModel()
        {
            var model = AARTN.EnsambleModel(21, 21,
                25, 25);
        }


    }
}
