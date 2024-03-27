using Chemistry;
using System.Collections.Generic;

namespace Proteomics;
public class PG
{
    public Nterminus Nt = new Nterminus();
    public Cterminus Ct = new Cterminus();
    public List<Node> Nodes { get; set; }
    public int NetCharge { get; set; }
    public double SyspH { get; set; }
    public double SysTemp { get; set; }
    public double Polarity { get; set; }

    public PG()
    {
        //always start with a node for the N-terminus

        //always end with the C-terminus

    }

    /// <summary>
    /// Sets partial charges for all atoms in the PG using the Gasteiger-Marsili method
    /// </summary>
    public void SetPartialChargesGM()
    {

    }

    /// <summary>
    /// Use Json file to set radii for all atoms in the PG
    /// </summary>
    public void SetRadiusForEntireNetwork()
    {

    }
}

public class DetailedElement : Element
{
    //Electronegativity from Pauling scale in the JSon file
    public double PaulingElectronegativity { get; set; }
    public double PartialCharge { get; set; }
    public ValenceElectrons ValenceElectrons { get; set; }
    public double FormalCharge { get; set; }

    public DetailedElement(string symbol, int atomicNumber, double averageMass, double paulingElectronegativity)
        : base(symbol, atomicNumber, averageMass)
    {
        PaulingElectronegativity = paulingElectronegativity;
    }
}

public class Backbone
{
    public Nitrogen AmineGroup = new Nitrogen();
    public Oxygen CarboxylGroup = new Oxygen();
    public Carbon AlphaCarbon = new Carbon();
    public Hydrogen AlphaHydrogen = new Hydrogen();
    public IBuildingBlocks SideChain { get; set; }

    public Backbone(IBuildingBlocks sidechain)
    {
        SideChain = sidechain;

        //connect Amine group to Alpha Carbon
        AmineGroup.Bonds[0] = AlphaCarbon;
        AlphaCarbon.Bonds[0] = AmineGroup;

        //connect Alpha Carbon to Alpha Hydrogen
        AlphaCarbon.Bonds[1] = AlphaHydrogen;
        AlphaHydrogen.Bonds[0] = AlphaCarbon;

        //connect Alpha Carbon to Carboxyl group
        AlphaCarbon.Bonds[2] = CarboxylGroup;
        CarboxylGroup.Bonds[0] = AlphaCarbon;

        //connect side chain to Alpha Carbon
        AlphaCarbon.Bonds[3] = SideChain;
        SideChain.Bonds[0].Bonds[0] = AlphaCarbon;
    }
}

public static class Elements
{
    public static Element H = new Element("H", 1, 1.00782503207);
    public static Element C = new Element("C", 6, 12.0000000000);
    public static Element N = new Element("N", 7, 14.00307400443);
    public static Element O = new Element("O", 8, 15.99491461957);
    public static Element S = new Element("S", 16, 31.97207100);
    public static Element P = new Element("P", 15, 30.97376199842);
}

public enum GreekCarbons
{
    Alpha = 1,
    Beta = 2,
    Gamma = 3,
    Delta = 4,
    Epsilon = 5,
    Zeta = 6,
    Eta = 7,
    Theta = 8,
    Iota = 9,
}

public enum ValenceElectrons
{
    C = 4,
    H = 1,
    N = 5,
    O = 6,
    S = 6,
    P = 5
}