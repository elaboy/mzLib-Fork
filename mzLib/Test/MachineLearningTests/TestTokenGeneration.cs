using MachineLearning;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Test.MachineLearningTests
{
    public class TestTokenGeneration
    {
        [Test]
        public void TestNumericalTokenizer()
        {
            var tokens = MachineLearning.TokenGeneration.NumericalTokenizer(123.456);
            var a = 0;
        }

        [Test]
        public void TestTokenizeRetentionTimeWithFullSequence()
        {
            var psms = Readers.SpectrumMatchTsvReader.ReadPsmTsv(
                @"MachineLearningTests/AllPeptides.psmtsv", out var warnings);

            List<List<string>> tokens = new();

            foreach (var psm in psms.Where(x => x.AmbiguityLevel=="1"))
            {
                tokens.Add(TokenGeneration.TokenizeRetentionTimeWithFullSequence(psm, 160));
            }

            var a = 0;
        }

    }
}
