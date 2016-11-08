using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Node : System.IComparable<Node>
{
	private Vector3 pos;
	private int gridIndexX, gridIndexZ;

    private bool walkable;

	private Node parentNode;
	private GameObject tileObject; //GameObject residing in/on this tile
	
	private HashSet<Node> neighbors;
	private List<Node> temp;

    public Color flag { get; set; }

	public static float maxHop = .5f;

	/*
	 * hueristic values
	 * f - Composite Function -- f(x) = g(x) + h(x)
	 * h - Estimated cost from node to goal
	 * g - Exact cost from start to node
	 * */
	private int fScore, hScore, gScore; 

	private int clearance = 1;

    private static int NODECOUNT = 0;
    public int NodeID { get; set; }

	public Node(Vector3 posIn, int x, int z)
	{
		pos = posIn;
		walkable = true;
		tileObject = null;
		gridIndexX = x;
		gridIndexZ = z;
        initID();
	}

	public Node(Vector3 posIn, int x, int z, bool walkableIn)
	{
		pos = posIn;
		walkable = walkableIn;
		tileObject = null;
		gridIndexX = x;
		gridIndexZ = z;
        initID();
	}

	public Node(Vector3 posIn, int x, int z, bool walkableIn, GameObject tileObjectIn)
	{
		pos = posIn;
		walkable = walkableIn;
		tileObject = tileObjectIn;
		gridIndexX = x;
		gridIndexZ = z;
        initID();
	}

    public void initID()
    {
        NodeID = NODECOUNT;
        ++NODECOUNT;
    }

	/*
	 * -Evaluate all neighbors of this node
	 * -Build a set of nodes traversable from this one
	 * -Determine the clearance value of this node
	 * */
	public void EvaluateNeighbors()
	{
		neighbors = new HashSet<Node> ();
		temp = GetAllNeighbors ();

		bool done = false;
		for(int x = 0; x < temp.Count; ++x)
		{
			if((Mathf.Abs(temp[x].Pos.y - pos.y) > maxHop) || !temp[x].Walkable)
			{
				done = true;
			}
			else
				neighbors.Add(temp[x]);
		}

		if(!done)
		{
			for(int k = 1; k < 4; ++k)
			{
				foreach(Node n in GetExpandingBounds(k))
				{
					if(!n.Walkable)
					{
						done = true;
						break;
					}
				}

				clearance = k;
				if(done)
				{
					break;
				}
			}
		}
	}

	public Vector3 Pos
	{
		get
		{
			return pos;
		}
	}
	
	public int GridIndexX
	{
		get
		{
			return gridIndexX;
		}
	}

    public int GridIndexZ
    {
        get
        {
            return gridIndexZ;
        }
    }

	public bool Walkable
	{
		get
		{           
			return walkable;
		}
		set
		{
			walkable = value;
		}
	}

	public int Clearance
	{
		get
		{
			return clearance;
		}
	}

	public GameObject TileObject
	{
		get
		{
			return tileObject;
		}
		set
		{
			tileObject = value;
		}
	}

	public int F
	{
		get
		{
			return fScore;
		}
	}

	public int H
	{
		get
		{
			return hScore;
		}
		set
		{
			hScore = value;
		}
	}

	public int G
	{
		get
		{
			return gScore;
		}
		set
		{
			gScore = value;
		}
	}

	public Node ParentNode
	{
		get
		{
			return parentNode;
		}
		set
		{
			parentNode = value;
		}
	}

    public SearchNode SearchableNode
    {
        get
        {
            return new SearchNode(this);
        }
    }

    public HashSet<Node> Neighbors
    {
        get
        {
            return neighbors;
        }
    }

    public Node NearestWalkableNode(int count)
    {
        count++;
        if (count > 10)
            return null;
        Node walkableNode = null;
        if (this.walkable)
        {
            return this;
        }
        else
        {           
            foreach(Node n in Neighbors)
            {
                walkableNode = n.NearestWalkableNode(count);
            }
        }
        return walkableNode;
    }

	/*
	 * I DON'T KNOW IF I NEED THIS METHOD AT ALL
	 * */
	public static List<Node> GetAllNeighbors(Node n)
	{
		List<Node> temp = new List<Node> ();
		WorldGrid world = WorldManager._WORLD;

		temp.Add (world.GetTile (n.GridIndexX - 1, n.GridIndexZ)); //left
		temp.Add (world.GetTile (n.GridIndexX, n.GridIndexZ - 1)); //down
		temp.Add (world.GetTile (n.GridIndexX + 1, n.GridIndexZ)); //right
		temp.Add (world.GetTile (n.GridIndexX, n.GridIndexZ + 1)); //up
		temp.Add (world.GetTile (n.GridIndexX - 1, n.GridIndexZ - 1)); //left down
		temp.Add (world.GetTile (n.GridIndexX - 1, n.GridIndexZ + 1)); //left up
		temp.Add (world.GetTile (n.GridIndexX + 1, n.GridIndexZ - 1)); //right down
		temp.Add (world.GetTile (n.GridIndexX + 1, n.GridIndexZ + 1)); //right up

		return temp;
	}

	public List<Node> GetAllNeighbors()
	{
		temp = new List<Node> ();
		WorldGrid world = WorldManager._WORLD;

		temp.Add (world.GetTile (GridIndexX - 1, GridIndexZ)); //left
		temp.Add (world.GetTile (GridIndexX, GridIndexZ - 1)); //down
		temp.Add (world.GetTile (GridIndexX + 1, GridIndexZ)); //right
		temp.Add (world.GetTile (GridIndexX, GridIndexZ + 1)); //up
		temp.Add (world.GetTile (GridIndexX - 1, GridIndexZ - 1)); //left down
		temp.Add (world.GetTile (GridIndexX - 1, GridIndexZ + 1)); //left up
		temp.Add (world.GetTile (GridIndexX + 1, GridIndexZ - 1)); //right down
		temp.Add (world.GetTile (GridIndexX + 1, GridIndexZ + 1)); //right up
		
		return temp;
	}

	/*
	 * Returns all neighbours
	 * */
	public HashSet<Node> GetRingNeighbours(int x)
	{
		HashSet<Node> temp = new HashSet<Node>();
		WorldGrid world = WorldManager._WORLD;
		
		temp.Add (world.GetTile (GridIndexX - 1 - x, GridIndexZ)); //left
		temp.Add (world.GetTile (GridIndexX, GridIndexZ - 1 - x)); //down
		temp.Add (world.GetTile (GridIndexX + 1 + x, GridIndexZ)); //right
		temp.Add (world.GetTile (GridIndexX, GridIndexZ + 1 + x)); //up
		temp.Add (world.GetTile (GridIndexX - 1 - x, GridIndexZ - 1 - x)); //left down
		temp.Add (world.GetTile (GridIndexX - 1 - x, GridIndexZ + 1 + x)); //left up
		temp.Add (world.GetTile (GridIndexX + 1 + x, GridIndexZ - 1 - x)); //right down
		temp.Add (world.GetTile (GridIndexX + 1 + x, GridIndexZ + 1 + x)); //right up
		
		return temp;
	}

    public Node GetRandomNeighbor()
    {
        Node[] a = new Node[neighbors.Count];
        neighbors.CopyTo(a);
        return a[WorldManager.Random.Next(a.Length)];
    }

	public HashSet<Node> GetExpandingBounds(int x)
	{
		HashSet<Node> temp = new HashSet<Node>();

		if (x == 0)
		{
			temp.Add (this);
			return temp;
		}
		WorldGrid world = WorldManager._WORLD;

		Node diagonal = world.GetTile (GridIndexX + x, GridIndexZ + x);
		temp.Add (diagonal); //right up
		for(int i = 1; i <= x; ++i)
		{
			temp.Add (world.GetTile ((int)diagonal.GridIndexX + x - i, (int)diagonal.GridIndexZ));
			temp.Add (world.GetTile ((int)diagonal.GridIndexX, (int)diagonal.GridIndexZ + x - i));
		}	
		return temp;
	}

	public override string ToString()
	{
		return "Node : (" + GridIndexX + ", " + GridIndexX + ") - Walkable: " + Walkable;
	}

	public int CompareTo(Node nodeIn) 
	{
		//Debug.Log ("Compare(" + this.ToString() + "H: " + this.hScore + "and " + nodeIn.ToString() + "H: " + nodeIn.hScore );
		if (nodeIn == null) return 1;

		int contrast = this.fScore.CompareTo(nodeIn.F);
		//Debug.Log ("Contrast: " + contrast);
		if(contrast == 0)
		{
			//Debug.Log (this.hScore.CompareTo(nodeIn.H));
			return this.hScore.CompareTo(nodeIn.H);
		}
		return contrast;
	}
}
