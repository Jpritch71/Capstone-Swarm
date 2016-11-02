using UnityEngine;
using System.Collections;

public interface I_Sight
{
    TaggableEntity Target { get; }
    float AggressionRange { get; set; }
    float DistanceToTarget();
    bool TargetAcquired { get; }

    SphereCollider VisionCollider { get; }
}
