using UnityEngine;
using System.Collections;
using System.Diagnostics;

namespace DamageControl
{
    public class Damage_AreaForTime : Damage_Area
    {
        private int lifeTime; //how long the damage entity persists
        public bool Damaging { get; set; } //is the entity actively damaging
        private Stopwatch lifeTimer, checkTimer; //respectively, the timer monitoring the total life time of the entity - the timer checkinging if the entity should check inside its radius for damagable objects

        public Damage_AreaForTime(Vector2 posIn, float damageIn, float radiusIn, int time)
        {
            lifeTimer = new Stopwatch();
            lifeTimer.Start();
            checkTimer = new Stopwatch();
            checkTimer.Start();

            pos = posIn;
            radius = radiusIn;
            lifeTime = time;
            Damaging = true;
            DamageControl.DamageController.Instance.Add_DamageOverTime(this);
        }

        public bool Update()
        {
            if (lifeTimer.ElapsedMilliseconds > lifeTime)
            {
                lifeTimer.Stop(); //life over, end the entity
                Damaging = false; //turn off the damage
                return true;
                //DamageController.Instance.RemoveDamage(this); //remove from the damage controller - entity will be removed once
            }
            if (Damaging && checkTimer.ElapsedMilliseconds > 100)
            {
                TryDamage(); //see - Class: Damage Entity
                checkTimer.Reset(); //reset the check timer --v start it up again. 
                checkTimer.Start();
            }
            return false;
        }
    }
}
