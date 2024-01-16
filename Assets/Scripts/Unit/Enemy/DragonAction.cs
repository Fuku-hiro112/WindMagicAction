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
    /// ���S����
    /// </summary>
    public override IEnumerator OnDeath()
    {
        //TODO: ��]���ď������Ȃ�悤��
        Debug.Log("�h���S�����S");
        _myNavi.enabled = false; // �i�r���b�V���؂�
        _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
        _myAnim.SetBool("Attack", false); // �U����~
        _myAnim.SetTrigger("Death"); // ���S���[�V��������

        DeathProduction();

        yield return new WaitForSeconds(_deathTime);
        Destroy(gameObject); // deathTime��Ɏ��g��P��
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
