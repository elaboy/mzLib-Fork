using System.Collections.Generic;
using System.Linq;

namespace Proteomics;

public class Electron : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[1];
}

#region ResidueNodes

//https://bmrb.cerm.unifi.it/referenc/commonaa.php?tyr
public class Alanine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];
    public IBuildingBlocks PreviousBlock { get; set; }
    public IBuildingBlocks[] SideChain { get; set; } = new IBuildingBlocks[1]
    {
        new Carbon(Greek.Beta, true)
    };

    public Alanine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
        PreviousBlock = previousBlock;
    }
}

public class Cysteine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public IBuildingBlocks[] SideChain { get; set; } = new IBuildingBlocks[2]
    {
        new Carbon(Greek.Beta),
        new Sulfur(Greek.Gamma)
    };

    public Cysteine(IBuildingBlocks previousBlock) : base(previousBlock)
    {

    }
}

public class AsparticAcid : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public AsparticAcid(IBuildingBlocks previousBlock) : base(previousBlock)
    {

    }
}

public class GlutamicAcid : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public GlutamicAcid(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Phenylalanine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Phenylalanine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Glycine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Glycine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Histidine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Histidine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Isoleucine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Isoleucine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Lysine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Lysine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Leucine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Leucine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Methionine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Methionine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Asparagine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Asparagine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Proline : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Proline(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Glutamine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Glutamine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Arginine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Arginine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Serine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Serine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Threonine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Threonine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Valine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Valine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Tryptophan : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Tryptophan(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

public class Tyrosine : ResidueNode
{
    //todo: add the side chain atoms
    public IBuildingBlocks[] Bonds { get; set; } = new ResidueNode[(int)Greek.Alpha];

    public Tyrosine(IBuildingBlocks previousBlock) : base(previousBlock)
    {
    }
}

#endregion