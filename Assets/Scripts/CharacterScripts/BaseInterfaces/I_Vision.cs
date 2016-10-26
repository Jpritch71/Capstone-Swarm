using UnityEngine;
using System.Collections;

public interface I_Vision
{
    TaggableEntity Target { get; }
    float VisionRange { get; set; }
    float DistanceToTarget();
    bool TargetAcquired { get; }
    bool AddTarget(TaggableEntity entityIn);

    SphereCollider VisionCollider { get; }
}
