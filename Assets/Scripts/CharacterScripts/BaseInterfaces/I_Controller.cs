using UnityEngine;
using System.Collections;

public interface I_Controller
{ 
    #region Components
    I_Movement C_Movement { get; }
    I_Entity C_Entity { get; }    
    StateMachine C_StateMachine { get; }
    GameObject C_AttachedGameObject { get; }
    #endregion

    void LoadStats();
}
