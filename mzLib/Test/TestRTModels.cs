using NUnit.Framework;
using Proteomics.RetentionTimePrediction;
using TorchSharp;
using TorchSharp.Modules;

namespace Test
{
    public class TestRTModels
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

        [Test]
        public void TestConstructor()
        {
            var model = new RTModels(Model.Chronologer);
            model.RTPredictor.eval();


            Assert.AreEqual(typeof(TorchSharp.Modules.Sequential), model.RTPredictor.GetType());
        }
    }
}
