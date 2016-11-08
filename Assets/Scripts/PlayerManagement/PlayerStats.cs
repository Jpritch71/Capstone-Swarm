using UnityEngine;
using System.Collections;

public class PlayerStats
{
    public PlayerEntity _PLAYER { get; set; }
    public PlayerInfo _PlayerInfo { get; set; }

    private static PlayerStats _Instance;
    public static PlayerStats PlayerStatContainer
    {
        get
        {
            if(_Instance == null)
            {
                _Instance = new PlayerStats();
            }
            return _Instance;
        }
    }
    private PlayerStats()
    {
        //CreateNewPlayer("Regum");
    }

    public int PlayerLevel
    {
        get
        {
            return _PLAYER.EntityLevel;
        }
    }

    public static bool TryLoadPlayer(ref PlayerEntityController controllerIn, ref I_Entity entityIn)
    {
        if(PlayerStatContainer._PlayerInfo == null)
        {
            return false;
        }
        entityIn = new PlayerEntity(controllerIn, 1000f, PlayerStats._Instance._PlayerInfo.Level);
        return true;
    }
}

public class PlayerInfo
{
    public long Id { get; set; }
    public string UserName { get; protected set; }
    public int HighScore { get; protected set; }
    public int Level { get; protected set; }

    public PlayerInfo(string nameIn, int scoreIn, int levelIn)
    {
        UserName = nameIn;
        HighScore = scoreIn;
        Level = levelIn;
    }
}
