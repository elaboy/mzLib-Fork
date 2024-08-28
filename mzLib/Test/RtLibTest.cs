using System;
using System.Collections.Generic;
using System.Configuration.Internal;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
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
        }, @"E:\rtLib2.json", false);

        int zero = 0;
    }

    [Test]
    public void TestLoadingRtLib()
    {
        RtLib loadedRtLib = new RtLib(@"E:\rtLib2.json");

        // spit all full sequences into a tsv with the last rt in each list
        using(var writer = new StreamWriter(@"E:\rtLib2.csv"))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            List<string> header = new List<string>();
            header.Add("FullSequence");
            header.Add("CalibratedRetentionTime");
            foreach (var kvp in loadedRtLib.Results)
            {
                List<string> record = new List<string>();
                record.Add(kvp.Key);
                record.Add(kvp.Value.Last().ToString());

                csv.WriteField(record);
                csv.NextRecord();
            }
        }
    }

    [Test]
    public void TestAlignerWithChronologer()
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
        }, @"E:\rtLib_chronologer.json", true);

        int zero = 0;
    }
}

