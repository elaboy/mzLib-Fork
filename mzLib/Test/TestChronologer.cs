using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Proteomics.RetentionTimePrediction;
using TorchSharp;
using TorchSharp.Modules;

namespace Test
{
    public class TestChronologer
    {
        [Test]
        public void Test()
        {
            //var model = ExtensionMethodsForChonologer.InitializeChonologerModel(
            //            @"C:\Users\Edwin\Documents\GitHub\mzLib-Fork\mzLib\Proteomics\RetentionTimePrediction\scripted_chronology_model.pt",
            //            false);
            var model = torch.jit.load(
                @"C:\Users\Edwin\Documents\GitHub\mzLib-Fork\mzLib\Proteomics\RetentionTimePrediction\scripted_chronology_model.pt");

            var zero = 0;
        }
    }
}
