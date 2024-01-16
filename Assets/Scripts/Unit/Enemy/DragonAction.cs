using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DragonAction : EnemyActionBase 
{
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
    public override IEnumerator OnDeath()
    {
        //TODO: 回転して小さくなるように
        Debug.Log("ドラゴン死亡");
        _myNavi.enabled = false; // ナビメッシュ切る
        _myAnim.SetFloat("Speed", 0); // 移動はしない
        _myAnim.SetBool("Attack", false); // 攻撃停止
        _myAnim.SetTrigger("Death"); // 死亡モーション発動

        DeathProduction();

        yield return new WaitForSeconds(_deathTime);
        Destroy(gameObject); // deathTime後に自身を撤去
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
