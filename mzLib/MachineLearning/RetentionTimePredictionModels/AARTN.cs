using MachineLearning.TransformerComponents;

namespace MachineLearning.RetentionTimePredictionModels
{
    public static class AARTN
    {
        public static TransformerComponents.Transformer EnsableModel()
        {
            return new TransformerComponents.Transformer(new Encoder());
        }
    }
}
