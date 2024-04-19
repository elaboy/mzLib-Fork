using NUnit.Framework;
using Proteomics;

namespace Test;

public class TestPG
{
    [Test]
    public void TestNTerminalNode()
    {
        var Nt = new Proteomics.NTerminus();

        Assert.AreEqual(Proteomics.Greek.NtNitrogen, Nt.Nitrogen.ValenceElectrons);
    }

    [Test]
    public void TestCTerminalNode()
    {
        var Ct = new Proteomics.CTerminus(previousBlock: new NTerminus());

        Assert.AreEqual(Proteomics.Greek.CtCarbon, Ct.Carbon.ValenceElectrons);
    }

    [Test]
    public void TestPeptidePG()
    {
        string peptide = "PEPTIDE";
        var pg = new Proteomics.PG(peptide);
        Assert.That(pg.Nodes.Count == 9);
    }

    [Test]
    public void TestGetPartialChargeOfAlanine()
    {
        string peptide = "A";
        var pg = new Proteomics.PG(peptide);
        Assert.That(pg == null);
    }

}
