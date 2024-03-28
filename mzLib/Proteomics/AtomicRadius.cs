using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Proteomics;

public class AtomicRadius
{
    public int AtomicNumber { get; private set; }
    public string Symbol { get; private set; }
    public double AtomicMass { get; private set; }
    public string CPKHexColor { get; private set; }
    public string ElectronConfiguration { get; private set; }
    public double Electronegativity { get; private set; }
    public double AtomicRadiusVDW { get; private set; }
    public double IonizationEnergy { get; private set; }
    public double ElectronAffinity { get; private set; }
    public double OxidationState { get; private set; }
    public string StandardState { get; private set; }
    public double MeltingPoint { get; private set; }
    public double BoilingPoint { get; private set; }
    public double Density { get; private set; }
    public string Group { get; private set; }
    public string YearDiscovered { get; private set; }

    /// <summary>
    /// takes the path to the file containing the atomic radius data. Default is PubChemElements_all.json
    /// </summary>
    public List<AtomicRadius> GetAtomicRadiusList()
    {
        using StreamReader reader = new(Path.Combine("PubChemElements_all.json"));
        string json = reader.ReadToEnd();
        List<AtomicRadius> elements = JsonSerializer.Deserialize<List<AtomicRadius>>(json);

        return elements;
    }

    public static double GetAtomicRadius(List<AtomicRadius> elements, int atomicNumber)
    {
        AtomicRadius element = elements.Find(x => x.AtomicNumber == atomicNumber);
        return element.AtomicRadiusVDW;
    }
}