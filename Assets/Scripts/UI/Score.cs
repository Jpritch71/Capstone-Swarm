public class Score
{
    public int score { get; private set; }

    private Score()
    {
        AddScore(0);
    }

    private static Score instance;
    public static Score _Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Score();
            }
            return instance;
        }
    }

    public void AddScore(int scoreIn)
    {
        score += scoreIn;
        SARSiteUI._Instance.s_score = "Score: {" + score + "}";
    }

    public void ResetScore()
    {
        score = 0;
        SARSiteUI._Instance.s_score = "Score: {" + score + "}";
    }
}
