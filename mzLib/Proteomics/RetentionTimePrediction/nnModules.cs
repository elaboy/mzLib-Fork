using System.Linq;
using System.Reflection;
using TorchSharp.Modules;
using TorchSharp;

namespace Proteomics.RetentionTimePrediction;

public class nnModules
{
    public class Chronologer : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private long _vectorLength { get; set; }
        private long _nStates { get; set; }
        private long _embedDim { get; set; }
        private long _nBlocks { get; set; }
        private long _kernel { get; set; }
        private long _dropRate { get; set; }
        private string _actFx { get; set; }

        public Chronologer(long vectorLength, long nStates,
            long embedDim, long nBlocks, long kernel,
            long dropRate, string actFx) : base(nameof(Chronologer))
        {

            _vectorLength = vectorLength;
            _nStates = nStates;
            _embedDim = embedDim;
            _nBlocks = nBlocks;
            _kernel = kernel;
            _dropRate = dropRate;
            _actFx = actFx;

            //InitiateModel();
            torch.nn.init.kaiming_normal_(seq_embed.weight,
                nonlinearity: torch.nn.init.NonlinearityType.Linear);
            torch.nn.init.xavier_normal_(output.weight);
            torch.nn.init.constant_(output.bias, 0); //.data?


            
            //adds resnet_blocks to the model
            resnet_blocks.add_module("0", new resnet_block(1));
            resnet_blocks.add_module("1", new resnet_block(2));
            resnet_blocks.add_module("2", new resnet_block(3));


            RegisterComponents();

        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var x = seq_embed.forward(input).transpose(1, -1);
            x = resnet_blocks.forward(x);
            x = dropout.forward(x);
            x = flatten.forward(x);
            return output.forward(x);
        }

        //Network layers
        public Embedding seq_embed = torch.nn.Embedding(55, 64, 0);

        public torch.nn.Module<torch.Tensor, torch.Tensor> resnet_blocks = torch.nn.Sequential();

        public Dropout dropout = torch.nn.Dropout((long)0.01);

        public Flatten flatten = torch.nn.Flatten();
        public Linear output = torch.nn.Linear(52 * 64, 1);

    }
    public class resnet_block : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private long _in_channels { get; set; }
        private long _out_channels { get; set; }
        public static long dil { get; set; }


        public Sequential process_blocks  = torch.nn.Sequential();


        public torch.nn.Module<torch.Tensor, torch.Tensor> shortcut { get; set; }

        public Identity term_block = torch.nn.Identity();

        public resnet_block(long dilation, long in_channels = 64, long out_channels = 64) : base(nameof(resnet_block))
        {
            _in_channels = in_channels;
            _out_channels = out_channels;
            dil = dilation;

            shortcut = new ConvolutionUnit(dil, false);

            process_blocks.add_module("0", new ConvolutionUnit(dil, true));
            process_blocks.add_module("1", torch.nn.ReLU());
            process_blocks.add_module("2", new ConvolutionUnit(dil, false));
            process_blocks.add_module("3", torch.nn.ReLU());
            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var residual = input;
            if (_in_channels != _out_channels)
                residual = shortcut.forward(input);
            input = process_blocks.forward(input);
            input = term_block.forward(input);
            input = input + residual;
            // var activation = activationFunctions[_actFX];
            input = torch.nn.functional.relu(input);
            return input;
        }
    }


    public class ConvolutionUnit : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private bool _kernel_1 { get; set; }
        public ConvolutionUnit(long dRate, bool kernel_1=true) : base(nameof(ConvolutionUnit))
        {
            _kernel_1 = kernel_1;
            if (kernel_1)
            {
                if (kernel_1 && dRate == 1)
                {

                    var conv_layer_kernel_1_1 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 1);
                    var norm_layer_1 = torch.nn.BatchNorm1d(64);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_1_1.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);
                    torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                    torch.nn.init.constant_(norm_layer_1.bias, 0.0);

                    conv_unit.add_module("0", conv_layer_kernel_1_1);
                    conv_unit.add_module("1", norm_layer_1);
                }
                else if (kernel_1 && dRate == 2)
                {
                    var conv_layer_kernel_1_2 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 2);
                    var norm_layer_1 = torch.nn.BatchNorm1d(64);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_1_2.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);
                    torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                    torch.nn.init.constant_(norm_layer_1.bias, 0.0);

                    conv_unit.add_module("0", conv_layer_kernel_1_2);
                    conv_unit.add_module("1", norm_layer_1);
                }
                else if (kernel_1 && dRate == 3)
                {
                    Conv1d conv_layer_kernel_1_3 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 3);
                    var norm_layer_1 = torch.nn.BatchNorm1d(64);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_1_3.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);
                    torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                    torch.nn.init.constant_(norm_layer_1.bias, 0.0);

                    conv_unit.add_module("0", conv_layer_kernel_1_3);
                    conv_unit.add_module("1", norm_layer_1);
                }
            }
            else
            {
                if (dRate == 1)
                {
                    Conv1d conv_layer_kernel_7_1 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 1);
                    var norm_layer_1 = torch.nn.BatchNorm1d(64);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_7_1.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);
                    torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                    torch.nn.init.constant_(norm_layer_1.bias, 0.0);

                    conv_unit.add_module("0", conv_layer_kernel_7_1);
                    conv_unit.add_module("1", norm_layer_1);
                }
                else if (dRate == 2)
                {
                    Conv1d conv_layer_kernel_7_2 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 2);
                    var norm_layer_1 = torch.nn.BatchNorm1d(64);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_7_2.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);
                    torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                    torch.nn.init.constant_(norm_layer_1.bias, 0.0);

                    conv_unit.add_module("0", conv_layer_kernel_7_2);
                    conv_unit.add_module("1", norm_layer_1);
                }
                else if (dRate == 3)
                {
                    Conv1d conv_layer_kernel_7_3 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 3);
                    var norm_layer_1 = torch.nn.BatchNorm1d(64);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_7_3.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);
                    torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                    torch.nn.init.constant_(norm_layer_1.bias, 0.0);

                    conv_unit.add_module("0", conv_layer_kernel_7_3);
                    conv_unit.add_module("1", norm_layer_1);
                }
            }


            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var x = conv_unit.forward(input);
            return torch.nn.functional.relu(x);
        }

        public Sequential conv_unit = torch.nn.Sequential();

    }

    public class ResnetUnit : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public ResnetUnit(long dRate, long kernel) : base(
            nameof(ResnetUnit))
        {
            if (kernel == 1)
            {
                resnet_unit = new ConvolutionUnit(dRate, true);
            }
            else if (kernel == 7)
            {
                resnet_unit =  new ConvolutionUnit(dRate, false);
            }

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            return resnet_unit.forward(input);
        }
        public torch.nn.Module<torch.Tensor, torch.Tensor> resnet_unit { get; set; }
    }
}