using TorchSharp;
using TorchSharp.Modules;

namespace MachineLearning.TransformerComponents;

public class InputEmbeddings : torch.nn.Module<torch.Tensor, torch.Tensor>
{
    public InputEmbeddings(int dModel, int vocabSize) : base(nameof(InputEmbeddings))
    {
        _embedding = torch.nn.Embedding(vocabSize, dModel);
        _dModel = dModel;
        _vocabSize = vocabSize;

        RegisterComponents();
    }

    public override torch.Tensor forward(torch.Tensor input)
    {
        return _embedding.forward(input.to_type(torch.ScalarType.Int64)) * Math.Sqrt(_dModel);
    }

    private Embedding _embedding;
    private int _dModel;
    private int _vocabSize;
}