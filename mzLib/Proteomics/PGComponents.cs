using Chemistry;

namespace Proteomics;
public class Nterminus
{
    public static ChemicalFormula AtomicComposition = new ChemicalFormula(ChemicalFormula.ParseFormula("C2H3NO"));
    public static ChemicalFormula? Modification = new ChemicalFormula();

    public Nterminus()
    {

    }
}

public class Cterminus
{
    public static ChemicalFormula AtomicComposition = new ChemicalFormula(ChemicalFormula.ParseFormula("C2H3O2"));

    public Cterminus()
    {

    }
}

public class Node
{
    public Backbone NodeBackbone { get; private set; }
    public RGroup NodeResidue { get; private set; }
    public Node? PreviousNode { get; private set; }
    public Node? NextNode { get; private set; }
    public int NodeNumber { get; private set; }

    public Node(RGroup residue, Node? previousNode, Node? nextNode)
    {
        NodeResidue = residue;
        PreviousNode = previousNode;
        NextNode = nextNode;
    }
}

public class RGroup
{
    public RGroup()
    {

    }
}

public class Backbone
{

    public Backbone()
    {

    }
}




