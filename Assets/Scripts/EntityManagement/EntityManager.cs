 using UnityEngine;
using System.Collections.Generic;

public class EntityManager
{
    private static HashSet<int> IDs = new HashSet<int>();
    private static Dictionary<int, I_Entity> entityByID = new Dictionary<int, I_Entity>();

    public static int AddEntity_GenerateID(ref I_Entity entityIn)
    {
        if (IDs.Contains(entityIn.Unique_ID))
            return -1;
        entityByID.Add(entityIn.Unique_ID, entityIn);
        return entityIn.Unique_ID;
    }

    public static bool GetEntityByCollider(Collider colliderAsID, ref I_Entity entityIn)
    {
        return entityByID.TryGetValue(colliderAsID.gameObject.GetInstanceID(), out entityIn);
    }

    public static bool GetEntityByUniqueID(int IDin, ref I_Entity entityIn)
    {
        return entityByID.TryGetValue(IDin, out entityIn);
    }

    public static bool GetEntityByUniqueID(int IDin, ref TaggableEntity taggableEntityIn)
    {
        I_Entity taggable = taggableEntityIn;
        bool success = entityByID.TryGetValue(IDin, out taggable);
        taggableEntityIn = (TaggableEntity)taggable;
        return success;
    }
}