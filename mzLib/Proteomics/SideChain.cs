namespace Proteomics;

public class Electron : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new SideChain[1];
}

#region SideChains

//https://bmrb.cerm.unifi.it/referenc/commonaa.php?tyr
public class Alanine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Alanine(Backbone backbone) : base(backbone) 
    {
    }
}

public class Cysteine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Cysteine(Backbone backbone) : base(backbone)
    {
    }
}

public class AsparticAcid : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public AsparticAcid(Backbone backbone) : base(backbone)
    {
    }
}

public class GlutamicAcid : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public GlutamicAcid(Backbone backbone) : base(backbone)
    {
    }
}

public class Phenylalanine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Phenylalanine(Backbone backbone) : base(backbone)
    {
    }
}

public class Glycine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Glycine(Backbone backbone) : base(backbone)
    {
    }
}

public class Histidine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Histidine(Backbone backbone) : base(backbone)
    {
    }
}

public class Isoleucine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Isoleucine(Backbone backbone) : base(backbone)
    {
    }
}

public class Lysine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Lysine(Backbone backbone) : base(backbone)
    {
    }
}

public class Leucine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Leucine(Backbone backbone) : base(backbone)
    {
    }
}

public class Methionine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Methionine(Backbone backbone) : base(backbone)
    {
    }
}

public class Asparagine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Asparagine(Backbone backbone) : base(backbone)
    {
    }
}

public class Proline : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Proline(Backbone backbone) : base(backbone)
    {
    }
}

public class Glutamine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Glutamine(Backbone backbone) : base(backbone)
    {
    }
}

public class Arginine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Arginine(Backbone backbone) : base(backbone)
    {
    }
}

public class Serine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Serine(Backbone backbone) : base(backbone)
    {
    }
}

public class Threonine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Threonine(Backbone backbone) : base(backbone)
    {
    }
}

public class Valine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Valine(Backbone backbone) : base(backbone)
    {
    }
}

public class Tryptophan : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Tryptophan(Backbone backbone) : base(backbone)
    {
    }
}

public class Tyrosine : SideChain
{
    //todo: add the side chain atoms
    public SideChain[] Bonds { get; set; } = new SideChain[(int)GreekCarbons.Alpha];

    public Tyrosine(Backbone backbone) : base(backbone)
    {
    }
}

#endregion