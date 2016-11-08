using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SendUserInfo : MonoBehaviour
{
    public Text userText, passText;
    public InputField passwordField;

    public void Send()
    {
        DatabaseConnect.SendUserInfo(userText.text, passwordField.text);
        this.GetComponent<Button>().onClick = null;
    }
}
