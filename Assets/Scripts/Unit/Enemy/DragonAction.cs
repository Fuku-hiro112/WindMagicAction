using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

public class DragonAction : EnemyActionBase 
{
    [SerializeField] Material _material;
    private void Reset()
    {
        _fireDistance = 5;
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

        _myAnim.SetFloat("Speed", 0); // 移動はしない
        _myAnim.SetBool("Attack", false); // 攻撃停止

        _myAnim.SetTrigger("Death"); // 死亡モーション発動
        gameObject.tag = "Untagged";

        DeathPerformance();// 回転しながら小さくなる
    }
    /// <summary>
    /// 死亡演出
    /// </summary>
    /// <param name="obj"></param>
    protected override void DeathPerformance()
    {
        SmallingWhileRotating();
        Assert.IsNotNull(_material, $"_materialがnullです");
        //_material.DOFade(0, _deathTime);//HACK: 出来ない
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
