using Proteomics.PSM;

namespace MachineLearning
{
    public static class TokenGeneration
    {
        public const string RETENTION_TIME_START_TOKEN = "<RT>";
        public const string PADDING_TOKEN = "<PAD>";
        public const string END_OF_RETENTION_TIME_TOKEN = "</RT>";
        public const string START_OF_SEQUENCE_TOKEN = "<SOS>";
        public const string END_OF_SEQUENCE_TOKEN = "<EOS>";
        public static List<string> TokenizeRetentionTimeWithFullSequence(PsmFromTsv psm, int tokenLength)
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

            // Pad the rest of the tokens
            var paddingRequired = (tokenLength - tokenList.Count) - 1;

            for (int i = 0; i < paddingRequired; i++)
            {
                tokenList.Add(PADDING_TOKEN);
            }
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
    }
}
