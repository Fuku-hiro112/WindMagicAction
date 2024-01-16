using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(CombatAction))]
public class UnitBase : MonoBehaviour , IAnimationAttackable// AnimationEvent�֐��𔭐�������ɂ͌p�����Ɋ֐����K�v���ۂ��̂ŕK�{�ɂ���
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
            Assert.IsNotNull(_weaponActions[i], $"_weaponActions[{i}]��null�ł�");
        }
    }
    public virtual void AttackStart()  { }
    public virtual void AttackFinish() { }
}

//HACK: AnimationEvent�֐��𔭐�������ɂ͌p�����Ɋ֐����K�v���ۂ�
public interface IAnimationEventer { }
public interface IAnimationAttackable : IAnimationEventer
{
    /// <summary>
    /// �U���L����
    /// </summary>
    void AttackStart();
    /// <summary>
    /// �U��������
    /// </summary>
    void AttackFinish();
}