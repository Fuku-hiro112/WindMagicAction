using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unit;

public class WargAction : EnemyActionBase
{
    [SerializeField] Material _material;
    private void Reset()
    {
        _fireDistance = 2;
        _searchRange = 10;
        _deathTime = 3;
        _damagePos = new Vector3(0, 1.5f, 0);
    }
    protected new void Start()
    {
        GameObject.FindGameObjectWithTag("WanderingPosStorage").TryGetComponent(out WanderingManager);
        Assert.IsNotNull(WanderingManager, $"{this}‚ÌWanderingManager‚ªNull‚Å‚·");
        base.Start();
    }

    /// <summary>
    /// €–Sˆ—
    /// </summary>
    public override async UniTaskVoid OnDeath()
    {
        Debug.Log("˜T€–S");
        base.OnDeath().Forget();
        DeathPerformance();// ‰‰o ‰ñ“]‚µ‚È‚ª‚ç¬‚³‚­‚È‚é
    }
    /// <summary>
    /// €–S‰‰o
    /// </summary>
    /// <param name="obj"></param>
    protected override void DeathPerformance()
    {
        SmallingWhileRotating();
        //Assert.IsNotNull(_material, $"_material‚ªnull‚Å‚·");
        //_material.DOFade(0, _deathTime);//HACK: o—ˆ‚È‚¢
    }

#region AnimationEvent

    /// <summary>
    /// UŒ‚—LŒø‰»
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);
        IsAttacking = true;
    }
    /// <summary>
    /// UŒ‚–³Œø‰»
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
        IsAttacking = false;
    }

#endregion
}
