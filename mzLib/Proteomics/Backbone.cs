namespace Proteomics;

public class AmideBond : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Carbon Carbon = new(Greek.AmideCarbon);
    public Oxygen Oxygen = new(Greek.AmideOxygen);
    public Nitrogen Nitrogen = new(Greek.AmideNitrogen);
    public Hydrogen Hydrogen = new();
    public ResidueNode PreviousBlock { get; set; }
    public AmideBond(ResidueNode previousBlock)
    {
        PreviousBlock = previousBlock;

        //connect Carbon to alpha carbon
        Carbon.Bonds[0] = PreviousBlock.AlphaCarbon;
        PreviousBlock.AlphaCarbon.Bonds[3] = Carbon;

        //connect Nitrogen to Carbon
        Carbon.Bonds[1] = Nitrogen;
        Nitrogen.Bonds[0] = Carbon;

        //connect Oxygen to Carbon
        Carbon.Bonds[2] = Oxygen;
        Oxygen.Bonds[0] = Carbon;

        //connect Hydrogen to Nitrogen
        Nitrogen.Bonds[1] = Hydrogen;
        Hydrogen.Bonds[0] = Nitrogen;

        //connect the previous block to the Carbon
        Carbon.Bonds[3] = PreviousBlock;
        PreviousBlock.Bonds[0] = Carbon;
    }
}

//public class Backbone : IBuildingBlocks
//{
//    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
//    public Nitrogen Nitrogen = new();
//    public Hydrogen AmineHydrogen1 = new();
//    public Oxygen Oxygen = new();
//    public Carbon AlphaCarbon = new();
//    public Hydrogen AlphaHydrogen = new();
//    public IBuildingBlocks SideChain { get; set; }

//    public Backbone(Residue sidechain)
//    {
//        SideChain = sidechain;

//        //connect Amine group to Alpha Carbon
//        Nitrogen.Bonds[0] = AlphaCarbon;
//        AlphaCarbon.Bonds[0] = Nitrogen;

//        //connect Alpha Carbon to Alpha Hydrogen
//        AlphaCarbon.Bonds[1] = AlphaHydrogen;
//        AlphaHydrogen.Bonds[0] = AlphaCarbon;

//        //connect Alpha Carbon to Carboxyl group
//        AlphaCarbon.Bonds[2] = Oxygen;
//        Oxygen.Bonds[0] = AlphaCarbon;

//        //connect side chain to Alpha Carbon
//        AlphaCarbon.Bonds[3] = SideChain;
//        SideChain.Bonds[0].Bonds[0] = AlphaCarbon;
//    }

//    //todo: come up with a method to connect all the backbone blocks
//}

public class NTerminus : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Nitrogen Nitrogen = new(Greek.NtNitrogen);
    public Hydrogen AmineHydrogen1 = new();
    public Hydrogen AmineHydrogen2 = new();

    //todo: protonation of the Nitrogen x 2 and Alpha Carbon x 1
    public NTerminus() : base()
    {
        //connect Nitrogen to Amine Hydrogen 1
        Nitrogen.Bonds[0] = AmineHydrogen1;
        AmineHydrogen1.Bonds[0] = Nitrogen;

        //connect Nitrogen to Amine Hydrogen 2
        Nitrogen.Bonds[1] = AmineHydrogen2;
        AmineHydrogen2.Bonds[0] = Nitrogen;
    }
}

public class CTerminus : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Oxygen Oxygen1 = new(Greek.CtOxygen);
    public Oxygen Oxygen2 = new(Greek.CtOxygen);
    public Hydrogen Hydrogen = new();
    public Carbon Carbon = new(Greek.CtCarbon);
    public IBuildingBlocks PreviousBlock { get; set; }
    //todo: protonation of the Nitrogen x 1, Oxygen x 2, and Alpha Carbon x 1
    public CTerminus(IBuildingBlocks previousBlock) 
    {
        PreviousBlock = previousBlock;

        //Connect the previous block to the Carbon
        Carbon.Bonds[0] = PreviousBlock;
        PreviousBlock.Bonds[3] = Carbon;

        //Connect the Carbon to the Oxygen 1
        Carbon.Bonds[1] = Oxygen1;
        Oxygen1.Bonds[0] = Carbon;

        //Connect the Carbon to the Oxygen 2
        Carbon.Bonds[2] = Oxygen2;
        Oxygen2.Bonds[0] = Carbon;

        //Connect the Oxygen 2 to the Hydrogen
        Oxygen2.Bonds[1] = Hydrogen;
        Hydrogen.Bonds[0] = Oxygen2;
    }
}