using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// 死亡処理
    /// </summary>
    public override async UniTaskVoid OnDeath()
    {
        Debug.Log("ドラゴン死亡");
        _myNavi.enabled = false; // ナビメッシュ切る
        SomeAnimationsStopped();
        _myAnim.SetTrigger("Death"); // 死亡モーション発動
        gameObject.tag = "Untagged";

        DeathProduction();
        Destroy(gameObject, _deathTime); // deathTime後に自身を撤去
    }
    protected override void DeathProduction()
    {
        _material.DOFade(0, _deathTime);// 出来ない
        base.DeathProduction();
    }
    //-----------アニメーションイベント-----------------------
    /// <summary>
    /// 攻撃有効化
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);
    }
    /// <summary>
    /// 攻撃無効化
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
    }
    //----------------------------------------------------------
}
