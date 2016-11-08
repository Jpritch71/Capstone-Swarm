using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DamageControl
{
    public class DamageController : MonoBehaviour
    {
        static DamageController DamageManager;
        public static DamageController Instance
        {
            get
            {
                if (DamageManager == null)
                {
                    GameObject go = new GameObject();
                    go.name = "DamageManager";
                    DamageManager = go.AddComponent<DamageController>();
                }
                return DamageManager;
            }
        }

        private List<Damage_AreaForTime> d_OverTime;

        private HashSet<Damage_AreaForTime> addQueue;
        private HashSet<Damage_Area> removalQueue;

        void Awake()
        {
            d_OverTime = new List<Damage_AreaForTime>();
            addQueue = new HashSet<Damage_AreaForTime>();

            removalQueue = new HashSet<Damage_Area>();
            StartCoroutine(CheckRemove());
        }

        IEnumerator CheckRemove()
        {
            while (true)
            {
                yield return new WaitForSeconds(5f);
                if (removalQueue.Count > 0)
                    removalQueue.Clear();
            }
        }

        void FixedUpdate()
        {
            if (addQueue.Count > 0)
            {
                d_OverTime.AddRange(addQueue);
                addQueue.Clear();
            }
            foreach (Damage_AreaForTime d in d_OverTime)
            {
                if (d.Update())
                    removalQueue.Add(d);
            }
            foreach (Damage_AreaForTime d in removalQueue)
            {
                d_OverTime.Remove(d);
            }
        }

        public void Add_DamageOverTime(Damage_AreaForTime damnIn)
        {
            addQueue.Add(damnIn);
        }

        public void AddDamageToTarget(I_Entity enTarget, float damageIn)
        {
            enTarget.IncurDamage(damageIn);
        }
    }

    interface I_Damage
    {
        float Damage { get; }
    }
}
