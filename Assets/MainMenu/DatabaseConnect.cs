// C# , ADO.NET  
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class DatabaseConnect : MonoBehaviour
{
    private string urlResponse = "";
    private string urlBuild = "http://71stcompany.azurewebsites.net/SwarmUser/Login/";
    public bool done = false;

    public Text responseText;

    private static bool canBegin = false;

    public string username;

    IEnumerator WebSend(string usernameIn, string passIn)
    {
        username = usernameIn;
        urlBuild += "" + usernameIn.ToLower() + "/" + passIn;
        WWW www = new WWW(urlBuild);
        yield return www;

        string respText = www.text;

        Debug.Log(urlBuild);

        if (www.responseHeaders.Count > 0)
        {
            foreach (KeyValuePair<string, string> entry in www.responseHeaders)
            {
                Debug.Log("KEY: " + entry.Key + "=" + entry.Value);
                //Output: HTTP/1.0 200 OK=STATUS
                //...
                //image/jpeg=CONTENT-TYPE
            }
        }

        done = true;

        if (!www.responseHeaders.TryGetValue("FoundUser", out urlResponse))
        {
            www.responseHeaders.TryGetValue("status", out urlResponse);
            SetResponse("Login Failed: Response --> \n" + urlResponse);
        }

        if (urlResponse == "failure")
        {
            SetResponse("Login Failed: Connection Refused");
        }
        else
        {
            if (urlResponse == "user added")
            {
                canBegin = true;
                SetResponse("User Succesfully Registered,\nBeginning Game");
            }
            if (urlResponse == "success")
            {
                canBegin = true;
                SetResponse("User Logged In,\nBeginning Game");
            }
            Timer.Register(2f, BeginGame, null, false, true, this);
        }

    }

    public static void SendUserInfo(string usernameIn, string passIn)
    {
        _Instance.StartCoroutine(_Instance.WebSend(usernameIn, passIn));
    }

    public void SetResponse(string res)
    {
        urlResponse = res;
        responseText.text = res;
    }

    public void BeginGame()
    {
        if (!canBegin)
            return;
        PlayerStats.PlayerStatContainer._PlayerInfo = new PlayerInfo(username, 0, 2);
        SceneManager.LoadScene("Alcove");
    }

    private static DatabaseConnect dbC;
    private static DatabaseConnect _Instance
    {
        get
        {
            if(dbC == null)
            {
                dbC = Camera.main.GetComponent<DatabaseConnect>();
            }
            return dbC;
        }
    }
}

/**** Actual output:  
Connected successfully.  
Press any key to finish...  
****/
