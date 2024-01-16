using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CombatAction))]
public class UnitBase : MonoBehaviour , IAnimationAttackable// AnimationEvent関数を発生させるには継承元に関数が必要っぽいので必須にした
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
            Assert.IsNotNull(_weaponActions[i], $"_weaponActions[{i}]がnullです");
        }
    }
    public virtual void AttackStart()  { }
    public virtual void AttackFinish() { }
}

//HACK: AnimationEvent関数を発生させるには継承元に関数が必要っぽい
public interface IAnimationEventer { }
public interface IAnimationAttackable : IAnimationEventer
{
    /// <summary>
    /// 攻撃有効化
    /// </summary>
    void AttackStart();
    /// <summary>
    /// 攻撃無効化
    /// </summary>
    void AttackFinish();
}