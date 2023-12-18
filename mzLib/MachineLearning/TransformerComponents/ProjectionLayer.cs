using TorchSharp;
using TorchSharp.Modules;

namespace MachineLearning.Transformer;

public class ProjectionLayer : torch.nn.Module<torch.Tensor, torch.Tensor>
{
    public ProjectionLayer(int dModel, int vocabSize) : base(nameof(ProjectionLayer))
    {
        _projectionLayer = torch.nn.Linear(dModel, vocabSize);

        RegisterComponents();
    }

    public override torch.Tensor forward(torch.Tensor input)
    {
        //(Batch, SequenceLength, dModel) to (Batch, SequenceLength, vocabSize)

        return torch.nn.functional.log_softmax(_projectionLayer.forward(input), -1);
    }

    private Linear _projectionLayer;
}