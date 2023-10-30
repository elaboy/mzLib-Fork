using System;
using System.Collections.Generic;
using NUnit.Framework;
using pepXML.Generated;
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

        //[Test]
        //public void TestConstructor()
        //{
        //    var model = new RTModels(Model.Chronologer);
        //    model.RTPredictor.eval();


        //    Assert.AreEqual(typeof(TorchSharp.Modules.Sequential), model.RTPredictor.GetType());
        //}

        [Test]
        public void TestPrediction()
        {
            var model = new RTModels(Model.Chronologer);
            
            var value = new int[]
            {
                38, 11, 6, 10, 4, 1, 10, 18, 13, 10, 1, 11, 8, 18, 1, 8, 5, 10, 10, 10, 18, 3, 10, 11,
                7, 15, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0
            };
            var tensor = torch.tensor(value);
            var prediction = model.predict(tensor);
            Console.WriteLine(prediction.ToString(TensorStringStyle.Numpy));
        }
    }
}
