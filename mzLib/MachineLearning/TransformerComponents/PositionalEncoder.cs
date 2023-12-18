using MathNet.Numerics;
using TorchSharp;
using TorchSharp.Modules;

namespace MachineLearning.TransformerComponents;

public class PositionalEncoder : torch.nn.Module<torch.Tensor, torch.Tensor>
{
    public PositionalEncoder(int dModel, int sequenceLenth, double dropout) : base(nameof(PositionalEncoder))
    {
        //Create vector of shape (sequenceLenth, dModel)
        var positionalEncoding = torch.zeros(sequenceLenth, dModel);
        //Create tensor of shape (sequenceLenth, 1)
        var position = torch.arange(0, sequenceLenth).unsqueeze(1);
        //Division term
        var divTerm = torch.exp(
            torch.arange(0, dModel, 2) * (-Math.Log(10000.0) / dModel));

        //Apply the sin to even indices and cos to odd indices
        var counter = 0;
        for (int i = 0; i < dModel; i += 2)
        {
            if (i.IsEven())
                positionalEncoding[counter, i] = torch.sin(position * divTerm);
            else if (i.IsOdd())
                positionalEncoding[counter, i] = torch.cos(position * divTerm);

            counter = counter + 1;
        }
        _positionalEncoding = positionalEncoding.unsqueeze(0); //becomes (1, sequenceLenth, dModel)
        register_buffer("positionalEncoding", _positionalEncoding);
    }

    public override torch.Tensor forward(torch.Tensor input)
    {
        input = input + _positionalEncoding.requires_grad_(false);
        return _dropout.forward(input);
    }

    private Dropout _dropout = torch.nn.Dropout(0.1);
    private torch.Tensor _positionalEncoding;
}