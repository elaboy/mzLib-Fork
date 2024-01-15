using MachineLearning;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MachineLearning.RetentionTimePredictionModels;
using MachineLearning.Transformer;
using MachineLearning.TransformerComponents;

namespace Test
{
    public class TestTransformer
    {
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

            //var tokens = Tokenizer.Tokenize(listOfInputs, @"D:/AI_Datasets/Tokenizer.zip");
        }

        [Test]
        public void TestAndTokenizeHela1()
        {
            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                               @"D:/AI_Datasets/Hela1_AllPSMs.psmtsv", out var warnings);

            List<(List<Tokenizer.Token>, double)> tokens = new();

            Parallel.ForEach(psms, psm =>
            {

                var psmTokens =Tokenizer.Tokenize(psm.FullSequence,
                    @"D:/AI_Datasets/Tokenizer.zip");

                var token = (psmTokens, psm.RetentionTime.Value);

                tokens.Add(token);
                Debug.WriteLine(psm.FullSequence + " " + tokens.Count);
            });

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
