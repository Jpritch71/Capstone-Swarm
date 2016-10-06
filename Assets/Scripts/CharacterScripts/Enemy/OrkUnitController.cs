using UnityEngine;
using System.Collections;

public abstract class OrkUnitController : MonoBehaviour
{
    // Use this for initialization
    void Awake()
    {
        InitAwake();
    }

    protected abstract void InitAwake();

    // Update is called once per frame
    void Update()
    {
        if (stateController != null)
            stateController.ExecuteUpdate(); //update State Machine
    }

    #region Components
    public GroupUnit MovementComponent { get; protected set; }
    public GrouchoAnimController AnimController { get; protected set; }
    public StateMachine stateController { get; protected set; }
    #endregion

    public void msg(string s)
    {
        print(s);
    }
}
