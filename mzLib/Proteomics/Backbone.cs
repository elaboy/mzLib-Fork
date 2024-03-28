namespace Proteomics;

public class AmideBond : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Carbon Carbon = new();
    public Oxygen Oxygen = new();
    public Nitrogen Nitrogen = new();
    public Hydrogen Hydrogen = new();

    public AmideBond(Backbone previousBlock)
    {
        //connect Carbon to Nitrogen
        Carbon.Bonds[0] = previousBlock.AlphaCarbon;
        Nitrogen.Bonds[0] = Carbon;

        //connect Nitrogen to Hydrogen
        Nitrogen.Bonds[1] = Hydrogen;
        Hydrogen.Bonds[0] = Nitrogen;

        //connect Nitrogen to Oxygen
        Nitrogen.Bonds[2] = Oxygen;
        Oxygen.Bonds[0] = Nitrogen;

        //connect Oxygen to Carbon
        Oxygen.Bonds[1] = Carbon;
        Carbon.Bonds[1] = Oxygen;

        //connect the previous block to the Carbon
        Carbon.Bonds[3] = previousBlock;
        previousBlock.Bonds[0] = Carbon;
    }
}

public class Backbone : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Nitrogen Nitrogen = new();
    public Hydrogen AmineHydrogen1 = new();
    public Oxygen Oxygen = new();
    public Carbon AlphaCarbon = new();
    public Hydrogen AlphaHydrogen = new();
    public IBuildingBlocks SideChain { get; set; }

    public Backbone(SideChain sidechain)
    {
        SideChain = sidechain;

        //connect Amine group to Alpha Carbon
        Nitrogen.Bonds[0] = AlphaCarbon;
        AlphaCarbon.Bonds[0] = Nitrogen;

        //connect Alpha Carbon to Alpha Hydrogen
        AlphaCarbon.Bonds[1] = AlphaHydrogen;
        AlphaHydrogen.Bonds[0] = AlphaCarbon;

        //connect Alpha Carbon to Carboxyl group
        AlphaCarbon.Bonds[2] = Oxygen;
        Oxygen.Bonds[0] = AlphaCarbon;

        //connect side chain to Alpha Carbon
        AlphaCarbon.Bonds[3] = SideChain;
        SideChain.Bonds[0].Bonds[0] = AlphaCarbon;
    }

    //todo: come up with a method to connect all the backbone blocks
}

public class NTerminus : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Nitrogen Nitrogen = new();
    public Hydrogen AmineHydrogen1 = new();
    public Hydrogen AmineHydrogen2 = new();
    public Carbon AlphaCarbon = new();
    public Hydrogen AlphaHydrogen = new();
    public IBuildingBlocks SideChain { get; set; }

    //todo: protonation of the Nitrogen x 2, Oxygen x 1, and Alpha Carbon x 1
    public NTerminus(Backbone sideChain) 
    {
        SideChain = sideChain;

        //connect Amine group to Alpha Carbon
        Nitrogen.Bonds[0] = AlphaCarbon;
        AlphaCarbon.Bonds[0] = Nitrogen;

        //connect Alpha Carbon to Alpha Hydrogen
        AlphaCarbon.Bonds[1] = AlphaHydrogen;
        AlphaHydrogen.Bonds[0] = AlphaCarbon;

        //connect Alpha Carbon to Carboxyl group
        AlphaCarbon.Bonds[2] = Oxygen;
        Oxygen.Bonds[0] = AlphaCarbon;

        //connect side chain to Alpha Carbon
        AlphaCarbon.Bonds[3] = SideChain;
        SideChain.Bonds[0].Bonds[0] = AlphaCarbon;
    }
}

public class CTerminus : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Oxygen Oxygen = new();
    public Hydrogen CarboxylHydrogen = new();
    public Carbon Carbon = new();
    public IBuildingBlocks SideChain { get; set; }

    //todo: protonation of the Nitrogen x 1, Oxygen x 2, and Alpha Carbon x 1
    public CTerminus(Backbone sideChain) 
    {
        SideChain = sideChain;
    }
}