namespace Proteomics;

public interface IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; }

    //todo: method to get all atoms connected to this block

    //todo: method to retrieve all atoms in the block
}

public class SideChain : IBuildingBlocks
{
    public IBuildingBlocks[] Bonds { get; set; } = new IBuildingBlocks[9];

    public SideChain(Backbone backbone)
    {
    }
}