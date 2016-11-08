using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

public class WaveManager : MonoBehaviour
{
    public GameObject[] baddys; //Array of all Enemy Objects, Index Number is Object Spawn ID
    public static GameObject[] baddysGet;
    public Transform[] spawnLocations;

    public TowerController[] TurretTargets;
    public List<TowerController> TurretTarget_List;

    public GameObject OrkGroup;

    private static int _wavenumber = 0;
    public static int WaveNumber
    {
        get
        {
            return _wavenumber;
        }
        set
        {
            _wavenumber = value;
        }
    }
    public static int enemyWaveLimit;
    public static int enemyAliveCouont;
    public static int enemyKilledCount;

    private Wave currentWave;

    private Queue<Wave> waves;
    public void AddWave(Wave waveIn)
    {
        waves.Enqueue(waveIn);
    }

    void Awake()
    {
        var x = SARSiteUI._Instance;
    }

    void Start()
    {
        InitStart();
    }

    void InitStart()
    {
        TurretTarget_List = new List<TowerController>(TurretTargets);
        baddysGet = baddys;

        LoadWaveData();
        //waves.Dequeue();
        //waves.Dequeue();
        ActivateNextWave();


        /*print("||__||Wave Queue Diagnostics||__||\n///////\n///////");
        foreach (Wave w in waves)
        {
            print("Wave[" + w.ID + "]");
            int chunkCount = 1;
            foreach (WaveChunk c in w.waveChunks)
            {
                print("Chunk[" + chunkCount + "]");
                print("\tChunk Order Count: " + c.chunkOrders.Count);
                chunkCount++;
                print("END CHUNK");
            }
            print("END WAVE");
        }*/

        /*WeaponPlatform wep = WeaponManager._Instance.currentWeapon;
        for(int x = 0; x < 15; ++x)
        {
            wep.AddAmmo_BATCH();
        }*/
    }

    private void LoadWaveData()
    {
        waves = new Queue<Wave>();
        Wave tempWave = null;
        WaveChunk tempChunk = null;
        WaveOrder tempOrder = null;

        /*StreamReader reader = new StreamReader("Assets/SAR/Code/Baddys/WaveManager/WaveData.txt");

        string s = "";

        do
        {
            s = reader.ReadLine().Trim();
            //print(s);
            continue;
        } while (s.Length >= 2 && s.Substring(0, 2).Equals("//"));

        if (s.Length <= 0)
            return;*/

        tempWave = new Wave(this);
        tempChunk = new WaveChunk(tempWave);
        tempOrder = new WaveOrder(tempChunk, 0, 1, 0, 0, 0, 0);

        waves.Enqueue(tempWave);

        tempWave = new Wave(this);
        tempChunk = new WaveChunk(tempWave);
        tempOrder = new WaveOrder(tempChunk, 0, 1, 0, 0, 0, 0);
        tempOrder = new WaveOrder(tempChunk, 0, 1, 3, 3, 0, 1);

        waves.Enqueue(tempWave);

        //for(int w = 0; w < 10; ++w)
        //{
        //    tempWave = new Wave(this);
        //    for (int c = 0; c < Random.Range(1, w / 4 + 1); ++c)
        //    {
        //        tempChunk = new WaveChunk(tempWave);
        //        float time = 0;
        //        for (int o = 0; o < Random.Range(1, 3); ++o)
        //        {
        //            int id = Random.Range(0, Mathf.Clamp(o + Random.Range(1, w) - 3, 0, baddys.Length));
        //            int count = Mathf.Clamp(Random.Range(2, 1 + w), w + 1, (w + 2) / (o + 1));
        //            float freq = Mathf.Clamp(2 + w, 0, 2) + Random.Range(.15f, .5f);
        //            tempOrder = new WaveOrder(tempChunk, 1, count, freq, time, 0);
        //            time += Random.Range(.15f, o * Random.Range(.5f, 1.15f));
        //        }
        //    }
        //    waves.Enqueue(tempWave);
        //}
    }

    public void ActivateNextWave()
    {
        var lastWave = currentWave;
        if (waves.Count > 0)
        {
            WaveNumber++;
            //SARSiteUI._Instance.waveString = "Wave: " + waveNumber;
            currentWave = waves.Dequeue();
            currentWave.ActivateNextChunk();
        }
        else
        {
            WaveNumber++;
            currentWave = new Wave(this);
            WaveChunk tempChunk = new WaveChunk(currentWave);
            WaveOrder tempOrder;

            int orders = WaveNumber + 1;
            UnityEngine.Debug.Log(orders);
            for (int x = 0; x < orders; ++x)
            {
                int spawnLocID = (int)Mathf.Clamp(Random.Range(0, orders) - Random.Range(0, 2), 0, 2);
                tempOrder = new WaveOrder(tempChunk, 0, 1, x * orders / 2f, orders * 2f, 1f, 0);
            }

            //int orders = (int)(Mathf.Clamp((waveNumber - Random.Range(1, 3)) * waveNumber / 5f, 1, 10));
            //for (int x = 0; x < orders; ++x)
            //{
            //    int spawnLocID = (int)Mathf.Clamp(Random.Range(0, orders) - Random.Range(0, 2), 0, 2);
            //    tempOrder = new WaveOrder(tempChunk, 0, Mathf.Clamp((orders / 2) - 2, 1 , 10), x * orders / 2f, orders * 2f, 1f, 0);
            //}

            currentWave.ActivateNextChunk();
        }
        SARSiteUI._Instance.s_waveCount = "Wave: " + WaveNumber;
    }

    public void SpawnBaddy(int targetIDIn, WaveOrder orderIn)
    {
        var position = spawnLocations[orderIn.SpawnLocation_ID].position;
        GameObject g = Instantiate(baddys[targetIDIn], position, Quaternion.identity) as GameObject;
        g.transform.parent = GameObject.Find("EntityHierarchy").transform;

        SquadController group = g.GetComponent<SquadController>();

        Timer.Register(1f, () =>
                    {
                        foreach (GroupUnitController unit in group.C_SquadControllers_HashSet)
                        {
                            UnityEngine.Debug.Log("en " + ((TaggableEntity)unit.C_Entity));
                            new BaddyDestroyTag(orderIn.ParentChunk, ((TaggableEntity)unit.C_Entity));
                        }

                        float chance = Random.Range(0f, 1f);

                        if (TurretTarget_List.Count == 0)
                        {
                            chance = Mathf.Clamp(chance, 0, .59f);
                        }

                        I_Entity en = null;

                        if (chance < .4f)
                        {
                            if (EntityManager.GetEntityByUniqueID(ObjectiveStructure._OBJECTIVE_ENTITY_ID, ref en))
                            {
                                group.AddOrder(new Order_SquadSearchAndDestroy(group, true, ((TaggableEntity)en)));
                                UnityEngine.Debug.Log("TARGETTING OBJECTIVE");
                            }
                        }
                        else if (chance < .6f)
                        {
                            if (EntityManager.GetEntityByUniqueID(PlayerEntity.PlayerID, ref en))
                            {
                                group.AddOrder(new Order_SquadSearchAndDestroy(group, true, ((TaggableEntity)en)));
                                UnityEngine.Debug.Log("TARGETTING PLAYER");
                            }
                        }
                        else
                        {
                            int index = Random.Range(0, 2) * Random.Range(0, TurretTarget_List.Count);
                            if (index > TurretTarget_List.Count)
                                index = 0;

                            var targetTower = TurretTarget_List[index];
                            if (targetTower != null)
                            {
                                while (index != -1)
                                {
                                    if (targetTower.C_Entity.Dead)
                                    {
                                        index++;
                                        if (index >= TurretTarget_List.Count)
                                        {
                                            index = -1;
                                        }
                                    }
                                    else
                                    {
                                        Node targetNode = WorldManager._WORLD.GetTile(targetTower.C_Entity.Pos);
                                        if (!targetNode.Walkable)
                                            targetNode = targetNode.NearestWalkableNode(0);
                                        if (targetNode != null)
                                        {
                                            if (targetNode.Walkable)
                                            {
                                                group.AddOrder(new Order_SqaudMovement(group, true, targetNode));
                                                index = -1;

                                                UnityEngine.Debug.Log("MOVING TO KILL TOWER: " + targetTower.C_AttachedGameObject.name);
                                            }
                                        }
                                        else
                                        {
                                            index++;
                                            if (index >= TurretTarget_List.Count)
                                            {
                                                index = -1;
                                            }
                                        }
                                    }
                                }
                            }
                            if (index == -1)
                            {
                                if (EntityManager.GetEntityByUniqueID(PlayerEntity.PlayerID, ref en))
                                    group.AddOrder(new Order_SquadSearchAndDestroy(group, true, ((TaggableEntity)en)));
                                UnityEngine.Debug.Log("TARGETTING PLAYER, failed all other tests");
                            }
                        }
                    }, null, false, false, group);      
    }

    public static void RestartWaves()
    {
        Timer.CancelAllRegisteredTimers();
    }
}

//-------------------------------------------//

public class Wave
{
    public int EnemyCount { get; set; }
    public Queue<WaveChunk> waveChunks { get; protected set; }
    public int Chunks
    {
        get
        {
            return waveChunks.Count;
        }
    }
    public int StartingChunks
    {
        get; protected set;
    }
    public WaveManager ManagerInstance { get; private set; }

    public Wave(WaveManager managerInstance)
    {
        ManagerInstance = managerInstance;
        waveChunks = new Queue<WaveChunk>();

        managerInstance.AddWave(this);

        StartingChunks = Chunks;
    }

    public void AddWaveChunk(WaveChunk chunkIn)
    {
        waveChunks.Enqueue(chunkIn);
    }

    public void ActivateNextChunk()
    {
        SARSiteUI._Instance.s_enemyCount = "Enemies Left: " + EnemyCount;
        if (waveChunks.Count > 0)
        {
            WaveChunk tempChunk = waveChunks.Dequeue();
            tempChunk.StartChunk();
        }
        else
        {
            OnWaveEnd();
            ManagerInstance.ActivateNextWave();
        }
    }

    public void OnWaveEnd()
    {
        //WeaponManager._Instance.currentWeapon.AddAmmo_BATCH();
    }
}

public class WaveChunk
{
    public Wave ParentWave { get; private set; }

    public List<WaveOrder> chunkOrders { get; protected set; }
    public int TargetsLeft { get; protected set; } //total baddies left to be destroyed before this chunk is over

    public WaveChunk(Wave parentWaveIn)
    {
        ParentWave = parentWaveIn;
        chunkOrders = new List<WaveOrder>();

        ParentWave.AddWaveChunk(this);
    }

    public void AddOrder(WaveOrder order)
    {
        chunkOrders.Add(order);
        TargetsLeft += order.TargetCount;
        ParentWave.EnemyCount += TargetsLeft;
    }

    public void AddOrder(WaveOrder order, int targetsInOrder)
    {
        chunkOrders.Add(order);
        TargetsLeft += targetsInOrder;
        ParentWave.EnemyCount += TargetsLeft;
    }

    public void StartChunk()
    {
        if (chunkOrders.Count <= 0)
        {
            System.Console.Write(": error : Chunk orders empty - Chunk not setup");
            return;
        }
        foreach(WaveOrder order in chunkOrders)
        {
            order.RegisterOrder();
        }
    }

    private bool quit = false;
    public void TargetDestroyed()
    {
        TargetsLeft -= 1;
        SARSiteUI._Instance.s_enemyCount = "Enemies Left: " + TargetsLeft;
        if (TargetsLeft <= 0 && !quit)
        {
            quit = true;
            ParentWave.ActivateNextChunk();
        }
    }
}

public class WaveOrder
{
    public WaveChunk ParentChunk { get; protected set; }

    public float ExecutionTime { get; protected set; } //in seconds, using timers
    public float Frequency { get; protected set; } //how often the enemy is spawned once the order is executed, format: every x seconds
    public float FrequencyMargin { get; protected set; }

    public int SpawnID { get; protected set; } //ID of the enemy to be spawned
    public int TargetCount { get; protected set; } //number of enemies to spawn
    public int SpawnedCount { get; protected set; } //number of enemies to spawn

    public int SpawnLocation_ID { get; protected set; } //Index of the spawn location

    public WaveOrder(WaveChunk chunkIn, int idIn, int countIn, float timeIn, float freqIn, float marginIn, int spawnLocationID)
    {
        ParentChunk = chunkIn;

        SpawnID = idIn;
        TargetCount = countIn;
        SpawnedCount = 0;

        ExecutionTime = timeIn;
        Frequency = freqIn;
        FrequencyMargin = marginIn;

        ParentChunk.AddOrder(this, 5 * TargetCount);

        SpawnLocation_ID = spawnLocationID;
    }

    public void RegisterOrder()
    {
        Timer.Register(ExecutionTime, () => SpawnBaddys(), null, false, false, ParentChunk.ParentWave.ManagerInstance);
    }

    public void SpawnBaddys()
    {
        SpawnedCount++;
        ParentChunk.ParentWave.ManagerInstance.SpawnBaddy(SpawnID, this);
        
        if (SpawnedCount < TargetCount)
        {
            Timer.Register(Mathf.Clamp(Frequency + Random.Range(-FrequencyMargin, FrequencyMargin), 0, 1000f), () => SpawnBaddys(), null, false, false, ParentChunk.ParentWave.ManagerInstance);
        }
    }
}

public class BaddyDestroyTag : I_EntityTag
{
    private WaveChunk parentChunk;

    public BaddyDestroyTag(WaveChunk parentChunkIn, TaggableEntity entityIn)
    {
        parentChunk = parentChunkIn;
        entityIn.TagEntity(this);
    }

    public void TagAction()
    {
        parentChunk.TargetDestroyed();
        UnityEngine.Debug.Log("kill");
    }
}