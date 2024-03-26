using Chemistry;
using System.Collections.Generic;
namespace Proteomics;
public class PG
{
    public Nterminus Nt = new Nterminus();
    public Cterminus Ct = new Cterminus();
    public List<Node> Nodes { get; private set; }
    public double SyspH { get; private set; }
    public PG()
    {
        //always start with a node for the N-terminus

        //always end with the C-terminus

    }
}

public class PGExtensions
{
    public double GetDipoleMoment()
    {
        return 0;
    }

    public double GetMass()
    {
        return 0;
    }

    public double GetCharge()
    {
        return 0;
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
