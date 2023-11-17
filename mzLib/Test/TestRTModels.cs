using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BayesianEstimation;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Easy.Common.Extensions;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.Transforms.Text;
using Nett;
using NUnit.Framework;
using OxyPlot;
using pepXML.Generated;
using Proteomics.RetentionTimePrediction;
using TorchSharp;
using TorchSharp.Modules;
using TorchSharp.Utils;
using static TorchSharp.torch.utils;

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

        //[Test]
        //public void TestPrediction()
        //{
        //    // var model = torch.jit.load(@"F:\Research\Data\Chronologer\exported_model.dat");

        //    var value = new Int64[]
        //    {
        //        38, 11, 6, 10, 4, 1, 10, 18, 13, 10, 1, 11, 8, 18, 1, 8, 5, 10, 10, 10, 18, 3, 10, 11,
        //        7, 15, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //        0, 0, 0, 0
        //    };
        //    var tensor = torch.tensor(value, torch.ScalarType.Int64);

        //    var m = torch.jit.ScriptModule.Load(@"F:\Research\Data\Chronologer\scripted_model.pt");

        //    m.load(@"F:\Research\Data\Chronologer\model_weights.dat");


        //    // torch.jit.save(model, "exported_torchsharp_chronologer.dat");
        //    var prediction = torch.nn.Sequential();

        //    var predictor = torch.jit.load(
        //        @"F:\Research\Data\Chronologer\scripted_model.pt");

        //    predictor.load_state_dict(new Dictionary<string, torch.Tensor>()
        //    {
        //        {
        //            "", torch.load(@"F:\Research\Data\Chronologer\model_weights.dat")
        //        }
        //    });
        //    var modules = predictor.modules();

        //    for (int i = 0; i < modules.Count(); i++)
        //    {
        //        prediction.add_module(i.ToString(), modules.ElementAt(i));
        //    }
        //    // var result = engine.invoke("forward",tensor);

        //    prediction.train(false); 
        //    var prediction2 = prediction.forward(tensor);

        //    Console.WriteLine(prediction2.ToString(TensorStringStyle.Numpy));
        //}

        [Test]
        public void TestResNet50()
        {
            var net = ResNet.ResNet50(10);

            net.load(@"C:\Users\Edwin\Documents\GitHub\RT-DP\model_weights_resnet50.dat");

            var dataset = torchvision.datasets
                .CIFAR10(@"C:\Users\Edwin\Documents\GitHub\RT-DP\CIFAR10.gz", false,
                    true);

            var dataLoader = new DataLoader(dataset, 3, shuffle: false);

            net.eval();

            var loss = torch.nn.NLLLoss();

            foreach (var data in dataLoader)
            {

                var prediction = net.forward(data["data"]);
                var target = data["label"];
                var lsm = torch.nn.functional.log_softmax(prediction, 1);
                var output = loss.forward(lsm, target);

                Console.WriteLine("Truth: " + target.ToString(TensorStringStyle.Numpy));
                Console.WriteLine("Predicted: " + prediction.argmax(1).ToString(TensorStringStyle.Numpy));
                break;
            }
        }

        [Test]
        public void TestChronologer()
        {
            var Chronologer = new nnModules.Chronologer(52, 55, 64, 3, 7, (long)0.1, "relu");
            foreach (var param in Chronologer.named_parameters())
            {
                Debug.WriteLine(param);
            }
            Chronologer.load(@"C:\Users\Edwin\Documents\GitHub\RT-DP\model_weights_Chronologer.dat", true);

            // var model = torch.jit.load(
            //     @"C:\Users\Edwin\Documents\GitHub\RT-DP\Chronologer_jit.dat");

            // model.save(
            //     @"C:\Users\Edwin\Documents\GitHub\mzLib-Fork\mzLib\Proteomics\RetentionTimePrediction\chronologer.model.bin");
            //
            // model.load(@"C:\Users\Edwin\Documents\GitHub\RT-DP\model_weights_Chronologer.dat", true);
            //
            // model.train(false);

            // model.save(@"C:\Users\Edwin\Documents\GitHub\RT-DP\Chronologer_jit_trained_saved.dat");
            var example = torch.zeros(1, 52, torch.ScalarType.Int64);

            example[0] = new int[]
            {
                38, 11,  6, 10,  4,  1, 10, 18, 13, 10,  1, 11,  8, 18,  1,  8,  5, 10, 10, 10, 18,  3, 10, 11,
                7, 15, 44,  0,  0,  0,  0, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0, 0,  0,  0,  0,  0,  0,  0,
                0,  0,  0,  0
            };

            Debug.WriteLine(example.ToString(TensorStringStyle.Numpy));
            Debug.WriteLine(example.shape);
            Chronologer.eval();
            var prediction = Chronologer.forward(example);

            Debug.WriteLine(prediction.ToString(TensorStringStyle.Numpy));

        }
        [Test]
        public void TestChronologer2()
        {
            //var Chronologer = new nnModules.Chronologer(52, 55, 64, 3, 7, (long)0.1, "relu");

            //Chronologer.modules().ForEach(x => x.modules().ForEach(i => Debug.WriteLine(i)));
            //var state_dict = Chronologer.state_dict();

            //Chronologer.load(@"C:\Users\Edwin\Documents\GitHub\RT-DP\model_weights_Chronologer_new_module_names.dat",
            //true);

            ////Chronologer.save(@"F:\Research\Data\Chronologer\loadedWeightsCSharpModel.dat");


            ////var new_model = new nnModules.Chronologer(52, 55, 64, 3, 7, (long)0.1, "relu");
            ////new_model = (nnModules.Chronologer)new_model.load(@"F:\Research\Data\Chronologer\loadedWeightsCSharpModel.dat");
            ////var new_state_dict = new_model.state_dict();
            //foreach (var thing in Chronologer.state_dict())
            //{
            //    Debug.WriteLine(thing.Key + " " + thing.Value.ToString(TensorStringStyle.Julia));
            //}
            ////Chronologer.eval();

            var stream = new StreamReader(@"F:\Research\Data\Hela\predictedExample - Copy.tsv");
            var data = new CsvHelper.CsvReader(stream, Mapper.MapperConfig);

            var mappedData = new List<Mapper>();

            var d = data.GetRecords<Mapper>().ToList();

            Mapper.Write(d, @"F:\Research\Data\Hela\predictedExample - Copy2.tsv");

            var readWrittenData =
                new CsvHelper.CsvReader(new StreamReader(@"F:\Research\Data\Hela\predictedExample - Copy2.tsv"),
                    Mapper.MapperConfig);
            var tensors = readWrittenData.GetRecords<Mapper>().ToList();
            var example = torch.zeros(1, 52, torch.ScalarType.Int64);

            foreach (var tensor in tensors)
            {
                // Debug.WriteLine(tensor.tensor); 
            }

            

            var tensor_example = torch.zeros(52*tensors.Count, torch.ScalarType.Int64);
            var concatenatedTensors = torch.cat(tensors.Select(x => x.tensor).ToArray(), 0);


            //Chronologer.eval();
            Debug.WriteLine(tensor_example.ToString(TensorStringStyle.Julia));
            Debug.WriteLine(concatenatedTensors[0].ToString(TensorStringStyle.Julia));
            //var sliced_tensor = concatenatedTensors.take_along_dim(torch.tensor(Enumerable.Range(0, 52).ToList(), torch.ScalarType.Int64));
            //var prediction = Chronologer.call(concatenatedTensors[0]);
            var model = new nnModules.RawChronologer();
            model.load(@"C:\Users\Edwin\Documents\GitHub\RT-DP\model_weights_Chronologer_new_module_names.dat", true);
          
            model.eval();
            model.train(false);
            // using (torch.no_grad())
            // {

                var sadasd = model.state_dict();
                for (int i = 0; i < 10; i++)
                {
                    var tt = concatenatedTensors[i].reshape(1, 52);
                    // var batch = torch.cat(Enumerable.Range(0, 64).Select(x => tt).ToArray());
                    Debug.WriteLine(tt.ToString(TensorStringStyle.Julia));
                    // tt = torch.tensor(new List<long>(52){38, 11,  6, 10,  4,  1, 10, 18, 13, 10,  1, 11,  8, 18,  1,  8,  5, 10, 10, 10, 18,  3, 10, 11,
                    //     7, 15, 44,  0,  0,  0,  0,  0, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
                    //     0,  0,  0,  0 });
                    var prediction2 = model.call(tt);
                    Debug.WriteLine(prediction2.ToString(TensorStringStyle.Julia));
                }

                //foreach (var thing in sadasd)
                //{
                //    Debug.WriteLine(thing.Key + " " + thing.Value.ToString(TensorStringStyle.Julia));
                //}
            // }

            
            //foreach (var i in sadasd)
            //{
            //    Debug.WriteLine(i.Key + " " + i.Value.ToString(TensorStringStyle.Julia));
            //}
            // var tt = concatenatedTensors[0].reshape(1, 52);
            // var prediction2 = model.call(tt);
            // Debug.WriteLine(prediction2.ToString(TensorStringStyle.Julia));

            //foreach (var thing in Chronologer.state_dict())
            //{
            //    Debug.WriteLine(thing.Key + " "+ thing.Value.ToString(TensorStringStyle.Julia));
            //}
            //var arch = Chronologer.named_modules();

            //var f = new[]
            //{
            //    38, 9, 10, 4, 11, 3, 10, 9, 44, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0

            //};

            //var first = torch.zeros(52, torch.ScalarType.Int64);
            //for (int i = 0; i < f.Length; i++)
            //{
            //    first[i] = f[i];
            //}
            //var first_rt = Chronologer.call(first);
            //Debug.WriteLine(first.ToString(TensorStringStyle.Julia));
            //Debug.WriteLine(first_rt.ToString(TensorStringStyle.Julia));

            //var prediction_values = prediction.data<double>().ToArray();

            //using (var writter = new StreamWriter(@"F:\Research\Data\Hela\predictedExample_fromCSharp.tsv"))
            //{
            //    for (int i = 0; i < prediction_values.Length; i++)
            //    {
            //        writter.WriteLine(prediction_values[i]);
            //    }
            //}
        }

        [Test]
        public void RawChronologer()
        {
            var model = new nnModules.RawChronologer().load(@"C:\Users\Edwin\Documents\GitHub\RT-DP\model_weights_Chronologer_new_module_names.dat");
            
            var stateDict = model.state_dict();
        }

        internal class Mapper
        {
            internal static CsvConfiguration MapperConfig => new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t",
                HeaderValidated = null,
            };

            [Name("Scan Retention Time")]
            public string ScanRetentionTime { get; set; }
            public string PeptideModSeq { get; set; }
            public string Length { get; set; }
            public string CodedPeptideSeq { get; set; }
            public string PeptideLength { get; set; }
            public string Pred_HI { get; set; }

            [TypeConverter(typeof(TensorConverter))]
            public torch.Tensor tensor { get; set; }

            internal static void Write(IEnumerable<Mapper> toWrite, string filepath)
            {
                using var csv = new CsvWriter(new StreamWriter(filepath), MapperConfig);
                csv.WriteHeader<Mapper>();
                csv.NextRecord();
                csv.WriteRecords(toWrite);
                csv.Dispose();
            }

            internal class TensorConverter : DefaultTypeConverter
            {
                public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
                {
                    var tensor = torch.zeros(1, 52, torch.ScalarType.Int64);
                    text = text.Replace("\n", "").Replace(")", "");
                    var values = text.Split(',').Select(x => int.Parse(x)).ToArray();
                    tensor[0] = values;
                    return tensor;
                }

                public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
                {
                    var tensor = value as torch.Tensor ?? throw new ArgumentException("Value is not a tensor");
                    var temp2 = tensor.ToString(TensorStringStyle.Julia);
                    temp2 = temp2.TrimStart().TrimEnd().Replace("\n", "").Replace(")", "").Replace("\r", "").Replace("[[", "").Replace("]]", "");
                    var temp3 = tensor.data<long>().ToArray();
                    return string.Join(',', temp3);
                }
            }
            
        }
        
    }

    public class resnet_blocks : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private long _inChannels { get; set; }
        private long _outChannels { get; set; }
        private long _kernel { get; set; }
        private long _dRate { get; set; }
        private string _actFX { get; set; }
        private int _numberOfBlocks { get; set; }

        public resnet_blocks(long inChannels, long outChannels, long kernel, long dRate, string actFX,
            int numberOfBlocks = 3) : base(nameof(resnet_blocks))
        {

            _inChannels = inChannels;
            _outChannels = outChannels;
            _kernel = kernel;
            _dRate = dRate;
            _actFX = actFX;
            _numberOfBlocks = numberOfBlocks;
            // InitiateModel();
            RegisterComponents();

        }

        private void InitiateModel()
        {
            shortcut = ConvolutionUnit();
            process_blocks = torch.nn.Sequential();
            process_blocks.add_module("process_blocks0", ResnetUnit(kernel: 1));
            process_blocks.add_module("process_blocks1", ResnetUnit());
        }

        public static Sequential ResnetUnit(long kernel = 7)
        {
            var convUnit = ConvolutionUnit();
            var activation = torch.nn.ReLU();
            var resnetUnit = torch.nn.Sequential();
            resnetUnit.add_module("convUnit", convUnit);
            resnetUnit.add_module("activation", activation);
            return resnetUnit;
        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            var residual = input;
            if (_inChannels != _outChannels) residual = shortcut.forward(input);

            var x = process_blocks.forward(input);
            x = process_blocks.forward(x);
            x = termBlock.forward(x);
            x = x + residual;
            // var activation = activationFunctions[_actFX];
            x = torch.nn.functional.relu(x);
            return x;
        }

        private static Sequential ConvolutionUnit(long kernel = 7)
        {
            var convolutionalLayer = torch.nn.Conv1d(64, 64,
                kernel, dilation: 1, padding: Padding.Same);

            torch.nn.init.kaiming_normal_(convolutionalLayer.weight, mode: torch.nn.init.FanInOut.FanOut,
                nonlinearity: torch.nn.init.NonlinearityType.ReLU);
            var nomalizationLayer = torch.nn.BatchNorm1d(64);
            torch.nn.init.constant_(nomalizationLayer.weight, 1);
            torch.nn.init.constant_(nomalizationLayer.bias, 0);
            var unit = torch.nn.Sequential(convolutionalLayer, nomalizationLayer);

            return unit;
        }

        private static Sequential ConvolutionUnit2(long kernel = 7)
        {
            var convolutionalLayer = torch.nn.Conv1d(64, 64,
                kernel, dilation: 2, padding: Padding.Same);

            torch.nn.init.kaiming_normal_(convolutionalLayer.weight, mode: torch.nn.init.FanInOut.FanOut,
                nonlinearity: torch.nn.init.NonlinearityType.ReLU);
            var nomalizationLayer = torch.nn.BatchNorm1d(64);
            torch.nn.init.constant_(nomalizationLayer.weight, 1);
            torch.nn.init.constant_(nomalizationLayer.bias, 0);
            var unit = torch.nn.Sequential(convolutionalLayer, nomalizationLayer);

            return unit;
        }

        private static Sequential ConvolutionUnit3(long kernel = 7)
        {
            var convolutionalLayer = torch.nn.Conv1d(64, 64,
                kernel, dilation: 3, padding: Padding.Same);

            torch.nn.init.kaiming_normal_(convolutionalLayer.weight, mode: torch.nn.init.FanInOut.FanOut,
                nonlinearity: torch.nn.init.NonlinearityType.ReLU);
            var nomalizationLayer = torch.nn.BatchNorm1d(64);
            torch.nn.init.constant_(nomalizationLayer.weight, 1);
            torch.nn.init.constant_(nomalizationLayer.bias, 0);
            var unit = torch.nn.Sequential(convolutionalLayer, nomalizationLayer);

            return unit;
        }

        public Sequential process_blocks = torch.nn.Sequential(
            ("0", ResnetUnit(1)),
            ("1", ResnetUnit()));

        public Identity termBlock = torch.nn.Identity();

        public Sequential shortcut = torch.nn.Sequential(("0", torch.nn.Conv1d(64, 64, 1, Padding.Same)),
            ("1", torch.nn.BatchNorm1d(64)));

        public Dictionary<string, torch.nn.Module> activationFunctions =
            new Dictionary<string, torch.nn.Module>()
            {
                { "relu", torch.nn.ReLU() },
                { "leaky_relu", torch.nn.LeakyReLU(negative_slope: 0.01, inplace: true) },
                { "selu", torch.nn.SELU(inplace: true) },
                { "none", torch.nn.Identity() }
            };
    }

    public class process_blocks : torch.nn.Module<torch.Tensor, torch.Tensor>
    {
        private Sequential process_block = torch.nn.Sequential();

        public process_blocks(string name) : base(name)
        {

        }

        public override torch.Tensor forward(torch.Tensor input)
        {
            return input;
        }


    }
}

