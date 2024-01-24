using System.Data;
using Easy.Common.Extensions;
using Proteomics.PSM;
using TorchSharp;
using static TorchSharp.torch.utils.data;

namespace MachineLearning
{
    public static class TokenGeneration
    {
        public const string RETENTION_TIME_START_TOKEN = "<RT>";
        public const string PADDING_TOKEN = "<PAD>";
        public const string END_OF_RETENTION_TIME_TOKEN = "</RT>";
        public const string START_OF_SEQUENCE_TOKEN = "<SOS>";
        public const string END_OF_SEQUENCE_TOKEN = "<EOS>";
        public const string MASKING_TOKEN = "<MASK>";

        public static List<string> TokenizeRetentionTimeWithFullSequence(PsmFromTsv psm)
        {
            var retentionTime = psm.RetentionTime;
            var fullSequence = psm.FullSequence;

            List<string> tokenList = new();
            tokenList.Add(RETENTION_TIME_START_TOKEN);
            foreach (var token in NumericalTokenizer(retentionTime.Value))
            {
                tokenList.Add(token);
            }

            tokenList.Add(END_OF_RETENTION_TIME_TOKEN);
            tokenList.Add(START_OF_SEQUENCE_TOKEN);
            var fullSequenceSplit = fullSequence.Split('[', ']');
            foreach (var item in fullSequenceSplit)
            {
                if (!item.Contains(" "))
                {
                    foreach (var residue in item)
                    {
                        tokenList.Add(residue.ToString());
                    }
                }
                else
                {
                    var splitByColon = item.Split(':');
                    tokenList.Add(splitByColon[1]);
                }
            }
            //tokenList.Add(fullSequence);

            //// Pad the rest of the tokens
            //var paddingRequired = (tokenLength - tokenList.Count) - 1;

            //for (int i = 0; i < paddingRequired; i++)
            //{
            //    tokenList.Add(PADDING_TOKEN);
            //}
            tokenList.Add(END_OF_SEQUENCE_TOKEN);

            return tokenList;
        }

        public static string[] NumericalTokenizer(double number)
        {
            var numberAsString = number.ToString();
            var integerAndDecimalsSplit = numberAsString.Split('.');

            var integerPart = integerAndDecimalsSplit[0];
            var integerPartLength = integerPart.Length;

            var decimalPart = integerAndDecimalsSplit[1];
            var decimalPartLength = decimalPart.Length;

            string[] tokens = new string[integerPartLength + decimalPartLength];

            var positiveToZeroCounter = integerPartLength - 1;

            for (int i = 0; i < integerPartLength; i++)
            {
                // Add a period at the end of the integer part
                if (positiveToZeroCounter == 0)
                {
                    tokens[i] = integerPart[i].ToString() + "_" +
                                (positiveToZeroCounter--).ToString() + ".";
                    break;
                }

                tokens[i] = integerPart[i].ToString() + "_" + (positiveToZeroCounter--).ToString();
            }

            var negativeCounter = -1;

            for (int i = 0; i < decimalPartLength; i++)
            {
                tokens[integerPartLength + i] = decimalPart[i].ToString() + "_" + (negativeCounter--).ToString();
            }

            return tokens;
        }

        public static torch.Tensor PaddingTensor(torch.Tensor tensor, int desiredTensorLength)
        {
            if (tensor.shape[0] != desiredTensorLength)
            {
                var padsToAdd = desiredTensorLength - tensor.shape[0];
                var paddingTensor = torch.zeros(padsToAdd);

                var paddedTensor =  torch.concat(new List<torch.Tensor>(){tensor, paddingTensor})
                    .to_type(torch.ScalarType.Int32);

                paddedTensor[1, -1] = paddedTensor[1, desiredTensorLength-padsToAdd];
            }

            return torch.zeros(1);

        }

        public static List<int> PaddingIntegerList(List<int> integerList, int paddingInteger, int desiredListLength)
        {
            if (integerList.Count < desiredListLength)
            {
                integerList.RemoveAt(integerList.Count-1); //remove end of sequence token
                
                var padsToAdd = (desiredListLength - integerList.Count) - 1; //the -1 is to leave space for the end of sequence token
                
                for (int i = 0; i < padsToAdd; i++)
                {
                    integerList.Add(paddingInteger);
                }

                integerList.Add(3); //end of sequence token id
            }

            return integerList;
        }

        public static (List<List<int>>, List<List<int>>, List<List<int>>)
            TrainValidateTestSplit(List<List<int>> listOfTokenId, double trainingFraction = 0.8,
                double testingFraction = 0.1, double validationFraction = 0.1)
        {
            var randomizedData = listOfTokenId.Randomize().ToList();

            var trainingSet = randomizedData
                .Take((int)(randomizedData.Count * (1 - validationFraction)))
                .ToList();

            var validationSet = randomizedData
                .Skip((int)(randomizedData.Count * (1 - validationFraction)))
                .Take((int)(randomizedData.Count * (1 - testingFraction)))
                .ToList();

            var testSet = randomizedData
                .Skip((int)(randomizedData.Count * (1 - validationFraction)))
                .ToList();

            return (trainingSet, validationSet, testSet);
        }
    }
}
