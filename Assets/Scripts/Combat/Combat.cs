
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

    public class AttackContainer
    {
        public System.Collections.Generic.Dictionary<int, A_Attack> Attacks;
        public A_Attack AutoAttack { get; protected set; }

        public AttackContainer()
        {
            Attacks = new System.Collections.Generic.Dictionary<int, A_Attack>();
        }

        public void AddAttack(int idIn, A_Attack attackIn)
        {
            if (AutoAttack == null)
                SetAutoAttack(attackIn);
            Attacks.Add(idIn, attackIn);
        }

        public void SetAutoAttack(A_Attack attackIn)
        {
            AutoAttack = attackIn;
        }

        public bool SetAutoAttackByID(int attackIDin)
        {
            A_Attack att = null;
            bool success = Attacks.TryGetValue(attackIDin, out att);
            if(success)
            {
                AutoAttack = att;
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
