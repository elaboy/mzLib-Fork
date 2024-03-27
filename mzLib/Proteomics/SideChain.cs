using System;
using Proteomics;

namespace Proteomics;

public class LonePairElectrons : IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = null;

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[2];
    public AtomicRadius AtomicRadius { get; set; } = null;
    public double PartialCharge { get; set; }

    public LonePairElectrons() : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}


#region Elemental
public class Carbon : IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = new DetailedElement("C", 6, 12.0000000000, 2.55);

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.C];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Carbon() : base()
    { }
}

public class Nitrogen : IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = new DetailedElement("N", 7, 14.00307400443, 3.04);

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.N];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Nitrogen() : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Oxygen : IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = new DetailedElement("O", 8, 15.99491461957, 3.44);

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.O];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Oxygen() : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Hydrogen : IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = new DetailedElement("H", 1, 1.00782503207, 2.20);

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.H];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Hydrogen() : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Sulfur : IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = new DetailedElement("S", 16, 32.065, 2.58);

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.S];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Sulfur() : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Phosphorus : DetailedElement, IBuildingBlocks
{
    public DetailedElement BuildingBlock { get; set; } = new DetailedElement("P", 15, 30.97376199842, 2.19);

    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)ValenceElectrons.P];
    public AtomicRadius AtomicRadius { get; set; }
    public double PartialCharge { get; set; }

    public Phosphorus() : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}
#endregion


#region SideChains
//https://bmrb.cerm.unifi.it/referenc/commonaa.php?tyr
public class Alanine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Alanine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Cysteine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Cysteine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class AsparticAcid : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public AsparticAcid(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class GlutamicAcid : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public GlutamicAcid(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Phenylalanine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Phenylalanine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Glycine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Glycine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Histidine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Histidine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Isoleucine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Isoleucine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Lysine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Lysine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Leucine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Leucine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Methionine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Methionine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Asparagine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Asparagine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Proline : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Proline(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Glutamine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Glutamine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Arginine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Arginine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Serine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Serine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Threonine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Threonine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Valine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Valine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Tryptophan : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Tryptophan(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}

public class Tyrosine : IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; } = null;

    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[(int)GreekCarbons.Alpha];

    public Tyrosine(Backbone backbone) : base() { }

    public void Protonate()
    {
        throw new NotImplementedException();
    }
}
#endregion
