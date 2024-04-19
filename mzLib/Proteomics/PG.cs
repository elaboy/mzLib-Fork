using System.Collections.Generic;
using System.Linq;

namespace Proteomics;

public class PG
{
    public List<IBuildingBlocks> Nodes { get; set; }
    public int NetCharge { get; set; }
    public double SyspH { get; set; }
    public double SysTemp { get; set; }
    public double Polarity { get; set; }
    public string Sequence { get; set; }

    public PG(string sequence)
    {
        Sequence = sequence;
        Nodes = new List<IBuildingBlocks>();
        Nodes.Add(new NTerminus());
        AddResidueNode();
        Nodes.Add(new CTerminus(Nodes[^1]));
        ConnectBackbone();
    }

    public void AddResidueNode()
    {
        for (int i = 0; i < Sequence.Length; i++)
        {
            switch (Sequence[i])
            {
                case 'A':
                    Nodes.Add(new Alanine(Nodes[i]));
                    break;
                case 'C':
                    Nodes.Add(new Cysteine(Nodes[i]));
                    break;
                case 'D':
                    Nodes.Add(new AsparticAcid(Nodes[i]));
                    break;
                case 'E':
                    Nodes.Add(new GlutamicAcid(Nodes[i]));
                    break;
                case 'F':
                    Nodes.Add(new Phenylalanine(Nodes[i]));
                    break;
                case 'G':
                    Nodes.Add(new Glycine(Nodes[i]));
                    break;
                case 'H':
                    Nodes.Add(new Histidine(Nodes[i]));
                    break;
                case 'I':
                    Nodes.Add(new Isoleucine(Nodes[i]));
                    break;
                case 'K':
                    Nodes.Add(new Lysine(Nodes[i]));
                    break;
                case 'L':
                    Nodes.Add(new Leucine(Nodes[i]));
                    break;
                case 'M':
                    Nodes.Add(new Methionine(Nodes[i]));
                    break;
                case 'N':
                    Nodes.Add(new Asparagine(Nodes[i]));
                    break;
                case 'P':
                    Nodes.Add(new Proline(Nodes[i]));
                    break;
                case 'Q':
                    Nodes.Add(new Glutamine(Nodes[i]));
                    break;
                case 'R':
                    Nodes.Add(new Arginine(Nodes[i]));
                    break;
                case 'S':
                    Nodes.Add(new Serine(Nodes[i]));
                    break;
                case 'T':
                    Nodes.Add(new Threonine(Nodes[i]));
                    break;
                case 'V':
                    Nodes.Add(new Valine(Nodes[i]));
                    break;
                case 'W':
                    Nodes.Add(new Tryptophan(Nodes[i]));
                    break;
                case 'Y':
                    Nodes.Add(new Tyrosine(Nodes[i]));
                    break;
            }

            if (i != Sequence.Length - 1)
            {
                Nodes.Add(new AmideBond((ResidueNode)Nodes[^1]));
            }
        }
    }

    /// <summary>
    ///     Sets partial charges for all atoms in the PG using the Gasteiger-Marsili method
    /// </summary>
    public void SetPartialChargesGM()
    {
    }

    /// <summary>
    ///     Use Json file to set radii for all atoms in the PG
    /// </summary>
    public void SetRadiusForEntireNetwork()
    {
    }

    public void ConnectBackbone()
    {
        for (int i = 0; i < Nodes.Count - 1; i++)
        {
            Nodes[i].Bonds[3] = Nodes[i + 1];
            Nodes[i + 1].Bonds[0] = Nodes[i];
        }
    
    }

}

public enum Greek
{
    AmideCarbon = -5,
    AmideNitrogen = -4,
    AmideOxygen = -3,
    NtNitrogen = -2,
    CtOxygen = -1,
    CtCarbon = 0,
    Alpha = 1,
    Beta = 2,
    Gamma = 3,
    Delta = 4,
    Epsilon = 5,
    Zeta = 6,
    Eta = 7,
    Theta = 8,
    Iota = 9
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