using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Proteomics.RetentionTimePrediction;
using TorchSharp;

namespace Test
{
    public class TestChronologer
    {
        [Test]
        public void Test()
        {
            //var rtModel = new Chonologer();
            torch.load(
                        @"C:\Users\Edwin\Documents\GitHub\mzLib-Fork\mzLib\Proteomics\RetentionTimePrediction\Chronologer_20220601193755.pt");
            //var model = torch.jit.load(
            //    @"C:\Users\Edwin\Documents\GitHub\mzLib-Fork\mzLib\Proteomics\RetentionTimePrediction\Chronologer_20220601193755.pt");

            var zero = 0;
        }
    }
}
