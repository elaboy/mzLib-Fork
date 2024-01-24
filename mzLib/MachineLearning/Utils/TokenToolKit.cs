using Easy.Common.Extensions;
using Proteomics.PSM;
using TorchSharp;

namespace MachineLearning.Utils
{
    /// <summary>
    /// Class to hold static methods for tokenization/encoding/decoding 
    /// </summary>
    public static class TokenToolKit
    {
        //Current constant comments are refering to the VocabularyForTransformerUniprot_V2.csv file Token ids
        public const string PADDING_TOKEN = "<PAD>";               //0
        public const string RETENTION_TIME_START_TOKEN = "<RT>";   //1
        public const string END_OF_RETENTION_TIME_TOKEN = "</RT>"; //2
        public const string END_OF_SEQUENCE_TOKEN = "<EOS>";       //3
        public const string START_OF_SEQUENCE_TOKEN = "<SOS>";     //4
        public const string MASKING_TOKEN = "<MASK>";              //5

        /// <summary>
        /// Returns a list of tokens for a given retention time.
        /// </summary>
        /// <param name="psm"></param>
        /// <returns>
        /// List of tokens with the following format:
        /// &lt;RT&gt;, 1_1, 1_0., 1_-1, &lt;/RT&gt;, &lt;SOS&gt;, P, E, P, T, I, D, E, &lt;EOS&gt; 
        /// </returns>
        public static List<string> GetRetentionTimeWithFullSequenceAsTokens(PsmFromTsv psm)
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

            tokenList.Add(END_OF_SEQUENCE_TOKEN);

            return tokenList;
        }

        /// <summary>
        /// <para>
        /// Takes a double and returns a string array of tokens representing the number.
        /// For example:
        /// 12.3 = [1_1, 2_0., 3_-1]
        /// </para>>
        /// Reference: <see href="https://www.nature.com/articles/s42256-023-00639-z#Sec12" />
        /// </summary> todo: Make this a fixed array size using pads in this step, not later so every rt has the same expected size
        /// <param name="number"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds Padding tokens before the end of sequence token.
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="padsToAdd"></param>
        public static void AddPaddingToTokenList(List<string> tokenList, int padsToAdd)
        {
            if (tokenList.Count < padsToAdd)
            {
                tokenList.RemoveAt(tokenList.Count - 1);
                
                for (int i = 0; i < padsToAdd; i++)
                {
                    tokenList.Add(PADDING_TOKEN);
                }
            }
            tokenList.Add(END_OF_SEQUENCE_TOKEN);
        }

        /// <summary>
        /// Resize list of tokens by removing padding tokens until the desired length is reached.
        /// If the desired length is less than the current length, the list is not modified.
        /// If the desired length is greater than the current length, the same list will be returned without modifications.
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="newTokenListLength"></param>
        public static void ResizeByRemovingPaddingTokens(List<string> tokenList, int newTokenListLength)
        {
            tokenList.RemoveAll(x => x.Equals(PADDING_TOKEN));
        }

        /// <summary>
        /// Padds a torch.Tensor with zeros until the desired length is reached. Zeros are the padding token id.
        /// todo: Check if this method returns EOS token at the end of the tensor instead of a padding tensor.
        /// </summary>
        /// <param name="tensor"></param>
        /// <param name="desiredTensorLength"></param>
        /// <returns></returns>
        public static torch.Tensor PaddingTensor(torch.Tensor tensor, int desiredTensorLength)
        {
            if (tensor.shape[0] != desiredTensorLength)
            {
                var padsToAdd = desiredTensorLength - tensor.shape[0];
                var paddingTensor = torch.zeros(padsToAdd);

                var paddedTensor = torch.concat(new List<torch.Tensor>() { tensor, paddingTensor })
                    .to_type(torch.ScalarType.Int32);

                paddedTensor[1, -1] = paddedTensor[1, desiredTensorLength - padsToAdd];
            }

            return torch.zeros(1);

        }

        public static List<int> PaddingIntegerList(List<int> integerList, int paddingInteger, int desiredListLength)
        {
            if (integerList.Count < desiredListLength)
            {
                integerList.RemoveAt(integerList.Count - 1); //remove end of sequence token

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
