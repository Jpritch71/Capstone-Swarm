using UnityEngine;
using UnityEngine.UI;

public class AttackButtonHandler : MonoBehaviour
{
    public Button[] buttons;

    void Awake()
    {
        buttons = GetComponentsInChildren<Button>();

       // print(buttons.Length);
    }

    public bool ClickButton(int buttonIndex)
    {
        if (buttonIndex > buttons.Length || buttonIndex < 0)
            return false;

        Button button = buttons[buttonIndex - 1];

        if (button == null)
            return false;

        button.onClick.Invoke();
        //Debug.Log("INVOKED");
        return true;
    }

    public void SetButton(int buttonIndex, Weapon attacker, int attackID)
    {
        buttons[buttonIndex].onClick.AddListener(delegate { attacker.DoAttack(attackID); });
        SetButtonName(buttonIndex, attacker.GetAttackByID(attackID).AttackName);
    }

    private void SetButtonName(int buttonIndex, string nameIn)
    {
        buttons[buttonIndex].GetComponentInChildren<Text>().text = nameIn;
    }
}
