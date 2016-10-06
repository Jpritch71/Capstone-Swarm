using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private RaycastHit[] hits; //variable for storing temporary RaycastHit array
    private RaycastHit hit;

    // Use this for initialization
    void Awake ()
    {
        MovementComponent = GetComponent<PlayerMovement>();
        //AnimController = new PlayerAnimController(GameObject.Find("PlayerUnit").GetComponent<Animation>());
        //stateController = new StateMachine(MovementComponent, new S_Player_Idle(this));
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(stateController != null)
            stateController.ExecuteUpdate(); //update State Machine

        if (Input.GetMouseButton(0) && MovementComponent.CanMove)
        {
            if (!MovementComponent.DirectionalMovement)
            {
                MovementComponent.StopMoving();
            }
            MovementComponent.StartFreeMoving();
        }
        else if(Input.GetMouseButtonUp(0) || !MovementComponent.CanMove)
        {
            MovementComponent.EndFreeMove();
        }
        if (Input.GetMouseButtonDown(1) && !MovementComponent.DirectionalMovement)
        {
            hits = Physics.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.ScreenPointToRay(Input.mousePosition).direction, Mathf.Infinity, (1 << 8) + (1 << 9));

            if (hits.Length > 0)
            {
                hit = hits[0];
                for (int x = 1; x < hits.Length; x++)
                {
                    if (hit.point.y < hits[x].point.y)
                        hit = hits[x];
                }
                //marker.transform.position = hit.point;
                MovementComponent.SetPathToPoint(hit.point);
            }
        }
    }

    #region Components
    public PlayerMovement MovementComponent { get; private set; }
    public PlayerAnimController AnimController { get; private set; }
    public StateMachine stateController { get; private set; }
    #endregion
}
