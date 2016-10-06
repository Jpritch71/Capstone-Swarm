using UnityEngine;
using System.Collections;
namespace DamageControl
{

    public class Damage_Area : I_Damage
    {
        public float Damage { get; protected set; } //amount of base damage this entity deals 
        protected float radius; //radius that the damage entity will affect
        protected Vector2 pos;  //position in world space of the entity

        public Damage_Area()
        {
            radius = 1f;
            pos = new Vector2(-999, -999);
            Damage = 100f;
            TryDamage();
        }

        public Damage_Area(Vector2 posIn, float damageIn, float radiusIn)
        {
            pos = posIn;
            Damage = damageIn;
            radius = radiusIn;
            TryDamage();
        }

        public void TryDamage()
        {
            //Debug.Log("tryDamn");
            //Debug.DrawRay(pos, (Vector2.up - new Vector2(pos.x, pos.y)) * radiusIn);
            foreach (Collider2D c in Physics2D.OverlapCircleAll(pos, radius))
            {
                //Debug.Log("foundDamn");
                I_Entity en = c.GetComponent<I_Entity>();
                if (en != null)
                {
                    en.IncurDamage(Damage);
                    //Debug.Log("Damage[" + Damage + "] to " + en.AttachedGameObject.name);
                }
            }
        }
    }

    public class DamageControlSystem
    {
        private static DamageControlSystem _instance;
        public static DamageControlSystem _DamageController
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DamageControlSystem();
                }
                return _instance;
            }
        }

        private DamageControlSystem()
        {

        }
    }
}
