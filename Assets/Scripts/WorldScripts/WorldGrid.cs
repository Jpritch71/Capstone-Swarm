using UnityEngine;
using System.Collections;

public class WorldGrid : MonoBehaviour 
{
	private Terrain worldTerrain;

	private float tileSize; //size of the tile representing each node
	private Vector2 gridDimension;
    private int X_DIM, Y_DIM; //Grid dimensions, ensure that this is a multiple of 2
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
        X_DIM = (int)(Mathf.Ceil(Mathf.Abs(pos.x - boundaryMarker.transform.position.x)) / tileSize);
        Y_DIM = (int)(Mathf.Ceil(Mathf.Abs(pos.z - boundaryMarker.transform.position.z)) / tileSize);
        tiles = new Node[X_DIM, Y_DIM];
		//if(worldTerrain != null)
			//worldTerrain.terrainData.size = new Vector3 (tileSize * gridDimension.x, 10f, tileSize * gridDimension.y);


		GameObject g;
		float corner1 = 0, corner2 = 0, corner3 = 0, corner4 = 0;
		for(int x = 0; x < this.X_DIM; x++)
		{
			for(int z = 0; z < this.Y_DIM; z++)
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
		for(int x = 0; x < X_DIM; x++)
		{
			for(int z = 0; z < Y_DIM; z++)
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
        //print(WorldNodeCount());
    }

    void Update()
    {        
        //Debug.DrawLine(transform.position + Vector3.up * 3, boundaryMarker.transform.position - new Vector3(0, 0, boundaryMarker.transform.position.z) + Vector3.up * 3);
        //Debug.DrawLine(transform.position + Vector3.up * 3, boundaryMarker.transform.position - new Vector3(boundaryMarker.transform.position.x, 0, 0) + Vector3.up * 3);

        //Debug.DrawLine(transform.position + (boundaryMarker.transform.position - new Vector3(0, 0, boundaryMarker.transform.position.z)) + Vector3.up * 3f, boundaryMarker.transform.position + Vector3.up * 3);
        //Debug.DrawLine(transform.position + (boundaryMarker.transform.position - new Vector3(boundaryMarker.transform.position.x, 0, 0)) + Vector3.up * 3f, boundaryMarker.transform.position + Vector3.up * 3);

        //if (false)
        //{
        //    for (int x = 0; x < X_DIM; x++)
        //    {
        //        for (int z = 0; z < Y_DIM; z++)
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

    public int WorldNodeCount()
    {
        return (X_DIM * Y_DIM);
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

    public Node GetTile(int x, int z)
    {
        try
        {
            return tiles[x, z];
        }
        catch (System.IndexOutOfRangeException e)
        {
            if (x < 0)
                x = 0;
            else if (x >= X_DIM)
                x = X_DIM - 1;

            if (z < 0)
                z = 0;
            else if (z >= Y_DIM)
                z = Y_DIM - 1;
            return tiles[x, z];
        }
    }

    public Node GetTile(float x, float z)
	{
        return GetTile (WorldXToTileX(x), WorldZToTileZ(z));
	}

	public int WorldXToTileX(float xCoordIn)
	{
		return Mathf.Clamp((int)(Mathf.Abs(-pos.x + xCoordIn) / (tileSize)), 0, X_DIM - 1);
	}

	public int WorldZToTileZ(float zCoordIn)
	{
		return Mathf.Clamp((int)(Mathf.Abs(-pos.z + zCoordIn) / (tileSize)), 0, Y_DIM - 1);
	}
}
