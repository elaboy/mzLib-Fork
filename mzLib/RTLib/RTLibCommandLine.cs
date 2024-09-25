namespace RTLib;
public class RtLibCommandLine
{
    public static void Main(string[] args)
    {
        (List<string> filePaths, string outputPath) = CommandLineParser(args);

        RtLib rtLib = new RtLib(filePaths, outputPath, false);
    }

    private static (List<string> filePaths, string outputPath) CommandLineParser(string[] args)
    {
        List<string> filePaths = new List<string>();
        string outputPath = "";

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
                        filePaths.Add(args[i]);
                        break;
                    case "--output":
                        outputPath = args[i];
                        break;
                    default:
                        Console.WriteLine("Argument error: this is not an option in the program. Revise your input.");
                        break;
                }
            }
        }

        return (filePaths, outputPath);
    }

}