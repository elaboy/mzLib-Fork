using System.Runtime.CompilerServices;
using Proteomics.PSM;
using Proteomics.RetentionTimePrediction.Chronologer;

namespace Analyzer;
public class PlotFactory
{
    private static Dictionary<string, List<string>> options = new Dictionary<string, List<string>>()
    {
        { "--files", new List<string>() },
        {"--labels", new List<string>()},
        {"--tasks", new List<string>()}
    };

    private static Dictionary<string, List<PsmFromTsv>> fileDictionary { get; set; }

    private static string outputDirectory { get; set; }

    public PlotFactory(string[] paths, string[] labels)
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
                x.PEP_QValue <= 0.05)
                .ToList());
        }
    }
    public static void Main(string[] args)
    {
        CommandLineParser(args);
        var plotFactory = new PlotFactory(
            options.First(x => x.Key == "--files").Value.ToArray(), 
            options.First(x => x.Key == "--labels").Value.ToArray());

        Run(options.First(x => x.Key == "--tasks").Value.First());
    }

    private static void Run(string task)
    {
        switch (task)
        {
            case "ChronologerEstimator":

                foreach (var file in fileDictionary)
                {
                    Parallel.ForEach(file.Value, psm =>
                    {
                        psm.ChronolgerHI = GetChronologerHI(psm);
                    });
                }

                //make directory 
                var di = Directory.CreateDirectory(Path.Join(outputDirectory, "ChronologerEstimator"));

                // dump options file as json


                // save to output directory
                var filePath = Path.Join(outputDirectory, "ChronologerEstimator");
                Write(Path.Join(outputDirectory, "ChronologerEstimator"), filePath);
                break;
        }
    }

    private static void IndividualFileCollector()
    {
        // Create a new directory to save the outputs
        // Read all the individual files (the user withh provide only the search task so we need to move one more directory in)
        // Gather all the data
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
                                     psm.FullSequence + "\t" +
                                     psm.RetentionTime + "\t" +
                                     psm.ChronolgerHI + "\t" +
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
                    case "-o":
                        outputDirectory = args[i];
                        break;
                    default:
                        Console.WriteLine("Argument error: this is not an option in the program. Revise your input.");
                        break;
                }
            }
        }
    }

    private static double? GetChronologerHI(PsmFromTsv psm)
    {
        return ChronologerEstimator.PredictRetentionTime(psm.BaseSeq, psm.FullSequence);
    }

    private static void ChronologerHI(List<PsmFromTsv> psms)
    {
        foreach (PsmFromTsv psm in psms)
        {
            psm.ChronolgerHI = ChronologerEstimator.PredictRetentionTime(psm.BaseSeq, psm.FullSequence);
        }
    }

}