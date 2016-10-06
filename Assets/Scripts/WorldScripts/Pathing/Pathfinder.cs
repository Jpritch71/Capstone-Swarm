using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Diagnostics;

public class Pathfinder : MonoBehaviour
{
	//private static WorldGrid workingGrid;
    private static Pathfinder pathInstance;
    private static Queue<E_GridedMovement> pathingQueue;
    private static HashSet<E_GridedMovement> queueMembers;

    private static Thread pathingThread;
    private static List<Thread> threads;

    private static bool pathingThreadRunning = false;
    private static int threadsRunning = 0;

    //private static Stopwatch timeCard;

    void Awake()
    {
        //PATHFINDER = this;
        pathingQueue = new Queue<E_GridedMovement>();
        queueMembers = new HashSet<E_GridedMovement>();
        threads = new List<Thread>();
        //timeCard = new Stopwatch();
        //timeCard.Start();
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

    public bool GetPathAStar(ref E_GridedMovement character)
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

	private static int GetMoveCost(Node a, Node b)
	{
		int dX = Mathf.Abs(a.GridIndexX - b.GridIndexX);
		int dY = Mathf.Abs(a.GridIndexZ - b.GridIndexZ);
		
		if (dX > dY)
			return 14 * dY + 10 * (dX - dY);
		if(dX < dY)
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


    ////TURNS OUT I PROBABLY DON'T NEED THIS SHIT OOPS
    //public static int getNodesElapsed(E_GridedMovement c)
    //{
    //    //UnityEngine.Debug.Log("Start: " + (c.PathObj.timeCard / 1000f) + " || CurrentTime: " + (timeCard.ElapsedMilliseconds / 1000f) + " || Elapsed: " + ((timeCard.ElapsedMilliseconds - c.PathObj.timeCard)  / 1000f));
    //    return (int)((c.Speed * (timeCard.ElapsedMilliseconds - c.PathObj.timeCard) / 1000f) / WorldManager._WORLD.TileSize);
    //}

    //public float getTimeCard()
    //{
    //    return timeCard.ElapsedMilliseconds;
    //}

    private void RunPathingThread()
    {
        if (pathingThread != null && pathingThread.IsAlive)
            return;
        pathingThread = new Thread(() =>
        {
            pathingThreadRunning = true; 
            while (pathingThreadRunning)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                while (timer.ElapsedMilliseconds < 150)
                {
                    if (pathingQueue.Count > 0)
                    {
                        E_GridedMovement c = null;
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

                        Thread thread = new Thread(() =>
                        {
                            HashSet<Node> closed = new HashSet<Node>();
                            SearchNode current, searchStart, temp;

                            PriorityQueue<SearchNode, int> open = c.PathObj.OpenSet;
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
                                    //UnityEngine.Debug.Log("path found");
                                    closed = null;
                                    current = null;
                                    open.ResetQueue();
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
                            return;
                        });
                        thread.Start();
                        thread.Join();
                    }
                }
                timer.Start();
            }    
        });
        threads.Add(pathingThread);
        pathingThread.Start();
    }

    /*private IEnumerator Dequeue()
    {
        while (running)
        {
            yield return new WaitForEndOfFrame();
            if (pathingQueue.Count <= 0)
            {
                running = false;
                break;
            }   
            int count = 0;

            //UnityEngine.Debug.Log(pathingQueue.Count);
            while (pathingQueue.Count > 0)
            {
                UnityEngine.Debug.Log(count);
                if (count > 1)
                {
                    UnityEngine.Debug.Log("too many paths this frame -- pause |||||||||||||||||||||||||||||||||||||||||||||||");
                    yield return new WaitForEndOfFrame();
                    break;
                }
                ++count; //increase the count of paths worked on this frame ||||| try changing this to time based work

                HashSet<Node> closed = new HashSet<Node>();
                SearchNode current, searchStart, temp;

                CharacterMovement c = pathingQueue.Dequeue();
                PriorityQueue<SearchNode, int> open = c.PathObj.OpenSet;
                c.PathObj.SetRecentNode();
                searchStart = c.PathObj.startNode.SearchableNode;

                if (!open.InsertElement(searchStart))
                {
                    c.PathObj.setPath(null);
                    UnityEngine.Debug.Log("failed to insert element -- outer");
                    queueMembers.Remove(c);
                    continue;
                }
                count = 0;
                while (!open.IsEmpty())
                {
                    ++count;
                    current = open.RemoveTop();
                    closed.Add(current.PairedNode);
                    if (current.PairedNode == c.PathObj.targetNode)
                    {
                        c.PathObj.setPath(RetracePath(searchStart, current));
                        //UnityEngine.Debug.Log("path found");
                        closed = null;
                        current = null;
                        open.ResetQueue();
                        queueMembers.Remove(c);
                        break;
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
                                    queueMembers.Remove(c);
                                    yield break;
                                }
                        }
                    }
                }
                if (!c.PathObj.pathFound)
                {
                    UnityEngine.Debug.Log("PATH NEVER FOUND, count: " + count);
                    queueMembers.Remove(c);
                    c.PathObj.setPath(null);
                }
            }
        }
    }*/
    
    void OnApplicationQuit()
    {
        pathingThreadRunning = false;
        UnityEngine.Debug.Log("quit");
        pathingThread.Abort();
        foreach (Thread t in threads)
            t.Abort();
    }
}
