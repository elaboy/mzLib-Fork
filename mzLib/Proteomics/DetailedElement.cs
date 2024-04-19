using Chemistry;

namespace Proteomics;

#region Elemental

//todo: add different states of oxidation states such as Fe2+ and Fe3+
public class Carbon : DetailedElement
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.C];
    public Greek CarbonType { get; set; }
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Carbon(Greek carbonType, bool protonated = false) :
        base("C", 6, 12.0000000000, 2.55, protonated)
    {
        CarbonType = carbonType;
    }
}

public class Nitrogen : DetailedElement
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.N];
    public Greek NitrogenType { get; set; }
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Nitrogen(Greek nitrogenType) : base("N", 7, 14.00307400443, 3.04)
    {
        NitrogenType = nitrogenType;
    }
}

public class Oxygen : DetailedElement
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.O];
    public Greek OxygenType { get; set; }
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Oxygen(Greek oxygenType) : base("O", 8, 15.99491461957, 3.44)
    {
        OxygenType = oxygenType;
    }
}

public class Hydrogen : DetailedElement
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.H];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Hydrogen() : base("H", 1, 1.00782503207, 2.20)
    {
    }
}

public class Sulfur : DetailedElement
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.S];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }
    public Greek SulfurType { get; set; }
    public Sulfur(Greek sulfurType) : base("S", 16, 32.065, 2.58)
    {
        SulfurType = sulfurType;
    }
}

public class Phosphorus : DetailedElement
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.P];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Phosphorus() : base("P", 15, 30.97376199842, 2.19)
    {
    }
}

#endregion

public class DetailedElement : Element, IBuildingBlocks
{
    //Electronegativity from Pauling scale in the JSon file
    public double PaulingElectronegativity { get; set; }
    public double PartialCharge { get; set; }
    public ValenceElectrons ValenceElectrons { get; set; }
    public double FormalCharge { get; set; }
    public IBuildingBlocks[] Bonds { get; set; }

    public DetailedElement(string symbol, int atomicNumber, double averageMass, double paulingElectronegativity, bool protonated = false)
        : base(symbol, atomicNumber, averageMass)
    {
        PaulingElectronegativity = paulingElectronegativity;
        if (protonated) 
            Protonate();
        
    }

    public void Protonate()
    {
        switch (AtomicSymbol)
        {
            case "C":
                for (var i = 0; i < Bonds.Length; i++)
                    if (Bonds[i] == null)
                        Bonds[i] = new Hydrogen();
                break;

            //Nitrogen has 2 bonds and 1 lone e pair
            case "N":
                for (var i = 0; i < Bonds.Length - 2; i++)
                    if (Bonds[i] == null)
                        Bonds[i] = new Hydrogen();

                //add electrons to empty spaces in the array
                for (var i = Bonds.Length - 2; i < Bonds.Length; i++)
                    if (Bonds[i] == null)
                        Bonds[i] = new Electron();
                break;

            //Oxygen has 2 bonds and 2 lone e pairs
            case "O":
                for (var i = 0; i < Bonds.Length - 4; i++)
                    if (Bonds[i] == null)
                        Bonds[i] = new Hydrogen();

                //add electrons to empty spaces in the array
                for (var i = Bonds.Length - 4; i < Bonds.Length; i++)
                    if (Bonds[i] == null)
                        Bonds[i] = new Electron();
                break;
        }
    }
}