using UnityEngine;
using UnityEngine.UI;

public class SARSiteUI
{
    public Text scoreText, ammoText, waveText;

    private SARSiteUI()
    {
        scoreText = GameObject.Find("UIscore").GetComponent<Text>();
        ammoText = GameObject.Find("UIammo").GetComponent<Text>();
        waveText = GameObject.Find("UIwave").GetComponent<Text>();
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

    public string scoreString
    {
        get
        {
            return scoreText.text;
        }
        set
        {
            scoreText.text = value;
        }
    }

    public string ammoString
    {
        get
        {
            return ammoText.text;
        }
        set
        {       
            ammoText.text = value;
        }
    }

    public string waveString
    {
        get
        {
            return waveText.text;
        }
        set
        {
            waveText.text = value;
        }
    }
}
