using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analyzer;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;

namespace Test;
public class AnalizerTests
{
    [Test]
    public void TestRTLibCMD()
    {
        string commandString = @"C:\Users\elabo\Documents\GitHub\mzLib-Fork\mzLib\Analyzer\dotnet run --files E:\DatasetsForRT\Mann_11cell_lines\A549\2024-07-10-15-53-11_ClassigBigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\GAMG\2024-07-15-13-44-31_ClassicBigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\HEK293\2024-07-16-14-03-19_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\Hela\2024-07-23-11-26-20_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\HepG2\2024-07-23-11-29-16_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\Jurkat\2024-07-23-11-33-12_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\K562\2024-07-23-11-39-32_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\LanCap\2024-07-23-11-43-38_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\MCF7\2024-07-23-11-46-09_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\RKO\2024-07-23-11-49-07_BigSearch\Task4-SearchTask\AllPSMs.psmtsv E:\DatasetsForRT\Mann_11cell_lines\U2OS\2024-07-23-11-51-33_BigSearch\Task4-SearchTask\AllPSMs.psmtsv --tasks CreateRTLib -o E:\";

        Process cmd = new Process();
        cmd.StartInfo.FileName = commandString;
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();
    }

    [Test]
    public void TestRTLibCMD2()
    {
        PlotFactory factory = new PlotFactory(
            new string[11]
            { 
                @"E:\Datasets\\Mann_11cell_lines\\A549\\2024-07-10-15-53-11_ClassigBigSearch\\Task4-SearchTask\\AllPSMs.psmtsv", 
                @"E:\Datasets\\Mann_11cell_lines\\GAMG\\2024-07-15-13-44-31_ClassicBigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\HEK293\\2024-07-16-14-03-19_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\\\Mann_11cell_lines\\\\Hela\\\\2024-07-23-11-26-20_BigSearch\\\\Task4-SearchTask\\\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\HepG2\\2024-07-23-11-29-16_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\Jurkat\\2024-07-23-11-33-12_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\K562\\2024-07-23-11-39-32_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv", 
                @"E:\Datasets\\Mann_11cell_lines\\LanCap\\2024-07-23-11-43-38_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\MCF7\\2024-07-23-11-46-09_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\RKO\\2024-07-23-11-49-07_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv",
                @"E:\Datasets\\Mann_11cell_lines\\U2OS\\2024-07-23-11-51-33_BigSearch\\Task4-SearchTask\\AllPSMs.psmtsv"

            });
        
        PlotFactory.Run("CreateRTLib");
    }
}
