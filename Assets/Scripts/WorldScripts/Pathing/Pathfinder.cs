using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Diagnostics;

public class Pathfinder : MonoBehaviour
{
    private static Pathfinder pathInstance; //singleton instance of Pathfinder
    private static Queue<A_GridMover> pathingQueue; //First in First out Grid Movers to be serviced
    private static HashSet<A_GridMover> queueMembers; //Membership set, do not allow Grid movers in twice

    private static Thread pathingThread, workerThread;
    //private static List<Thread> threads;

    private static bool pathingThreadRunning = false;
    private static int threadsRunning = 0;


    void Awake()
    {
        pathingQueue = new Queue<A_GridMover>();
        queueMembers = new HashSet<A_GridMover>();
        //threads = new List<Thread>();
    }

    /*
     * Singleton structure
     * */
    public static Pathfinder _instance
	{
		get
		{
			if(pathInstance == null)
			{
                //PATHFINDER = ((GameObject)(Instantiate(Resources.Load("pather"), Vector3.zero, Quaternion.identity))).GetComponent<Pathfinder>(); //load pathfinder object -- needed for monobehaviour
                var g = new GameObject("PATHFINDER");
                g.AddComponent<Pathfinder>();
                pathInstance = g.GetComponent<Pathfinder>();
			}
			return pathInstance;
		}	
	}

    /// <summary>
    /// Grid Mover requests a path from Pathfinder, if it has already requested one, the method returns fals.
    /// Otherwise, the Grid Mover is added to the Queue
    /// If the pathfinder is not currently running, the thread is started.
    /// Set the most recent node now in case it can't be retrieved in the thread (FIX THIS)
    /// </summary>
    /// <param name="character">reference to the Grid Mover requesting a path</param>
    /// 
    /// /// <returns>bool</returns>
    public bool GetPathAStar(ref A_GridMover character) 
    {
        if (!queueMembers.Contains(character))
        {
            pathingQueue.Enqueue(character);
            queueMembers.Add(character);
            if (!pathingThreadRunning)
            {
                //running = true;
                //StartCoroutine(Dequeue());   
                RunPathingThread();
            }
            //character.PathObj.timeCard = timeCard.ElapsedMilliseconds;
            character.PathObj.SetRecentNode();
            return true;
        }
        character.PathObj.setPath(null);
        UnityEngine.Debug.Log("ALREDY CONTAINED");
        return false;
    }

    private void RunPathingThread()
    {
        if (pathingThread != null && pathingThread.IsAlive)
            return;
        pathingThread = new Thread(() =>
        {
            pathingThreadRunning = true; 
            while (pathingThreadRunning)
            {             
                if (pathingQueue.Count > 0)
                {
                    DoAStar();
                }
                else
                    pathingThreadRunning = false;
            }    
        });
        pathingThread.Start();
    }

    private static PriorityQueue<SearchNode, int> open;
    private void DoAStar()
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();
        A_GridMover c = null;
        lock (pathingQueue)
        {
            c = pathingQueue.Dequeue();
        }
        lock (queueMembers)
        {
            queueMembers.Remove(c);
        }
        c.PathObj.TryMostRecentNode();
        //UnityEngine.Debug.Log(getNodesElapsed(c));

        HashSet<Node> closed = new HashSet<Node>();
        SearchNode current, searchStart, temp;
        
        if(open == null)
            open = new PriorityQueue<SearchNode, int>(WorldManager._WORLD.WorldNodeCount());
        else
            open.ResetQueue();
        searchStart = c.PathObj.startNode.SearchableNode;

        if (!open.InsertElement(searchStart))
        {
            c.PathObj.setPath(null);
            UnityEngine.Debug.Log("failed to insert element -- outer");
            return;
        }
        int count = 0;
        while (!open.IsEmpty())
        {
            ++count;
            current = open.RemoveTop();
            closed.Add(current.PairedNode);
            if (current.PairedNode == c.PathObj.targetNode)
            {
                c.PathObj.setPath(RetracePath(searchStart, current));
                c.PathObj.AlgorithmTime = timer.ElapsedMilliseconds;
                //UnityEngine.Debug.Log("path found");
                closed = null;
                current = null;                
                return;
            }

            foreach (Node n in current.PairedNode.Neighbors)
            {
                if (closed.Contains(n) || !n.Walkable || n.Clearance < c.PathObj.clearance)
                    continue;
                temp = n.SearchableNode;

                int distance = current.G + GetMoveCost(n, current.PairedNode);
                if (!open.IsMember(temp) || temp.G > distance)
                {
                    temp.H = GetMoveCost(current.PairedNode, c.PathObj.targetNode);
                    temp.G = distance;
                    temp.SetF();
                    temp.ParentNode = current;

                    if (!open.IsMember(temp))
                        if (!open.InsertElement(temp))
                        {
                            c.PathObj.setPath(null);
                            c.PathObj.pathFailed = true;
                            return;
                        }
                }
            }
        }
        if (!c.PathObj.pathFound)
        {
            UnityEngine.Debug.Log("PATH NEVER FOUND, count: " + count);
            c.PathObj.pathFailed = true;
            c.PathObj.setPath(null);
        }
    }

    private static int GetMoveCost(Node a, Node b)
    {
        int dX = Mathf.Abs(a.GridIndexX - b.GridIndexX);
        int dY = Mathf.Abs(a.GridIndexZ - b.GridIndexZ);

        if (dX > dY)
            return 14 * dY + 10 * (dX - dY);
        if (dX < dY)
            return 14 * dX + 10 * (dY - dX);
        else
            return 14 * dX + 14 * (dY - dX);
    }

    public static List<Node> RetracePath(SearchNode startPos, SearchNode endPos)
    {
        List<Node> path = new List<Node>();
        while (endPos != startPos)
        {
            endPos.PairedNode.flag = Color.blue;
            path.Add(endPos.PairedNode);
            endPos = endPos.ParentNode;
        }
        path.Reverse();
        return path;
    }

    void OnApplicationQuit()
    {
        pathingThreadRunning = false;
        UnityEngine.Debug.Log("quit");
        pathingThread.Abort();
        //foreach (Thread t in threads)
           // t.Abort();
    }
}
