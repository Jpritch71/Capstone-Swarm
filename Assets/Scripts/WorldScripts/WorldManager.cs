using UnityEngine;
using System.Collections;

public class WorldManager : ScriptableObject
{
    private static WorldGrid WORLD;
    private static System.Random random;

    public static int mapFlag = (1 << 8) | (1 << 9) | (1 << 13);
    public static int obstacleFlag = (1 << 13);
    public static int entityFlag = (1 << 11) | (1 << 12);
    public static int entityUtilityFlag = (1 << 10);

    public static System.Random Random
    {
        get
        {
            if (random == null)
            {
                random = new System.Random();
            }
            return random;
        }
    }

    public static WorldGrid _WORLD
    {
        get
        {
            if (WORLD == null)
            {
                WORLD = GameObject.Find("PlayableArea").GetComponent<WorldGrid>();
            }
            return WORLD;
        }
    }
}

public enum Flags
{
    WorldObjects = (1 << 8) | (1 << 9) | (1 << 13),
    Obstacles = (1 << 13),
    Entities = (1 << 11) | (1 << 12),
    Enemy = (1 << 12),
    Player = (1 << 11),
    EntityUtilities = (1 << 10),
    Projectile = (1 << 15)
}
