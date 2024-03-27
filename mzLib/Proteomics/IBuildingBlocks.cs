namespace Proteomics;

public interface IBuildingBlocks
{
    public DetailedElement? BuildingBlock { get; set; }
    public IBuildingBlocks[] Bonds { get; set; }

    public void Protonate()
    {
        for (int i = 0; i < Bonds.Length; i++)
        {
            if (Bonds[i].BuildingBlock == null)
            {
                Bonds[i] = new Hydrogen();
            }
        }
    }
}