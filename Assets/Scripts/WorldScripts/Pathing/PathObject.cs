using UnityEngine;
using System.Collections.Generic;

public class PathObject
{
    public bool pathFound { get; set; }
    public bool pathFailed { get; set; }
    public List<Node> path;
    public Node startNode { get; set; }
    public Node targetNode { get; set; }
    public int clearance { get; set; }
    private PriorityQueue<SearchNode, int> openSet;

    public E_GridedMovement gridEntity { get; set; }
    //public float timeCard { get; set; }

    public PathObject()
    {
        pathFailed = false;
    }

    public void setPath(List<Node> pathIn)
    {
        if (pathIn == null)
            pathFound = false;
        else
        {
            pathFound = true;
        }
        path = pathIn;          
    }

    public List<Node> getPath()
    {
        return path;
    }

    public PriorityQueue<SearchNode, int> OpenSet
    {
        get
        {
            if(openSet == null)
            {
                openSet = new PriorityQueue<SearchNode, int>(100000);
            }
            return openSet;
        }
    }

    public void SetRecentNode()
    {
        startNode = WorldManager._WORLD.GetTile(gridEntity.transform.position);
    }

    public void TryMostRecentNode()
    {
        try
        {
            startNode = gridEntity.getNodeInPath();
        }
        catch(System.ArgumentOutOfRangeException e)
        {
            return;
        }
    }
}
