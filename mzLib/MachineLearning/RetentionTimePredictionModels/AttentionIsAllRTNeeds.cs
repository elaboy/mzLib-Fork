using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using TorchSharp;

namespace MachineLearning.RetentionTimePredictionModels
{
    public class AttentionIsAllRTNeeds : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public AttentionIsAllRTNeeds() : base(nameof(AttentionIsAllRTNeeds))
        {

        }



        public override torch.Tensor forward(torch.Tensor input)
        {
            throw new NotImplementedException();
        }

        
    }

    internal class PositionalEncoder : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public PositionalEncoder() : base(nameof(PositionalEncoder))
        {
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            throw new NotImplementedException();
        }
    }
}
