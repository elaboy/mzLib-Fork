using System.Collections.Generic;

namespace Proteomics;

public class PG
{
    public Nterminus Nt = new();
    public Cterminus Ct = new();
    public List<IBuildingBlocks> Nodes { get; set; }
    public int NetCharge { get; set; }
    public double SyspH { get; set; }
    public double SysTemp { get; set; }
    public double Polarity { get; set; }

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