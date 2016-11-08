using UnityEngine;
using UnityEngine.UI;

public class SARSiteUI
{
    #region GameRunning_Fields
    public Transform Screen_GameRunning { get; protected set; }
    public Transform Running_TextContainer { get; protected set; }
    private Text text_score, text_enemyCount, text_waveCount;
    private Text text_health, text_objectiveHealth;
    #endregion

    #region GameRunning_Fields
    public Transform Screen_GameOver{ get; protected set; }
    public Transform GameOver_TextContainer { get; protected set; }
    public Text text_GameOver { get; protected set; }
    public Button button_Restart { get; protected set; }
    #endregion

    private SARSiteUI()
    {
        LoadUI();
    }

    public void LoadUI()
    {
        Screen_GameRunning = GameObject.Find("Screen_GameRunning").transform;
        Running_TextContainer = Screen_GameRunning.Find("Text").transform;
        text_score = Running_TextContainer.Find("_score").GetComponent<Text>();
        text_enemyCount = Running_TextContainer.Find("_enemyCount").GetComponent<Text>();
        text_waveCount = Running_TextContainer.Find("_waveCount").GetComponent<Text>();

        text_health = Running_TextContainer.Find("HealthBackDrop").Find("_health").GetComponent<Text>();
        text_objectiveHealth = Running_TextContainer.Find("HealthBackDrop").Find("_objectiveHealth").GetComponent<Text>();

        Screen_GameOver = GameObject.Find("Screen_GameOver").transform;
        GameOver_TextContainer = Screen_GameOver.Find("Text").transform;
        text_GameOver = GameOver_TextContainer.Find("B_GameOver").GetComponentInChildren<Text>();
        s_gameOver_INITIAL = s_gameOver;

        button_Restart = Screen_GameOver.Find("RestartButton").GetComponent<Button>();
        button_Restart.onClick.AddListener(delegate { GameManager._Instance.RestartGame(); });

        Switch_GameRunning();
    }

    private static SARSiteUI instance;
    public static SARSiteUI _Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SARSiteUI();
            }
            return instance;
        }
    }

    public static void ReloadUI()
    {
        instance = new SARSiteUI();
    }

    public void Switch_GameRunning()
    {
        Screen_GameRunning.gameObject.SetActive(true);
        Screen_GameOver.gameObject.SetActive(false);
    }

    public void Switch_GameOver()
    {
        Screen_GameRunning.gameObject.SetActive(false);
        Screen_GameOver.gameObject.SetActive(true);

        var foundStats = PlayerStats.PlayerStatContainer._PlayerInfo;
        var score = Score._Instance.score;
        if (foundStats != null)
        {
            if (score > foundStats.HighScore) //highscore achieved
            {
                s_gameOver = "New High Score!\nScore: " + foundStats.HighScore;
            }
            else
            {
                s_gameOver = s_gameOver_INITIAL + "\nScore:" + score;
            }
        }
        else
        {
            s_gameOver = "New High Score!\nScore: " + score;
            //PlayerStats.PlayerStatContainer.CreateNewPlayer("New User", score);
        }
    }

    #region Strings

    #region GameRunning
    public string s_score
    {
        get
        {
            return text_score.text;
        }
        set
        {
            text_score.text = value;
        }
    }

    public string s_enemyCount
    {
        get
        {
            return text_enemyCount.text;
        }
        set
        {
            text_enemyCount.text = value;
        }
    }

    public string s_waveCount
    {
        get
        {
            return text_waveCount.text;
        }
        set
        {
            text_waveCount.text = value;
        }
    }

    public string s_health
    {
        get
        {
            return text_health.text;
        }
        set
        {
            text_health.text = value;
        }
    }

    public string s_objectiveHealth
    {
        get
        {
            return text_objectiveHealth.text;
        }
        set
        {
            text_objectiveHealth.text = value;
        }
    }
    #endregion

    #region GameOver
    private string s_gameOver_INITIAL;
    public string s_gameOver
    {
        get
        {
            return text_GameOver.text;
        }
        set
        {
            text_GameOver.text = value;
        }
    }
    #endregion

    #endregion
}
