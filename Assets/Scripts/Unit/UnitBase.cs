using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CombatAction))]
public class UnitBase : MonoBehaviour , IAnimationAttackable// AnimationEventŠÖ”‚ğ”­¶‚³‚¹‚é‚É‚ÍŒp³Œ³‚ÉŠÖ”‚ª•K—v‚Á‚Û‚¢‚Ì‚Å•K{‚É‚µ‚½
{
    [SerializeField] protected GameObject[] _weapons = new GameObject[0];
    protected WeaponAction[] _weaponActions;

    public virtual void OnDamage(){}
    public virtual IEnumerator OnDeath(){ yield return null; }

    protected void OnStart()
    {
        _weaponActions = new WeaponAction[_weapons.Length];

        for (int i = 0; i < _weapons.Length; i++)
        {
            _weapons[i].TryGetComponent(out _weaponActions[i]);
            Assert.IsNotNull(_weaponActions[i], $"_weaponActions[{i}]‚ªnull‚Å‚·");
        }
    }
    public virtual void AttackStart()  { }
    public virtual void AttackFinish() { }
}

//HACK: AnimationEventŠÖ”‚ğ”­¶‚³‚¹‚é‚É‚ÍŒp³Œ³‚ÉŠÖ”‚ª•K—v‚Á‚Û‚¢
public interface IAnimationEventer { }
public interface IAnimationAttackable : IAnimationEventer
{
    /// <summary>
    /// UŒ‚—LŒø‰»
    /// </summary>
    void AttackStart();
    /// <summary>
    /// UŒ‚–³Œø‰»
    /// </summary>
    void AttackFinish();
}