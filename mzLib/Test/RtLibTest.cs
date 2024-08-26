using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RTLib;

namespace Test;
public class RtLibTest
{
    [Test]
    public void TestRtLib11MannFiles()
    {
        RtLib rtLib = new RtLib(new List<string>()
        {
            @"E:\Datasets\Mann_11cell_lines\A549\2024-07-10-15-53-11_ClassigBigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\GAMG\2024-07-15-13-44-31_ClassicBigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\HEK293\2024-07-16-14-03-19_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\Hela\2024-07-23-11-26-20_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\HepG2\2024-07-23-11-29-16_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\Jurkat\2024-07-23-11-33-12_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\K562\2024-07-23-11-39-32_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\LanCap\2024-07-23-11-43-38_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\MCF7\2024-07-23-11-46-09_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\RKO\2024-07-23-11-49-07_BigSearch\Task4-SearchTask\AllPSMs.psmtsv",
            @"E:\Datasets\Mann_11cell_lines\U2OS\2024-07-23-11-51-33_BigSearch\Task4-SearchTask\AllPSMs.psmtsv"
        }, @"E:\");

        int zero = 0;
    }
}

