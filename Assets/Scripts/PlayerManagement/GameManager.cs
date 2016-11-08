using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager
{
    private GameManager()
    {
        var SCORE_INITIALIZE = Score._Instance;
    }

    private static GameManager instance;
    public static GameManager _Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }

    private Timer gameStopTimer;
    private bool gameStoped = false;
    public void GameOver()
    {
        if (!gameStoped)
        {
            gameStoped = true;
            gameStopTimer = Timer.Register(5f, () => { Time.timeScale = 0f; }, null, false, true, null);

            SARSiteUI._Instance.Switch_GameOver();
        }
    }

    public void PauseGame()
    {
        if (!gameStoped)
        {
            if(gameStopTimer == null)
                gameStopTimer = Timer.Register(5f, () => { Time.timeScale = 0f; }, null, false, true, null);
        }
    }

    public void RestartGame()
    {
        if (gameStoped)
        {
            if (gameStopTimer != null)
            {
                gameStopTimer.Cancel();
                gameStoped = false;
            }
        }

        Score._Instance.ResetScore();
        Time.timeScale = 0;
        WaveManager.RestartWaves();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SARSiteUI.ReloadUI();
        Time.timeScale = 1;
        SARSiteUI._Instance.Switch_GameRunning();
    }
}
