using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TorchSharp;
using TorchSharp.Modules;

namespace Proteomics.RetentionTimePrediction;

public class nnModules
{

    public class RawChronologer : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public RawChronologer() : base(nameof(RawChronologer))
        {

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor x)
        {
            var input  = seq_embed.forward(x).transpose(1, -1);

            // Debug.WriteLine(input.ToString(TensorStringStyle.Julia));
            var residual = input.clone();
            if (1 == 0)
            {
                residual = conv_layer_3.forward(input);//shortcut
                residual = norm_layer_3.forward(residual);
            }
            input = conv_layer_1.forward(input); //renet_block
            input = norm_layer_1.forward(input);
            input = relu.forward(input);
            input = conv_layer_2.forward(input);
            input = norm_layer_2.forward(input);
            input = relu.forward(input);
            input = term_block.forward(input);
            input = residual + input;
            input = relu.forward(input);


            residual = input.clone();
            // Debug.WriteLine(input.ToString(TensorStringStyle.Julia));

            if (1 == 0)
            {
                residual = conv_layer_6.forward(input); //shortcut
                residual = norm_layer_6.forward(residual);
            }
            input = conv_layer_4.forward(input);//renet_block
            input = norm_layer_4.forward(input);
            input = relu.forward(input);
            input = conv_layer_5.forward(input);
            input = norm_layer_5.forward(input);
            input = relu.forward(input);
            input = term_block.forward(input);
            input = residual + input;
            input = relu.forward(input);

            residual = input.clone();
            // Debug.WriteLine(input.ToString(TensorStringStyle.Julia));

            if (1 == 0)
            {
                residual = conv_layer_9.forward(input); //shortcut
                residual = norm_layer_9.forward(residual);
            }
            input = conv_layer_7.forward(input);//renet_block
            input = norm_layer_7.forward(input);
            input = term_block.forward(input);
            input = relu.forward(input);
            // residual = input;
            input = conv_layer_8.forward(input);
            input = norm_layer_8.forward(input);
            input = relu.forward(input);
            input = term_block.forward(input);
            input = residual + input;
            // input = residual + input;
            // input = relu.forward(input);

            // input = term_block.forward(input);
            input = relu.forward(input);
            input = dropout.forward(input);
            input = flatten.forward(input);
            input = output.forward(input);
            return input;
        }

        //All Modules
        private Embedding seq_embed = torch.nn.Embedding(55, 64, 0);
        
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_1 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 1);
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_2 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 1);
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_3 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 1);//shortcut
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_4 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 2);
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_5 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 2);
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_6 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 2);//shortcut
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_7 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 3);
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_8 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 3);
        private torch.nn.Module<torch.Tensor, torch.Tensor> conv_layer_9 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 3); //shortcut
        
        private torch.nn.Module<torch.Tensor, torch.Tensor> term_block = torch.nn.Identity();
        private torch.nn.Module<torch.Tensor, torch.Tensor> relu           = torch.nn.ReLU(true);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_1 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_2 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_3 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_4 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_5 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_6 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_7 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_8 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> norm_layer_9 = torch.nn.BatchNorm1d(64);
        private torch.nn.Module<torch.Tensor, torch.Tensor> dropout = torch.nn.Dropout(0.01);
        private torch.nn.Module<torch.Tensor, torch.Tensor> flatten = torch.nn.Flatten(1);
        private torch.nn.Module<torch.Tensor, torch.Tensor> output = torch.nn.Linear(52 * 64, 1);


    }

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
            torch.nn.init.constant_(output.bias, 0.0); //.data?



            //adds resnet_blocks to the model
            resnet_blocks.Add(new resnet_block(1,0));
            resnet_blocks.Add(new resnet_block(2,1));
            resnet_blocks.Add(new resnet_block(3, 2));


            RegisterComponents();

        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            input = seq_embed.forward(input).transpose(0, -1);
            input = resnet_blocks[0].forward(input);
            input = resnet_blocks[1].forward(input);
            input = resnet_blocks[2].forward(input);
            input = dropout.forward(input);
            input = flatten.forward(input);
            return output.forward(input);
        }

        //Network layers
        public Embedding seq_embed = torch.nn.Embedding(55, 64, 0);

        public ModuleList<torch.nn.Module<torch.Tensor, torch.Tensor>> resnet_blocks = new();

        public Dropout dropout = torch.nn.Dropout(0.01);

        public Flatten flatten = torch.nn.Flatten(0);
        public Linear output = torch.nn.Linear(52 * 64, 1);

    }
    public class resnet_block : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private long _in_channels { get; set; }
        private long _out_channels { get; set; }
        public static long dil { get; set; }


        public Sequential process_blocks = torch.nn.Sequential();


        public Sequential shortcut = torch.nn.Sequential();

        public Identity term_block = torch.nn.Identity();

        public resnet_block(long dilation, int block_number, long in_channels = 64, long out_channels = 64) : base(nameof(resnet_block))
        {
            _in_channels = in_channels;
            _out_channels = out_channels;
            dil = dilation;
            if (block_number == 0)
            {
                var norm_layer_1 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_1.weight, 1.0);
                torch.nn.init.constant_(norm_layer_1.bias, 0.0);
                process_blocks.add_module("process_block_1", new ConvolutionUnit(dil, "conv_layer_1", true));
                // process_blocks.add_module("process_block_2", new ConvolutionUnit(dil, "conv_layer_2", false));
                process_blocks.add_module("norm_layer_1", norm_layer_1);
                //process_blocks.add_module("relu_1", torch.nn.ReLU(true));


                var norm_layer_2 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_2.weight, 1.0);
                torch.nn.init.constant_(norm_layer_2.bias, 0.0);
                process_blocks.add_module("process_block_3", new ConvolutionUnit(dil, "conv_layer_3", false));
                // process_blocks.add_module("process_block_4", new ConvolutionUnit(dil, "conv_layer_4", false));
                process_blocks.add_module("norm_layer_2", norm_layer_2);
                //process_blocks.add_module("relu_2", torch.nn.ReLU(true));


                var norm_layer_3 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_3.weight, 1.0);
                torch.nn.init.constant_(norm_layer_3.bias, 0.0);
                shortcut.add_module("shortcut_1", new ConvolutionUnit(dil, "conv_layer_5", true));
                // shortcut.add_module("shortcut_2", new ConvolutionUnit(dil, "conv_layer_6", false));
                shortcut.add_module("norm_layer_3", norm_layer_3);

            }
            else if (block_number == 1)
            {
                var norm_layer_4 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_4.weight, 1.0);
                torch.nn.init.constant_(norm_layer_4.bias, 0.0);
                process_blocks.add_module("process_block_5", new ConvolutionUnit(dil, "conv_layer_7", true));
                // process_blocks.add_module("process_block_6", new ConvolutionUnit(dil, "conv_layer_8", false));
                process_blocks.add_module("norm_layer_4", norm_layer_4);
                //process_blocks.add_module("relu_3", torch.nn.ReLU(true));


                var norm_layer_5 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_5.weight, 1.0);
                torch.nn.init.constant_(norm_layer_5.bias, 0.0);
                process_blocks.add_module("process_block_7", new ConvolutionUnit(dil, "conv_layer_9", false));
                // process_blocks.add_module("process_block_8", new ConvolutionUnit(dil, "conv_layer_10", false));
                process_blocks.add_module("norm_layer_5", norm_layer_5);
                //process_blocks.add_module("relu_4", torch.nn.ReLU(true));

                var norm_layer_6 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_6.weight, 1.0);
                torch.nn.init.constant_(norm_layer_6.bias, 0.0);
                shortcut.add_module("shortcut_3", new ConvolutionUnit(dil, "conv_layer_11", true));
                // shortcut.add_module("shortcut_4", new ConvolutionUnit(dil, "conv_layer_12", false));
                shortcut.add_module("norm_layer_6", norm_layer_6);
            }
            else if (block_number == 2)
            {
                var norm_layer_7 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_7.weight, 1.0);
                torch.nn.init.constant_(norm_layer_7.bias, 0.0);
                process_blocks.add_module("process_block_9", new ConvolutionUnit(dil, "conv_layer_13", true));
                // process_blocks.add_module("process_block_10", new ConvolutionUnit(dil, "conv_layer_14", false));
                process_blocks.add_module("norm_layer_7", norm_layer_7);
                //process_blocks.add_module("relu_5", torch.nn.ReLU(true));


                var norm_layer_8 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_8.weight, 1.0);
                torch.nn.init.constant_(norm_layer_8.bias, 0.0);
                process_blocks.add_module("process_block_11", new ConvolutionUnit(dil, "conv_layer_15", false));
                // process_blocks.add_module("process_block_12", new ConvolutionUnit(dil, "conv_layer_16", false));
                process_blocks.add_module("norm_layer_8", norm_layer_8);
                //process_blocks.add_module("relu_6", torch.nn.ReLU(true));

                var norm_layer_9 = torch.nn.BatchNorm1d(64);
                torch.nn.init.constant_(norm_layer_9.weight, 1.0);
                torch.nn.init.constant_(norm_layer_9.bias, 0.0);
                shortcut.add_module("shortcut_5", new ConvolutionUnit(dil, "conv_layer_17", true));
                // shortcut.add_module("shortcut_6", new ConvolutionUnit(dil, "conv_layer_18", false));
                shortcut.add_module("norm_layer_9", norm_layer_9);
            }

            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var residual = input.clone();
            if (_in_channels != _out_channels)
                residual = shortcut.forward(input);
            input = process_blocks.forward(input);
            input = term_block.forward(input);
            input = input + residual;
            // var activation = activationFunctions[_actFX];
            input = relu.forward(input);
            return input;
        }
        private ReLU relu = torch.nn.ReLU(true);
    }


    public class ConvolutionUnit : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private bool _kernel_1 { get; set; }
        public ConvolutionUnit(long dRate, string conv_layer_name, bool kernel_1 = true) : base(nameof(ConvolutionUnit))
        {
            _kernel_1 = kernel_1;
            if (kernel_1)
            {
                if (kernel_1 && dRate == 1)
                {
                    var conv_layer_kernel_1_1 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 1);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_1_1.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);

                    resnet_block.add_module(conv_layer_name, conv_layer_kernel_1_1);
                }
                else if (kernel_1 && dRate == 2)
                {
                    var conv_layer_kernel_1_2 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 2);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_1_2.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);

                    resnet_block.add_module(conv_layer_name, conv_layer_kernel_1_2);

                }
                else if (kernel_1 && dRate == 3)
                {
                    Conv1d conv_layer_kernel_1_3 = torch.nn.Conv1d(64, 64, 1, Padding.Same, dilation: 3);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_1_3.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);

                    resnet_block.add_module(conv_layer_name, conv_layer_kernel_1_3);
                }
            }
            else
            {
                if (dRate == 1)
                {
                    Conv1d conv_layer_kernel_7_1 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 1);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_7_1.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);

                    resnet_block.add_module(conv_layer_name, conv_layer_kernel_7_1);
                }
                else if (dRate == 2)
                {
                    Conv1d conv_layer_kernel_7_2 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 2);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_7_2.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);

                    resnet_block.add_module(conv_layer_name, conv_layer_kernel_7_2);
                }
                else if (dRate == 3)
                {
                    Conv1d conv_layer_kernel_7_3 = torch.nn.Conv1d(64, 64, 7, Padding.Same, dilation: 3);

                    torch.nn.init.kaiming_normal_(conv_layer_kernel_7_3.weight, a: 0, mode: torch.nn.init.FanInOut.FanOut,
                        torch.nn.init.NonlinearityType.ReLU);

                    resnet_block.add_module(conv_layer_name, conv_layer_kernel_7_3);
                }
            }


            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            input = resnet_block.forward(input);
            return input;
            //return relu.forward(input);
        }

        public Sequential resnet_block  = torch.nn.Sequential();
        private ReLU relu = torch.nn.ReLU(true);
    }

    // public class ResnetUnit : torch.nn.Module<torch.Tensor, torch.Tensor>
    // {
    //     public ResnetUnit(long dRate, long kernel) : base(
    //         nameof(ResnetUnit))
    //     {
    //         if (kernel == 1)
    //         {
    //             resnet_unit = new ConvolutionUnit(dRate, true);
    //         }
    //         else if (kernel == 7)
    //         {
    //             resnet_unit = new ConvolutionUnit(dRate, false);
    //         }
    //
    //         RegisterComponents();
    //     }
    //
    //     public override torch.Tensor forward(torch.Tensor input)
    //     {
    //         return resnet_unit.forward(input);
    //     }
    //     public torch.nn.Module<torch.Tensor, torch.Tensor> resnet_unit { get; set; }
    // }

}