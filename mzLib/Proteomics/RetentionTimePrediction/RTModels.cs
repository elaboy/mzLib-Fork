using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using TorchSharp.Modules;

namespace Proteomics.RetentionTimePrediction
{
    public enum Model
    {
        Chronologer //https://github.com/searlelab/chronologer/tree/main
    }
    public class RTModels
    {
        private torch.nn.Module<torch.Tensor, torch.Tensor> RTPredictor { get; set; }

        public RTModels(Model model)
        {
            if (model == Model.Chronologer)
            {
                var modules = torch.jit.load(
                        @"C:\Users\Edwin\Documents\GitHub\mzLib-Fork\mzLib\Proteomics\RetentionTimePrediction\scripted_chronology_model.pt")
                    .modules();

                RTPredictor = torch.nn.Sequential();

                var counter = 0;
                foreach (var module in modules)
                {
                    counter = counter + 1;
                    RTPredictor.add_module(counter.ToString(),module);
                }
            }

        }

        public torch.Tensor predict(torch.Tensor input)
        {
            RTPredictor.eval();
            return RTPredictor.forward(input);
        }
    }
}
