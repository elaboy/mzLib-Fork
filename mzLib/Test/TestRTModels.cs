using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Easy.Common.Extensions;
using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
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
            // var model = torch.jit.load(@"F:\Research\Data\Chronologer\exported_model.dat");
            
            var value = new Int64[]
            {
                38, 11, 6, 10, 4, 1, 10, 18, 13, 10, 1, 11, 8, 18, 1, 8, 5, 10, 10, 10, 18, 3, 10, 11,
                7, 15, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0
            };
            var tensor = torch.tensor(value, torch.ScalarType.Int64);

            var m = torch.jit.ScriptModule.Load(@"F:\Research\Data\Chronologer\scripted_model.pt");

            m.load(@"F:\Research\Data\Chronologer\model_weights.dat");


            // torch.jit.save(model, "exported_torchsharp_chronologer.dat");
            var prediction = torch.nn.Sequential();

            var predictor = torch.jit.load(
                @"F:\Research\Data\Chronologer\scripted_model.pt");

            predictor.load_state_dict(new Dictionary<string, torch.Tensor>()
            {
                {
                    "", torch.load(@"F:\Research\Data\Chronologer\model_weights.dat")
                }
            });
            var modules = predictor.modules();
            
            for (int i = 0; i < modules.Count(); i++)
            {
                prediction.add_module(i.ToString(), modules.ElementAt(i));
            }
            // var result = engine.invoke("forward",tensor);
            
            prediction.train(false); 
            var prediction2 = prediction.forward(tensor);

            Console.WriteLine(prediction2.ToString(TensorStringStyle.Numpy));
        }

        [Test]
        public void testmlnet()
        {
            var model = new Chonologer(52, 55, 64, 3, 7, 
                (long)0.1, "relu");
            
            model.load(@"F:\Research\Data\Chronologer\exported_model.dat");
        }
    }
}
