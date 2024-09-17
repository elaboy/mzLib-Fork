using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using CsvHelper;
using CsvHelper.Configuration;
using MassSpectrometry;
using MathNet.Numerics.Statistics;
using Omics.SpectrumMatch;
using Proteomics.PSM;
using Proteomics.RetentionTimePrediction.Chronologer;
using Readers.QuantificationResults;

namespace Analyzer;
public class AnalyzerEngine
{
    private static Dictionary<string, List<string>> options = new Dictionary<string, List<string>>()
    {
        { "--files", new List<string>() },
        {"--labels", new List<string>()},
        {"--tasks", new List<string>()},
        {"--RTLib", new List<string>()}
    };

    private static Dictionary<string, List<PsmFromTsv>> fileDictionary { get; set; }

    public static string outputDirectory => @"E:\"; /*{ get; set; }*/
    public Dictionary<string, ExperimentalDesign> ExperimentalDesign { get; set; }
    public static List<IRetentionTimeAlignable> AlignablePsms { get; set; }
    public Dictionary<string, Dictionary<string, double>> RetentionTimeLibrary { get; set; }

    public AnalyzerEngine(string[] paths, string[] labels)
    {
        // makes the file dictionary (psms with label as the key)
        fileDictionary = new Dictionary<string, List<PsmFromTsv>>();
        for (int i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            var label = labels[i];

            var psms = new Readers.PsmFromTsvFile(path);
            psms.LoadResults();

            fileDictionary.Add(label, psms.Results.Where(x => 
                x.AmbiguityLevel == "1" & 
                x.QValue <= 0.01 &
                x.PEP <= 0.05 &
                x.PEP_QValue <= 0.01)
                .ToList());
        }
    }

    public AnalyzerEngine(string[] paths)
    {
        List<IRetentionTimeAlignable>[] psmData = new List<IRetentionTimeAlignable>[paths.Length];
        Parallel.For(0, psmData.Length, i=>
        {
            psmData[i] = new List<IRetentionTimeAlignable>();

            //load the data
            var psms = new Readers.PsmFromTsvFile(paths[i]);
            psms.LoadResults();

            psmData[i].AddRange(
                psms.Results
                    .Where(x => x.AmbiguityLevel == "1" &
                                x.DecoyContamTarget == "T")
                    .Cast<IRetentionTimeAlignable>().ToList());
        });

        var filteredPsmData = psmData.SelectMany(x => x
            .GroupBy(i => i.FileName)).ToList();
        
        var filteredPsms = new List<IRetentionTimeAlignable>();

        foreach (var file in filteredPsmData)
        {
            var FullSequenceGrouped = file
                .GroupBy(x => x.FullSequence).ToList();

            var toReplace = new List<IRetentionTimeAlignable>();
            foreach (var sequence in FullSequenceGrouped)
            {
                toReplace.AddRange(sequence.Select(x => x));
            }

            var toSendToFilteredPsm = toReplace[0];
            toSendToFilteredPsm.RetentionTime = toReplace.Select(x => x.RetentionTime).Median();
            filteredPsms.Add(toSendToFilteredPsm);
        }

        AlignablePsms = new List<IRetentionTimeAlignable>();
        AlignablePsms.AddRange(filteredPsms);
    }
    public static void Main(string[] args)
    {
        CommandLineParser(args);


        var analyzerEngine = new AnalyzerEngine(
            options.First(x => x.Key == "--files").Value.ToArray()); 
            //options.First(x => x.Key == "--labels").Value.ToArray());

        Run(options.First(x => x.Key == "--tasks").Value.First());
    }

    public static void Run(string task)
    {
        switch (task)
        {
            case "ChronologerEstimator":

                foreach (var file in fileDictionary)
                {
                    Parallel.ForEach(file.Value, psm =>
                    {
                        psm.ChronologerHIDouble = GetChronologerHI(psm);
                    });
                }

                //make directory 
                var di = Directory.CreateDirectory(Path.Join(outputDirectory, "ChronologerEstimator"));

                // dump options file as json

                // save to output directory
                var filePath = Path.Join(outputDirectory, "ChronologerEstimator");
                Write(Path.Join(outputDirectory, "ChronologerEstimator"), filePath);
                break;

            // TODO
            case "CreateRTLib":
                RetentionTimeAligner aligner = new RetentionTimeAligner(AlignablePsms);
                aligner.Calibrate();
                RetentionTimeAlignerExtensionMethods.SaveResults(aligner, Path.Join(outputDirectory, "RTLib.json"));
                break;

            case "RetentionTimeAligner":
                // calibrates the files and add a time shift to each peptide
                RetentionTimeAligner aliger = new RetentionTimeAligner(AlignablePsms);
                aliger.Calibrate();
                // returns a tsv with the mean and median time retention shift of the file
                break;
        }
    }

    private static void IndividualFileCollector(string searchTaskDirectoryPath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t"
        };

        // load experimental design
        List<ExperimentalDesign> experimentalDesign = new List<ExperimentalDesign>();
        
        using (var reader = new StreamReader(Path.Join(searchTaskDirectoryPath, "ExperimentalDesign.tsv")))
        using (var tsv = new CsvReader(reader, config))
        {
            experimentalDesign.AddRange(tsv.GetRecords<ExperimentalDesign>().ToList());
        }

        string[] fileNames = experimentalDesign.Select(x => x.FileName.Replace(".mzML", "")).Distinct().ToArray();

        string newDirectoryPath = Path.Join(outputDirectory, "IndividualFileCollector");
        // Create a new directory to save the outputs
        DirectoryInfo di = Directory.CreateDirectory(newDirectoryPath);
        // Read all the individual files (the user withh provide only the search task so we need to move one more directory in)
        string[] files = Directory.GetFiles(searchTaskDirectoryPath+"Individual File Results");
        // Gather all the data
        Dictionary<string, (List<PsmFromTsv> peptides, List<PsmFromTsv> psms, List<QuantifiedPeak> quantifiedPeaks)>
            allFilesData = new();
        
        foreach (var file in fileNames)
        {
            // peptides
            var peptides = new Readers.PsmFromTsvFile(file+"_Peptides.psmtsv");
            peptides.LoadResults();

            // psms
            var psms = new Readers.PsmFromTsvFile(file+"_PSMs.psmtsv");
            psms.LoadResults();

            // quantified peaks
            var quantifiedPeaks = new QuantifiedPeakFile(file+"_QuantifiedPeaks.psmtsv");
            quantifiedPeaks.LoadResults();

            allFilesData.Add(file, (peptides.Results, psms.Results, quantifiedPeaks.Results));
        }
        // save the data in the directory, should be one tsv per file (18 files at the end according to the 11 mann files dataset)

    }

    private static void Write(string outputDirectory, string filePath)
    {
        foreach (var file in fileDictionary)
        {
            using (StreamWriter writer =
                   new StreamWriter(new FileStream(filePath + file.Key + ".tsv", FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("Base Sequence\tFull Sequence\tScan Retention Time\tChronologerHI\tAmbiguity Level\tQValue\tPEP_QValue\tPEP\tQValueNotch");

                foreach (var psm in file.Value)
                {
                    writer.WriteLine(psm.BaseSeq + "\t" +
                                     ((SpectrumMatchFromTsv)psm).FullSequence + "\t" +
                                     psm.RetentionTime + "\t" +
                                     psm.ChronologerHIDouble + "\t" +
                                     psm.AmbiguityLevel + "\t" +
                                     psm.QValue + "\t" +
                                     psm.PEP_QValue + "\t" +
                                     psm.PEP + "\t" +
                                     psm.QValueNotch);
                }
                writer.Close();
            }
        }
    }

    private static void CommandLineParser(string[] args)
    {
        var argument = args[0];

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].StartsWith("-"))
            {
                argument = args[i];
            }
            else
            {
                switch (argument)
                {
                    case "--files":
                        options[argument].Add(args[i]);
                        break;
                    case "--tasks":
                        options[argument].Add(args[i]);
                        break;
                    case "--labels":
                        options[argument].Add(args[i]);
                        break;
                    case "--RTLib":
                        options[argument].Add(args[i]);
                        break;
                    //case "-o":
                    //    outputDirectory = args[i];
                        break;
                    default:
                        Console.WriteLine("Argument error: this is not an option in the program. Revise your input.");
                        break;
                }
            }
        }
    }

    private static double? GetChronologerHI(PsmFromTsv psm) => 
        ChronologerEstimator.PredictRetentionTime(psm.BaseSeq, ((SpectrumMatchFromTsv)psm).FullSequence); 

    private static void ChronologerHI(List<PsmFromTsv> psms)
    {
        foreach (PsmFromTsv psm in psms)
        {
            psm.ChronologerHIDouble = ChronologerEstimator.PredictRetentionTime(psm.BaseSeq, ((SpectrumMatchFromTsv)psm).FullSequence);
        }
    }
}