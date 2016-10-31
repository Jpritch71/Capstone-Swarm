using UnityEngine;
using System.Collections;

public interface I_Movement
{
    bool Moving { get; }
    bool CanMove { get; }

    void StopMoving();

    float BaseSpeed
    {
        get;
    }

    float SpeedModifier
    {
        get;
    }

    float Speed
    {
        get;
    }

    float GroundPosY
    {
        get;
    }

    /*
     * Use this to set or get the character's position
     * Get - Gets the current position
     * Set - Sets the position, offseting the value so that the collider is resting on the ground.
     * */
    Vector3 Pos
    {
        get;
    }

    I_Controller C_Controller { get; set; }

    //// REVIST THE ABOVE,
    //// See about doing a GetComponent<I_Controller>() in the InitAwake methods of each movement component. The appropriate controllers MIGHT always be in place
}
