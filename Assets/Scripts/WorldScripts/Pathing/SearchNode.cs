using UnityEngine;

/*
    POINTER/COPY of NODE
    Because nodes may be examined simultaneously, we have to create a temporary copy of a node for the purpose of pathfinding.
    A search node knows which node it is a copy of (paired node, along with its unique key/id), which node was most recently examined prior to this one by the pathfinder (parentnode),
    and the individual values needed for A*.

    REVISIT THIS IF POSSIBLE
*/
public class SearchNode : System.IComparable<SearchNode>, QueueType<int>
{
    private Node pairedNode;
    public SearchNode ParentNode { get; set; }
    public int F { get; set; }
    public int G { get; set; }
    public int H { get; set; }
    public int key { get; set; }

    public SearchNode(Node nodeIn)
    {
        pairedNode = nodeIn;
        key = pairedNode.NodeID;
    }

    public Node PairedNode
    {
        get { return pairedNode; }
    }

    public void SetF()
    {
        F = H + G;
    }

    public int CompareTo(SearchNode nodeIn)
    {
        if (nodeIn == null) return 1;

        int contrast = this.F.CompareTo(nodeIn.F);
        if (contrast == 0)
        {
            return this.H.CompareTo(nodeIn.H);
        }
        return contrast;
    }

    public override string ToString()
    {
        return "SearchNode Paired to: (" + pairedNode + ") with ID: " + key;
    }

    public override bool Equals(object obj)
    {
        return this.pairedNode.Equals(((SearchNode)obj).PairedNode);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
