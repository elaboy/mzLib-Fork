using MassSpectrometry;
using NUnit.Framework;
using Readers;
using System.Collections.Generic;
using System.Linq;

namespace Test;

[TestFixture]
public class TestRetentionTimeCalibration
{
    private MsDataFile[] _dataFiles;
    private List<string[]> _fullSequences;
    private List<double[]> _retentionTimes;

    [SetUp]
    public void SetUp()
    {
        int numberOfDataFilesToMake = 3;
        int numberOfDataScans = 10;


        _fullSequences = new List<string[]>()
        {
            // File 1
            new string[]
            {
                "GGGGGGGGGGGLGGGLGNVLGGLISGAGGGGGGGGGGGGGGGGGGGGTAM[Common Variable:Oxidation on M]R",
                "GGGGGGGGGGGLGGGLGNVLGGLISGAGGGGGGGGGGGGGGGGGGGGTAMR",
                "GGSGGSHGGGSGFGGESGGSYGGGEEASGSGGGYGGGSGK",
                "ALANVNIGSLIC[Common Fixed:Carbamidomethyl on C]NVGAGGPAPAAGAAPAGGPAPSTAAAPAEEK",
                "GSYGSGGSSYGSGGGSYGSGGGGGGHGSYGSGSSSGGYR",
                "KIEPELDGSAQVTSHDASTNGLINFIK",
                "TPGAATASASGAAEDGAC[Common Fixed:Carbamidomethyl on C]GC[Common Fixed:Carbamidomethyl on C]LPNPGTFEEC[Common Fixed:Carbamidomethyl on C]HR",
                "TDNAGDQHGGGGGGGGGAGAAGGGGGGENYDDPHK",
                "AQGPAASAEEPKPVEAPAAN[Common Artifact:Deamidation on N]SDQTVTVKE", // same until here
                "LPTGYYFGASAGTGDLSDNHDIISMK",
                "TTSGYAGGLSSAYGGLTSPGLSYSLGSSFGSGAGSSSFSR",
                "FQSAAIGALQEASEAYLVGLFEDTNLC[Common Fixed:Carbamidomethyl on C]AIHAK",
                "QEPQPQGPPPAAGAVASYDYLVIGGGSGGLASAR",
                "VIHDNFGIVEGLMTTVHAITATQK",
                "VLGETANILETGETLEPAGAHLVLEEK",
                "LEASDGGLDSAELAAELGMEHQAVVGAVK",
                "PDPGSGGQPAGPGAAGEALAVLTSFGR",
                "TAVNVPRQPTVTSVC[Common Fixed:Carbamidomethyl on C]SETSQELAEGQR",
                "PAVVETVTTAKPQQIQALMDEVTK",
                "KGTEN[Common Artifact:Deamidation on N]GVNGTLTSNVADSPR",
                "ALANVNIGSLIC[Common Fixed:Carbamidomethyl on C]N[Common Artifact:Deamidation on N]VGAGGPAPAAGAAPAGGPAPSTAAAPAEEK",
                "TFEHVTSEIGAEEAEEVGVEHLLR"
            },
            // File 2
            new string[]
            {
                "GGGGGGGGGGGLGGGLGNVLGGLISGAGGGGGGGGGGGGGGGGGGGGTAM[Common Variable:Oxidation on M]R",
                "GGGGGGGGGGGLGGGLGNVLGGLISGAGGGGGGGGGGGGGGGGGGGGTAMR",
                "GGSGGSHGGGSGFGGESGGSYGGGEEASGSGGGYGGGSGK",
                "ALANVNIGSLIC[Common Fixed:Carbamidomethyl on C]NVGAGGPAPAAGAAPAGGPAPSTAAAPAEEK",
                "GSYGSGGSSYGSGGGSYGSGGGGGGHGSYGSGSSSGGYR",
                "KIEPELDGSAQVTSHDASTNGLINFIK",
                "TPGAATASASGAAEDGAC[Common Fixed:Carbamidomethyl on C]GC[Common Fixed:Carbamidomethyl on C]LPNPGTFEEC[Common Fixed:Carbamidomethyl on C]HR",
                "TDNAGDQHGGGGGGGGGAGAAGGGGGGENYDDPHK",
                "AQGPAASAEEPKPVEAPAAN[Common Artifact:Deamidation on N]SDQTVTVKE", // same until here
                "HIDC[Common Fixed:Carbamidomethyl on C]AYVYQNEHEVGEAIQEK",
                "IGTYTGPLQHGIVYSGGSSDTIC[Common Fixed:Carbamidomethyl on C]DLLGAK",
                "QGSATDN[Common Artifact:Ammonia loss on N]VC[Common Fixed:Carbamidomethyl on C]HLFAEHDPEQPASAIVNFVSK",
                "LNDHFPLVVWQTGSGTQTNMNVNEVISNR",
                "LTGSSAQEEASGVALGEAPDHSYESLR",
                "MEIVELEDHPFFVGVQYHPEFLSR",
                "EPTSSEQGGLEGSGSAAGEGKPALSEEER",
                "[UniProt:N-acetylalanine on A]AAAAAAGSGTPREEEGPAGEAAASQPQAPTSVPGAR",
                "DGNASGTTLLEALDC[Common Fixed:Carbamidomethyl on C]ILPPTRPTDK",
                "KELEQVC[Common Fixed:Carbamidomethyl on C]NPIISGLYQGAGGPGPGGFGAQGPK",
                "HIDC[Common Fixed:Carbamidomethyl on C]AHVYQNENEVGVAIQEK",
                "ILNHVLQHAEPGNAQSVLEAIDTYC[Common Fixed:Carbamidomethyl on C]EQK",
                "[Common Artifact:Ammonia loss on C]C[Common Fixed:Carbamidomethyl on C]EAFGWHAIIVDGHSVEELC[Common Fixed:Carbamidomethyl on C]K"
            },
            // File 3
            new string[]
            {
                "GGGGGGGGGGGLGGGLGNVLGGLISGAGGGGGGGGGGGGGGGGGGGGTAM[Common Variable:Oxidation on M]R",
                "GGGGGGGGGGGLGGGLGNVLGGLISGAGGGGGGGGGGGGGGGGGGGGTAMR",
                "GGSGGSHGGGSGFGGESGGSYGGGEEASGSGGGYGGGSGK",
                "ALANVNIGSLIC[Common Fixed:Carbamidomethyl on C]NVGAGGPAPAAGAAPAGGPAPSTAAAPAEEK",
                "GSYGSGGSSYGSGGGSYGSGGGGGGHGSYGSGSSSGGYR",
                "KIEPELDGSAQVTSHDASTNGLINFIK",
                "TPGAATASASGAAEDGAC[Common Fixed:Carbamidomethyl on C]GC[Common Fixed:Carbamidomethyl on C]LPNPGTFEEC[Common Fixed:Carbamidomethyl on C]HR",
                "TDNAGDQHGGGGGGGGGAGAAGGGGGGENYDDPHK",
                "AQGPAASAEEPKPVEAPAAN[Common Artifact:Deamidation on N]SDQTVTVKE", // same until here
                "VDTVNHLADELINSGHSDAATIAEWK",
                "KYILGNPLTPGVTQGPQIDKEQYDK",
                "ADAGKEGNNPAEN[Common Artifact:Deamidation on N]GDAKTDQAQK",
                "APALGGSFAGLEPMGLLWALEPEKPLVR",
                "VDEGAGDSAAVASGGAQTLALAGSPAPSGHPK",
                "LGPASAADTGSEAKPGALAEGAAEPEPQR",
                "SKDDQVTVIGAGVTLHEALAAAELLK",
                "EAPAEGEAAEPGSPTAAEGEAASAASSTSSPK",
                "SSSSSSASAAAAAAAASSSASC[Common Fixed:Carbamidomethyl on C]SR",
                "TGQEYKPGNPPAEIGQNISSNSSASILESK",
                "SGPFGQIFRPDNFVFGQSGAGNNWAK",
                "SVGDGETVEFDVVEGEKGAEAANVTGPGGVPVQGSK",
                "TLTGTVIDSGDGVTHVIPVAEGYVIGSC[Common Fixed:Carbamidomethyl on C]IK"
            },
        };
        _retentionTimes = new List<double[]>()
        {
            // File 1
            new double[]
            {
                175.24016,
                166.06129,
                34.39925,
                173.39687,
                53.8748,
                139.34978,
                81.20449,
                42.50767,
                57.43386,// same until here
                146.18404,
                176.35578,
                187.35956,
                168.12843,
                188.03275,
                168.27943,
                160.18187,
                184.17795,
                77.53021,
                163.09807,
                38.61063,
                173.39687,
                132.24575
            },
            // File 2
            new double[]
            {
                175.24016,
                166.06129,
                34.39925,
                173.39687,
                53.8748,
                139.34978,
                81.20449,
                42.50767,
                57.43386,// same until here
                94.26678,
                166.45034,
                181.4612,
                171.35139,
                101.76396,
                181.19547,
                66.58133,
                73.27514,
                185.144,
                143.9447,
                80.80877,
                145.30089,
                165.8938

            },
            // File 3
            new double[]
            {
                175.24016,
                166.06129,
                34.39925,
                173.39687,
                53.8748,
                139.34978,
                81.20449,
                42.50767,
                57.43386,// same until here
                161.31821,
                111.66912,
                12.49289,
                185.66262,
                78.28083,
                62.0802,
                182.65081,
                59.18985,
                39.12168,
                92.36567,
                179.37413,
                113.848,
                181.89493
            },
        };

        _dataFiles = new MsDataFile[]
        {
            MakeFakeMsDataFile(_fullSequences.First().Length),
            MakeFakeMsDataFile(_fullSequences.First().Length),
            MakeFakeMsDataFile(_fullSequences.First().Length)
        };

    }

    #region Helper Functions
    private MsDataFile MakeFakeMsDataFile(int numberOfScans)
    {
        MsDataFile msDataFile = new GenericMsDataFile(new MsDataScan[numberOfScans],
            new SourceFile(null, null,
                null, null, null));
        return msDataFile;
    }

    private Dictionary<string, List<string>> GetFullSequences()
    {
        Dictionary<string, List<string>> dict = new();
        for (int i = 0; i < _dataFiles.Length; i++)
        {
            dict[_dataFiles[i].FilePath] = _fullSequences[i].ToList();
        }

        return dict;
    }

    private Dictionary<string, List<double>> GetRetentionTimes()
    {
        Dictionary<string, List<double>> dict = new();
        for (int i = 0; i < _dataFiles.Length; i++)
        {
            dict[_dataFiles[i].FilePath] = _retentionTimes[i].ToList();
        }

        return dict;
    }

    #endregion


    [Test]
    public void TestRetentionTimeCalibrationConstructor()
    {
        // Act
        RetentionTimeCalibration.RetentionTimeCalibration retentionTimeCalibration =
            new RetentionTimeCalibration.RetentionTimeCalibration(_dataFiles, GetFullSequences(), GetRetentionTimes());

        // Assert
        Assert.That(retentionTimeCalibration.ResultsFiles.Count == 1);
    }

    [Test]
    public void TestGetAnchors()
    {

    }

    [Test]
    private void SnipMzmlForTestData()
    {
        string origDataFile = @"E:\Projects\GlycoNickLei_PXD017646\2019_07_30_GlycoPepMix_35trig_EThcD35_rep2.raw";
        int startScan = 5715;
        int endScan = 5915;
        FilteringParams filter = new FilteringParams(200, 0.01, 1, null, false, false, true);
        var reader = MsDataFileReader.GetDataFile(origDataFile);
        reader.LoadAllStaticData(filter, 1);

        var scans = reader.GetAllScansList();
        var scansToKeep = scans.Where(x => x.OneBasedScanNumber >= startScan && x.OneBasedScanNumber <= endScan).ToList();

        List<(int oneBasedScanNumber, int? oneBasedPrecursorScanNumber)> scanNumbers = new List<(int oneBasedScanNumber, int? oneBasedPrecursorScanNumber)>();
        foreach (var scan in scansToKeep)
        {
            if (scan.OneBasedPrecursorScanNumber.HasValue)
            {
                scanNumbers.Add((scan.OneBasedScanNumber, scan.OneBasedPrecursorScanNumber.Value));
            }
            else
            {
                scanNumbers.Add((scan.OneBasedScanNumber, null));
            }
        }

        Dictionary<int, int> scanNumberMap = new Dictionary<int, int>();

        foreach (var scanNumber in scanNumbers)
        {
            if (!scanNumberMap.ContainsKey(scanNumber.oneBasedScanNumber) && (scanNumber.oneBasedScanNumber - startScan + 1) > 0)
            {
                scanNumberMap.Add(scanNumber.oneBasedScanNumber, scanNumber.oneBasedScanNumber - startScan + 1);
            }
            if (scanNumber.oneBasedPrecursorScanNumber.HasValue && !scanNumberMap.ContainsKey(scanNumber.oneBasedPrecursorScanNumber.Value) && (scanNumber.oneBasedPrecursorScanNumber.Value - startScan + 1) > 0)
            {
                scanNumberMap.Add(scanNumber.oneBasedPrecursorScanNumber.Value, scanNumber.oneBasedPrecursorScanNumber.Value - startScan + 1);
            }
        }
        List<MsDataScan> scansForTheNewFile = new List<MsDataScan>();


        foreach (var scanNumber in scanNumbers)
        {
            MsDataScan scan = scansToKeep.First(x => x.OneBasedScanNumber == scanNumber.oneBasedScanNumber);

            int? newOneBasedPrecursorScanNumber = null;
            if (scan.OneBasedPrecursorScanNumber.HasValue && scanNumberMap.ContainsKey(scan.OneBasedPrecursorScanNumber.Value))
            {
                newOneBasedPrecursorScanNumber = scanNumberMap[scan.OneBasedPrecursorScanNumber.Value];
            }
            MsDataScan newDataScan = new MsDataScan(
                scan.MassSpectrum,
                scanNumberMap[scan.OneBasedScanNumber],
                scan.MsnOrder,
                scan.IsCentroid,
                scan.Polarity,
                scan.RetentionTime,
                scan.ScanWindowRange,
                scan.ScanFilter,
                scan.MzAnalyzer,
                scan.TotalIonCurrent,
                scan.InjectionTime,
                scan.NoiseData,
                scan.NativeId.Replace(scan.OneBasedScanNumber.ToString(), scanNumberMap[scan.OneBasedScanNumber].ToString()),
                scan.SelectedIonMZ,
                scan.SelectedIonChargeStateGuess,
                scan.SelectedIonIntensity,
                scan.IsolationMz,
                scan.IsolationWidth,
                scan.DissociationType,
                newOneBasedPrecursorScanNumber,
                scan.SelectedIonMonoisotopicGuessMz,
                scan.HcdEnergy
            );
            scansForTheNewFile.Add(newDataScan);
        }

        string outPath = origDataFile.Replace(".raw", "_snip.mzML").ToString();

        SourceFile sourceFile = new SourceFile(reader.SourceFile.NativeIdFormat,
            reader.SourceFile.MassSpectrometerFileFormat, reader.SourceFile.CheckSum, reader.SourceFile.FileChecksumType, reader.SourceFile.Uri, reader.SourceFile.Id, reader.SourceFile.FileName);


        MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(new GenericMsDataFile(scansForTheNewFile.ToArray(), sourceFile), outPath, false);
    }

}