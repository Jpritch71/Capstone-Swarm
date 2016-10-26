using UnityEngine;
using System.Collections;

public class AspectRatioScale : MonoBehaviour
{
    private const int width = 1600;
    private const int height = 1000;
    private int widthRatio, heightRatio;
    private float aspectRatio;

    void Start()
    {
        float OldObjectWidth = this.transform.localScale.x;
        //print (OldObjectWidth);
        float NewObjectWidth = ((OldObjectWidth * 16.0f) / ((10f * Screen.width) / Screen.height));
        this.transform.localScale = new Vector3(NewObjectWidth, 1, 1);
        print(NewObjectWidth);
    }
}
