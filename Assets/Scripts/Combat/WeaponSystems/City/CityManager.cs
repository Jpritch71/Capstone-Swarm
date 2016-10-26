using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ObeliskManager : MonoBehaviour
{
    public GameObject[] Obelisks; //use this to populate list of cities from the editor
    public static GameObject[] ObelisksGameObjectsGet; //use this as a super fucking lazy way to share the list of Obelisks throughout the program

    void Awake()
    {
        ObelisksGameObjectsGet = Obelisks;
    }
    
    public static PlayerStructure GetRandomCity()
    {
        return ObelisksGameObjectsGet[Random.Range(0, ObelisksGameObjectsGet.Length)].GetComponent<PlayerStructure>();
    }

    
    public static void CheckStatus()
    {
        bool alive = false;
        foreach(GameObject g in ObelisksGameObjectsGet)
        {
            PlayerStructure c = g.GetComponent<PlayerStructure>();
            if (c == null)
                continue;
            if (c.StructureAlive)
                alive = true;
        }
        if (alive)
            return;
        else
        {
            //var manager = GameObject.Find("FacebookManager").GetComponent<FacebookManager>();

           // if (Score.score > PlayerPrefs.GetInt("highscore"))
           // {
             //   PlayerPrefs.SetInt("highscore", Score.score);
            //}
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
