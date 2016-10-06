using UnityEngine;
using System.Collections;

public class WorldGrid : MonoBehaviour 
{
	private Terrain worldTerrain;

	private float tileSize; //size of the tile representing each node
	private Vector2 gridDimension; //Grid dimensions, ensure that this is a multiple of 2
	private Node[,] tiles; //list of nodes/tiles 
    private GameObject boundaryMarker;
    private Vector3 pos;

	public GameObject tile, voidTile;

	public static bool debugMode = false;

    public GameObject a;
    public static GameObject attack;

    protected RaycastHit[] hits; //variable for storing temporary RaycastHit array
    protected RaycastHit hit;

    public static int mapFlag = (1 << 8) + (1 << 9) + (1 << 13);

    // Use this for initialization
    void Awake() 
	{
        pos = transform.position;
        attack = a;
		worldTerrain = GetComponent<Terrain> ();

        boundaryMarker = transform.Find("BoundaryMarker").gameObject;
		tileSize = 1f; 
		gridDimension = new Vector2 (Mathf.Ceil(Mathf.Abs(pos.x - boundaryMarker.transform.position.x)) / tileSize,
                                     Mathf.Ceil(Mathf.Abs(pos.z - boundaryMarker.transform.position.z)) / tileSize);
		tiles = new Node[(int)gridDimension [0], (int)gridDimension [1]];
		//if(worldTerrain != null)
			//worldTerrain.terrainData.size = new Vector3 (tileSize * gridDimension.x, 10f, tileSize * gridDimension.y);


		GameObject g;
		float corner1 = 0, corner2 = 0, corner3 = 0, corner4 = 0;
		for(int x = 0; x < this.gridDimension[0]; x++)
		{
			for(int z = 0; z < this.gridDimension[1]; z++)
			{
				//----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				//node Center ----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				//----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				hits = Physics.RaycastAll(new Vector3(tileSize * x + (tileSize / 2) + pos.x, 100f + pos.y, tileSize * z + (tileSize / 2) + pos.z),
				                          Vector3.down, 150f, mapFlag);
				if(hits.Length > 0)
				{
					hit = hits[0];
					for(int k = 1; k < hits.Length; k++)
					{
						if(hit.point.y < hits[k].point.y)
						{
							hit = hits[k];
						}
					}
					tiles[x, z] = new Node(new Vector3(tileSize * x + (tileSize / 2) + pos.x, hit.point.y, tileSize * z + (tileSize / 2) + pos.z),
					                       WorldXToTileX(hit.point.x), WorldZToTileZ(hit.point.z));
					if(hit.transform.gameObject.layer == 9 || hit.transform.gameObject.layer == 13)
                        tiles[x, z].Walkable = false;
                    tiles[x, z].flag = Color.white;
                }
				else 
				{
					tiles[x, z] = new Node(new Vector3(tileSize * x + (tileSize / 2) + pos.x, -69f, tileSize * z + (tileSize / 2) + pos.z),
					                       WorldXToTileX(hit.point.x),
					                       WorldZToTileZ(hit.point.z));
					tiles[x, z].Walkable = false;
                    tiles[x, z].flag = Color.red;
                }
				//----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				//corner1 -- bottom left ----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				hits = Physics.RaycastAll(new Vector3(tileSize * x + pos.x, 100f + pos.y, tileSize * z + pos.z),
				                          Vector3.down, 150f, mapFlag);
				
				if(hits.Length > 0)
				{
					corner1 = hits[0].point.y;
					for(int k = 1; k < hits.Length; k++)
					{
						if(corner1 < hits[k].point.y)
						{
							corner1 = hits[k].point.y;
						}
					}
				}
				//----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				//corner2 -- top left ----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				hits = Physics.RaycastAll(new Vector3(tileSize * x + pos.x, 100f + pos.y, tileSize * z + (tileSize) + pos.z),
				                          Vector3.down, 150f, mapFlag);
				
				if(hits.Length > 0)
				{
					corner2 = hits[0].point.y;
					for(int k = 1; k < hits.Length; k++)
					{
						if(corner2 < hits[k].point.y)
						{
							corner2 = hits[k].point.y;
						}
					}
				}
				//----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				//corner3 -- bottom right ----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				hits = Physics.RaycastAll(new Vector3(tileSize * x + (tileSize) + pos.x, 100f + pos.y, tileSize * z + pos.z),
				                          Vector3.down, 150f, mapFlag);
				
				if(hits.Length > 0)
				{
					corner3 = hits[0].point.y;
					for(int k = 1; k < hits.Length; k++)
					{
						if(corner3 < hits[k].point.y)
						{
							corner3 = hits[k].point.y;
						}
					}
				}
				//----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				//corner4 -- top right ----=======--------=======--------=======--------=======--------=======--------=======--------=======--------=======----
				hits = Physics.RaycastAll(new Vector3(tileSize * x + (tileSize) + pos.x, 100f + pos.y, tileSize * z + (tileSize) + pos.z),
				                          Vector3.down, 150f, mapFlag);
				
				if(hits.Length > 0)
				{
					corner4 = hits[0].point.y;
					for(int k = 1; k < hits.Length; k++)
					{
						if(corner4 < hits[k].point.y)
						{
							corner4 = hits[k].point.y;
						}
					}
				}

                if (((corner1 + corner2 + corner3 + corner4) / 4f) - hit.point.y > Node.maxHop / 2)
                {
                    tiles[x, z].Walkable = false;
                    //print(tiles[x, z]);
                }
			}
		}
		//places tiles over top of the node for display - remove after tests.
		//redo node evaluation
		//Material blackMaterial = new Material(tile.transform.Find("Cube").GetComponent<Renderer>().sharedMaterial);

		//blackMaterial.color = Color.black;
		for(int x = 0; x < gridDimension[0]; x++)
		{
			for(int z = 0; z < gridDimension[1]; z++)
			{
				tiles[x, z].EvaluateNeighbors();
				if(debugMode)
				{
					g = Instantiate (tile, new Vector3(tiles[x, z].Pos.x, tiles[x, z].Pos.y, tiles[x, z].Pos.z),
					                 Quaternion.identity) as GameObject;
					tiles[x, z].TileObject = g;
/*
					if(!tiles[x, z].Walkable)
					{
						g.transform.Find("Cube").GetComponent<Renderer>().sharedMaterial.color = Color.white;
						g.layer = 9;
						for(int i = 0; i < g.transform.childCount; ++i)
						{
							g.transform.GetChild(i).gameObject.layer = 9;
						}
					}
					else*/
						//g.transform.Find("Cube").GetComponent<Renderer>().sharedMaterial = blackMaterial;
					g.name = "Node: [" + x + "|" + z + "] Clearance: " + tiles[x, z].Clearance;
					g.transform.parent = this.transform;
				}
			}
		} 
		this.gameObject.SetActive(false);
		this.gameObject.SetActive(true);

        //Node n = null;
        //System.GC.Collect();
        //// run once outside of loop to avoid initialization costs
        //System.Diagnostics.Stopwatch sw;
        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if(Random.Range(0f, 1f) < .01f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .01f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .01f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //print("end 1%");

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .05f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .05f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .05f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //print("end 5%");

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .1f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .1f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //sw = System.Diagnostics.Stopwatch.StartNew();
        //for (int i = 0; i < 10000000; i++)
        //{
        //    if (Random.Range(0f, 1f) < .1f)
        //    {
        //        n = GetTile(50, 1000);
        //    }
        //    else
        //        n = GetTile(50, 51);
        //}
        //sw.Stop();
        //print((sw.ElapsedMilliseconds).ToString());

        //print("end 10%");
    }

    void Update()
    {        
        //Debug.DrawLine(transform.position + Vector3.up * 3, boundaryMarker.transform.position - new Vector3(0, 0, boundaryMarker.transform.position.z) + Vector3.up * 3);
        //Debug.DrawLine(transform.position + Vector3.up * 3, boundaryMarker.transform.position - new Vector3(boundaryMarker.transform.position.x, 0, 0) + Vector3.up * 3);

        //Debug.DrawLine(transform.position + (boundaryMarker.transform.position - new Vector3(0, 0, boundaryMarker.transform.position.z)) + Vector3.up * 3f, boundaryMarker.transform.position + Vector3.up * 3);
        //Debug.DrawLine(transform.position + (boundaryMarker.transform.position - new Vector3(boundaryMarker.transform.position.x, 0, 0)) + Vector3.up * 3f, boundaryMarker.transform.position + Vector3.up * 3);

        //if (false)
        //{
        //    for (int x = 0; x < gridDimension[0]; x++)
        //    {
        //        for (int z = 0; z < gridDimension[1]; z++)
        //        {
        //            Debug.DrawRay(tiles[x, z].Pos, Vector3.up, tiles[x, z].flag);
        //        }
        //    }
        //}
    }

    public Vector2 GridDimension
	{
		get
		{
			return gridDimension;
		}
	}

	public float TileSize
	{
		get
		{
			return tileSize;
		}
	}

	public Node GetTile(Vector3 pointIn)
	{
		return tiles [WorldXToTileX(pointIn.x), WorldZToTileZ(pointIn.z)];
	}

	public Node GetTile(RaycastHit hitIn)
	{
        return GetTile(hitIn.point);
	}

    //    public Node GetTile(int x, int z)
    //    {
    //#if Unity_Editor
    //            if (tiles == null)
    //    			Debug.Break ();
    //#endif
    //        if (x < 0)
    //            x = 0;
    //        if (x >= gridDimension[0])
    //            x = (int)gridDimension[0] - 1;

    //        if (z < 0)
    //            z = 0;
    //        if (z >= gridDimension[1])
    //            z = (int)gridDimension[1] - 1;
    //        return tiles[x, z];
    //    }

    public Node GetTile(int x, int z)
    {
        try
        {
            return tiles[x, z];
        }
        catch (System.IndexOutOfRangeException e)
        {
            //print("out of bounds");
            if (x < 0)
                x = 0;
            else if (x >= gridDimension[0])
                x = (int)gridDimension[0] - 1;

            if (z < 0)
                z = 0;
            else if (z >= gridDimension[1])
                z = (int)gridDimension[1] - 1;
            return tiles[x, z];
        }
    }

    public Node GetTile(float x, float z)
	{
        return GetTile (WorldXToTileX(x), WorldZToTileZ(z));
	}

	public int WorldXToTileX(float xCoordIn)
	{
		return (int)Mathf.Clamp((int)(Mathf.Abs(-pos.x + xCoordIn) / (tileSize)), 0, gridDimension[0] - 1);
	}

	public int WorldZToTileZ(float zCoordIn)
	{
		return (int)Mathf.Clamp((int)(Mathf.Abs(-pos.z + zCoordIn) / (tileSize)), 0, gridDimension[1] - 1);
	}
}
