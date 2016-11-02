// C# , ADO.NET  
using System;
using System.Collections;
using UnityEngine;

public class DatabaseConnect : MonoBehaviour
{
    public string url = "http://webswarm.azurewebsites.net/Home/About";
    public bool done = false;
    IEnumerator Start()
    {
        WWW www = new WWW(url);
        WWWForm f = new WWWForm();

        f.AddField("Connected", "con");
        Debug.Log("con".ToCharArray().GetValue(0));
        www = new WWW(url, f);
        yield return www;
        url = www.text;
        Debug.Log(www);
        
        foreach(string s in www.responseHeaders.Values)
        {
            Debug.Log(s);
        }
        done = true;

        foreach(byte b in f.data)
        {
            Debug.Log((char)b);
        }
        
    }
}

/**** Actual output:  
Connected successfully.  
Press any key to finish...  
****/
