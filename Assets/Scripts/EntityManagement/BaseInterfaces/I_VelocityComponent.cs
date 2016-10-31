using UnityEngine;
using System.Collections;

public interface I_VelocityComponent
{
    /// <summary>
    /// bool to check if the Entity associated with this velocity component is alive
    /// </summary>
    bool IsDead { get; }
    Vector2 Velocity { get; }
}
