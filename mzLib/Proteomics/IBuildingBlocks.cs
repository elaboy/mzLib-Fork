namespace Proteomics;

public interface IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; }

    //todo: method to get all atoms connected to this block

    //todo: method to retrieve all atoms in the block
}

public class ResidueNode : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[4];
    public Carbon AlphaCarbon = new(Greek.Alpha);
    public Hydrogen AlphaHydrogen = new();
    public IBuildingBlocks PreviousBlock { get; set; }

    public ResidueNode(IBuildingBlocks previousBlock)
    {
        PreviousBlock = previousBlock;

        //Connect the previous block's to the backbone's alpha carbon
        PreviousBlock.Bonds[3] = AlphaCarbon;
        AlphaCarbon.Bonds[0] = PreviousBlock;

        //Connect the alpha carbon to the alpha hydrogen
        AlphaCarbon.Bonds[1] = AlphaHydrogen;
        AlphaHydrogen.Bonds[0] = AlphaCarbon;
    }
}