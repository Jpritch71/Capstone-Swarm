
namespace Combat
{
    /// <summary>
    /// The specific method of attack
    /// one per attack
    /// </summary>
    public enum AttackMethod
    {
        Melee,
        Bow,
        Firearm,
        Magic,
        Ethereal
    }

    /// <summary>
    /// The specific type of weapon used to launch the attack
    /// one per attack
    /// </summary>
    public enum WeaponType
    {
        Sword, Axe, Polearm, Dagger, Club, Mace, Maul, Wand_Slash, //melee
        Staff_Melee, Wand_Melee, //melee
        Firearm, Laser, Bow, Crossbow, Thrown, //ranged
        Staff_Magic, Wand_Magic, //magic
        NoPhysicalWeapon //ethereal
    }

    /// <summary>
    /// The type of attack, modifies effects and damage dealt
    /// many per attack
    /// </summary>
    public enum AttackAttribute
    {
        Fire, Earth, Water, Air,
        Physical, Blunt, Slash, Stab,
        Explosive, Electric,
        God
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                Combat Modifier Utilities
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public interface I_CombatModifierHandler
    {
        System.Collections.Generic.Dictionary<AttackAttribute, float> Modifiers { get; }
        bool AddModifier(AttackAttribute attributeIn, float factorIn);
        bool ContainsAttribute(AttackAttribute attributeIn);
    }

    public class CombatModifierHandler : I_CombatModifierHandler
    {
        public System.Collections.Generic.Dictionary<AttackAttribute, float> Modifiers { get; protected set; }

        /// <summary>
        /// Add a new Combat Modifier the system. Returns false if a modifier of the same attribute type already is present.
        /// </summary>
        /// <param name="attributeIn">The modifiers AttackAttribute type.</param>
        /// <param name="factorIn">The factor to which this attribute modifies combat.</param>
        /// <returns></returns>
        public bool AddModifier(AttackAttribute attributeIn, float factorIn)
        {
            if (Modifiers.ContainsKey(attributeIn))
            {
                return false;
            }
            Modifiers.Add(attributeIn, factorIn);
            return true;
        }

        /// <summary>
        /// Returns true if this Handler already cont
        /// </summary>
        /// <param name="attributeIn"></param>
        /// <returns></returns>
        public bool ContainsAttribute(AttackAttribute attributeIn)
        {
            return Modifiers.ContainsKey(attributeIn);
        }
    }

    public class WeaponContainer
    {
        public Weapon ActiveWeapon { get; protected set; }
        public System.Collections.Generic.Dictionary<int, Weapon> Weapons;
       
        public WeaponContainer()
        {
            Weapons = new System.Collections.Generic.Dictionary<int, Weapon>();
        }

        public void AddWeapon(int idIn, Weapon weaponIn)
        {
            if (ActiveWeapon == null)
                SetActiveWeapon(weaponIn);
            Weapons.Add(idIn, weaponIn);
        }

        public void SetActiveWeapon(Weapon attackIn)
        {
            ActiveWeapon = attackIn;
        }

        public bool SetActiveWeaponByID(int attackIDin)
        {
            Weapon wep = null;
            bool success = Weapons.TryGetValue(attackIDin, out wep);
            if(success)
            {
                ActiveWeapon = wep;
                return true;
            }
            return false;
        }

        public A_Attack GetAttackByID(int idIn)
        {
            throw new System.NotImplementedException();
        }
    }
}
