using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Easy.Common.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using pepXML.Generated;
using TorchSharp;
using TorchSharp.Modules;

namespace Proteomics.RetentionTimePrediction
{
    public class Chonologer : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public static long VectorLength { get; set; }
        public static long NumberOfStates { get; set; }
        public static long EmbeddedDimensions;
        public static long NumberOfBlocks{ get; set; }
        public static long Kernel { get; set; }
        public static long DropRate { get; set; }
        public static string ActivationFunction { get; set; }
        //public static long d { get; set; }

        public Chonologer() : base(nameof(Chonologer))
        {
            RegisterComponents();
        }

        public Chonologer(long vectorLength, long nStates, 
            long embedDim, long nBlocks, long kernel,
            long dropRate, string actFx) : base(nameof(Chonologer))
        {
            RegisterComponents();

            VectorLength = vectorLength;
            NumberOfStates = nStates;
            EmbeddedDimensions = embedDim;
            NumberOfBlocks = nBlocks;
            Kernel = kernel;
            DropRate = dropRate;
            ActivationFunction = actFx;
            SetNetworks();
            //for (int i = 0; i < NumberOfBlocks; i++)
            //{
            //    d = d + 1;
            //}
            torch.nn.init.kaiming_normal_(seq_embed.weight, nonlinearity:torch.nn.init.NonlinearityType.Linear);
            torch.nn.init.xavier_normal_(output.weight);
            torch.nn.init.constant_(output.bias, 0);

        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var x = seq_embed.forward(input).transpose(1, -1);
            x = resnetBlocks.forward(x);
            x = dropOut.forward(x);
            x = flatten.forward(x);
            return output.forward(x);
        }


        private void SetNetworks()
        {

            List<ResNetBlock> resNetBlocks = new();
            for (int d = 0; d < NumberOfBlocks; d++)
            {
                resNetBlocks.Add(new ResNetBlock(EmbeddedDimensions, EmbeddedDimensions,
                    Kernel, d+1, ActivationFunction));
            }
            seq_embed = torch.nn.Embedding(NumberOfStates, EmbeddedDimensions, padding_idx: 0);
            resnetBlocks = torch.nn.Sequential(resNetBlocks);
            dropOut = torch.nn.Dropout(DropRate);
            flatten = torch.nn.Flatten();
            output = torch.nn.Linear(VectorLength * EmbeddedDimensions,
                1);
        }
        private Embedding seq_embed { get; set; }

        private Sequential resnetBlocks { get; set; }

        private Dropout dropOut {get; set; }
        private Flatten flatten { get; set; }

        private Linear output { get; set; }


    }

    public class ResNetBlock : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public static long InChannels { get; set; }
        public static long OutChannels { get; set; }
        public static long Kernel { get; set; }
        public static long DropRate { get; set; }
        public static string ActivationFunction { get; set; }
        public ResNetBlock(long in_channels, long out_channels, long kernel, long d_rate, string act_fx) : base(nameof(ResNetBlock))
        {
            RegisterComponents();

            InChannels = in_channels;
            OutChannels = out_channels;
            Kernel = kernel;
            DropRate = d_rate;
            ActivationFunction = act_fx;
            SetNetworks();

        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var residual = input;

            if (InChannels != OutChannels)
                residual = shortcut.forward(input);

            input = processBlock.forward(input);
            input = termBlock.forward(input);
            input = input + residual;
            input = activate.forward(input);

            return input;
        }

        private void SetNetworks()
        {
            processBlock = torch.nn.Sequential(ExtensionMethodsForChonologer.ResNetUnit(InChannels,
                    OutChannels, 1, DropRate, ActivationFunction),
                ExtensionMethodsForChonologer.ResNetUnit(OutChannels, OutChannels, Kernel, DropRate, ActivationFunction));
            activate = ExtensionMethodsForChonologer.ActivationFunction(ActivationFunction);
            shortcut = ExtensionMethodsForChonologer.ConvolutionUnit(InChannels, OutChannels, 1, DropRate);
            termBlock = torch.nn.Identity();
        }

        private Sequential processBlock { get; set; }
        private torch.nn.Module<torch.Tensor, torch.Tensor> activate { get; set; }

        private torch.nn.Module<torch.Tensor, torch.Tensor> shortcut { get; set; }

        private Identity termBlock { get; set; }
    }

    public static class ExtensionMethodsForChonologer
    {


        public static Chonologer InitializeChonologerModel(string modelFile, bool gpu)
        {
            var device = gpu == true ? torch.device(DeviceType.CUDA) : torch.device(DeviceType.CPU);

            var model = new Chonologer(52, 44, 64, 3, 7, (long)0.01, "relu");

            //torch.jit.load(modelFile);
            //var dict = new Dictionary<string, torch.Tensor>();
            //dict.Add("models", torch.jit.load(modelFile).state_dict());
            //torch.load(modelFile);

            //model.register_parameter("models", new Parameter(trace.get_parameter("models")));
            model.load_state_dict(torch.jit.load(modelFile).state_dict(), true, null);
            //model.load_state_dict(new Dictionary<string, torch.Tensor>()
            //{
            //    new KeyValuePair<string, torch.Tensor>("models", torch.jit.load(modelFile).state_dict())
            //});

            model.eval();

            return model;
        }

        public static torch.nn.Module<torch.Tensor, torch.Tensor> ActivationFunction(string activation)
        {
            torch.nn.Module module = null;

            var dict =  torch.nn.ModuleDict(new (string, torch.nn.Module)[]
            {
                ("relu", torch.nn.ReLU(inplace: true)),
                ("leaky_relu", torch.nn.LeakyReLU(0.1, inplace:true)),
                ("selu", torch.nn.SELU(inplace:true)),
                ("none", torch.nn.Identity())
            });
            dict.TryGetValue(activation, out module);

            return (torch.nn.Module<torch.Tensor, torch.Tensor>)module;
        }

        public static torch.nn.Module<torch.Tensor, torch.Tensor> ConvolutionUnit(long in_channels, long out_channels, long kernel, long d_rate)
        {
            var conv_layer = torch.nn.Conv1d(in_channels, out_channels, kernel, dilation: d_rate, padding: Padding.Same);

            torch.nn.init.kaiming_normal_(conv_layer.weight, mode: torch.nn.init.FanInOut.FanOut,
                nonlinearity: torch.nn.init.NonlinearityType.ReLU);

            var norm_layer = torch.nn.BatchNorm1d(out_channels);

            torch.nn.init.constant_(norm_layer.weight, 1);
            torch.nn.init.constant_(norm_layer.bias, 0);

            return torch.nn.Sequential(conv_layer, norm_layer);
        }
        public static Sequential ResNetUnit(long in_channels, long out_channels, long kernel, long d_rate, string act_fx)
        {
            List<torch.nn.Module<torch.Tensor, torch.Tensor>> modules = new()
            {
                ConvolutionUnit(in_channels, out_channels, kernel, d_rate),
                ActivationFunction(act_fx)
            };

            var unit = torch.nn.Sequential(modules);

            return unit;
        }

    }
}
