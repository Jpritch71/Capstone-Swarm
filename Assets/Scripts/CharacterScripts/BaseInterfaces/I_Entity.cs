﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface I_Entity
{
    int Unique_ID { get; }

    //bool Initialized { get; }
    bool Killable { get; }
    bool Dead { get; }
    float BaseIntegrity { get; }
    float Integrity { get; }

    void IncurDamage(float damageIn);
    void DeathAction();

    I_Controller _Owner_Controller
    {
        get;
    }
}

public class EntityManager
{
    //private static HashSet<I_Entity> IDs;
    private static int ID;
    public static int GetUniqueID(ref I_Entity entityIn)
    {
        //entityIn.Unique_ID = ID;
        return ID++;
    }
}

