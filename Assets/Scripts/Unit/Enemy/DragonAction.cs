using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

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
    /// ���S����
    /// </summary>
    public override async UniTaskVoid OnDeath()
    {
        Debug.Log("�h���S�����S");
        _myNavi.enabled = false; // �i�r���b�V���؂�

        _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
        _myAnim.SetBool("Attack", false); // �U����~

        _myAnim.SetTrigger("Death"); // ���S���[�V��������
        gameObject.tag = "Untagged";

        DeathProduction();
        Destroy(gameObject, _deathTime); // deathTime��Ɏ��g��P��
    }
    protected override void DeathProduction()
    {
        _material.DOFade(0, _deathTime);//HACK: �o���Ȃ�
        base.DeathProduction();
    }
//-----------�A�j���[�V�����C�x���g-----------------------
    /// <summary>
    /// �U���L����
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);
    }
    /// <summary>
    /// �U��������
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
    }
//----------------------------------------------------------
}
