using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using pepXML.Generated;
using TorchSharp;
using TorchSharp.Modules;

namespace Proteomics.RetentionTimePrediction
{
    public class Chonologer : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public static long vectorLength;
        public static long nStates { get; set; }
        public static long embedDim;
        public static long nBlocks { get; set; }
        public static long kernel { get; set; }
        public static long dropRate { get; set; }
        public static bool actFx { get; set; }

        public Chonologer(long vectorLength, long nStates, 
            long embedDim, long nBlocks, long kernel,
            long dropRate, long actFx) : base(nameof(Chonologer))
        {
            RegisterComponents();
            torch.nn.init.kaiming_normal_(seq_embed.weight, nonlinearity:torch.nn.init.NonlinearityType.Linear);
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var x = seq_embed.forward(input).transpose(1, -1);
        }

        private Embedding seq_embed =
            torch.nn.Embedding(nStates, embedDim, padding_idx:0);
        private Sequential resnetBlocks = torch.nn.Sequential(("resNetBlocks", ResNetBlock()))
        
    }

    public class ResNetBlock : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        public long InChannels { get; set; }
        public long OutChannels { get; set; }
        public ResNetBlock(long in_channels, long out_channels, long kernel, long d_rate, bool act_fx) : base(nameof(ResNetBlock))
        {
            RegisterComponents();
            InChannels = in_channels;
            OutChannels = out_channels;

        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            throw new NotImplementedException();
        }
    }

    public static class ExtensionMethodsForChonologer
    {
        

        public static Sequential ResNetUnit(long in_channels, long out_channels, long kernel, long d_rate, string act_fx)
        {
            
            var unit =  Sequential.Create<>(ConvolutionUnit(in_channels, out_channels, kernel, d_rate),
                ActivationFunction(act_fx));
            //(ConvolutionUnit(in_channels, out_channels, kernel, d_rate),
            //    ActivationFunction(act_fx));
        }

        public static torch.nn.Module ActivationFunction(string activation)
        {
            torch.nn.Module module = null;

            var dict =  torch.nn.ModuleDict(new (string, torch.nn.Module)[]
            {
                ("relu", torch.nn.ReLU(inplace: true)),
                ("leaky_relu", torch.nn.LeakyReLU(inplace:true)),
                ("selu", torch.nn.SELU(inplace:true)),
                ("none", torch.nn.Identity())
            });
            dict.TryGetValue(activation, out module);

            return module;
        }

        public static torch.nn.Module ConvolutionUnit(long in_channels, long out_channels, long kernel, long d_rate)
        {
            var conv_layer = torch.nn.Conv1d(in_channels, out_channels, kernel, dilation: d_rate);

            torch.nn.init.kaiming_normal_(conv_layer.weight, mode: torch.nn.init.FanInOut.FanOut,
                nonlinearity: torch.nn.init.NonlinearityType.LeakyReLU);

            var norm_layer = torch.nn.BatchNorm1d(out_channels);

            torch.nn.init.constant_(norm_layer.weight, 1);
            torch.nn.init.constant_(norm_layer.bias, 0);

            return torch.nn.Sequential(conv_layer, norm_layer);
        }


    }
}
