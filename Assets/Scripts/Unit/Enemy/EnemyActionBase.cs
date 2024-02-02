using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]// Animator��NavMesh��K�{��
public class EnemyActionBase : UnitBase
{
    [Header("�͈�")]
    [SerializeField, Tooltip("�U���J�n����")] protected float _fireDistance = 1.0f;
    [SerializeField, Tooltip("���G�͈�")] protected float _searchRange = 5.0f;
    [Space(20)]
    [SerializeField] protected float _deathTime = 3.0f; // ���S��ɏ�����܂ł̎���
    [SerializeField] private GameObject _patDamage; // �_���[�W�G�t�F�N�g
    [SerializeField] protected Vector3 _damagePos = new Vector3(0, 1.5f, 0); // �_���[�W�G�t�F�N�g�̈ʒu
    [Space(20)]
    protected NavMeshAgent _myNavi; // ���g�̃i�r���b�V��
    protected Animator _myAnim; // ���g�̃A�j���[�^�[
    private CombatAction _myCA;   // ���g��CombatAction

    private GameObject _player; // �v���C���[
    private CombatAction _playerCA; // �v���C���[��CombatAction

    private void Start()
    {
        base.Start();
        TryGetComponent(out _myAnim); // ���g�̃A�j���[�^�[���擾
        TryGetComponent(out _myNavi); // ���g�̃i�r���b�V�����擾
        TryGetComponent(out _myCA);�@ // ���g��CombatAction���擾
        _player = GameObject.FindGameObjectWithTag("Player"); // �v���C���[���擾
        if (_player)
        {
            // �v���C���[��CombatAction���擾
            _player.TryGetComponent(out _playerCA);
        }
    }
    private void Update()
    {
        ActionEnemy();
    }
    protected void ActionEnemy()
    {
        if (!_player || _myCA.IsDead) return;// �v���C���[��������
        // �v���C���[���S���̑Ή�
        if (_playerCA.IsDead)
        {
            SomeAnimationsStopped();
            return;
        }
        // �v���C���[�Ƃ̋��������߂�
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance > _searchRange)
        {
            // �ړ���~
            _myNavi.enabled = false; // �i�r���b�V���؂�
            _myAnim.SetFloat("Speed", 0); //�ړ��͂��Ȃ�
            _myAnim.SetBool("Attack", false); //�U����~
        }
        else if (distance <= _fireDistance)// �v���C���[�Ƃ̋�����_fireDistance�ȉ��A
        {
            // �����~�܂��čU��
            _myNavi.enabled = false; // �i�r���b�V���؂�
            _myAnim.SetFloat("Speed", 0); //�ړ��͂��Ȃ�
            _myAnim.SetBool("Attack", true); //�U���J�n
        }
        else // �v���C���[�Ƃ̋�����_fireDistance�`_searchRange�Ȃ�
        {
            // �v���C���[��Ǐ]
            _myNavi.enabled = true; // �i�r���b�V�����I��
            _myNavi.destination = _player.transform.position; // �^�[�Q�b�g���w��
            _myAnim.SetFloat("Speed", _myNavi.velocity.magnitude); //�ړ����[�V����
            _myAnim.SetBool("Attack", false); //�U����~
        }
    }
    /// <summary>
    /// �_���[�W���o����
    /// </summary>
    /// <returns></returns>
    public override void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // �_���[�W�G�t�F�N�g�𐶐�
        Fx.transform.position = transform.position + _damagePos; // �ʒu��␳
        Destroy(Fx, 1.0f); // �G�t�F�N�g��1.0�b��ɔj��
    }
    /// <summary>
    /// ���S����
    /// </summary>
    /// <returns></returns>
    public override async UniTaskVoid OnDeath()
    {
        _myNavi.enabled = false; // �i�r���b�V���؂�
        
        _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
        _myAnim.SetBool("Attack", false); // �U����~

        _myAnim.SetTrigger("Death"); // ���S���[�V��������
        gameObject.tag = "Untagged";
        Destroy(gameObject, _deathTime); // _deathTime�b��Ɏ��g���폜
    }
    protected virtual void SomeAnimationsStopped()
    {
        _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
        _myAnim.SetBool("Attack", false); // �U����~
    }
    /// <summary>
    /// ���S���o
    /// </summary>
    protected virtual void DeathProduction()
    {
        // ���񂾂񏬂����Ȃ�
        transform.DOScale(Vector3.zero, _deathTime)
            .SetEase(Ease.OutCirc)
            .OnUpdate(() =>
            {
                transform.rotation *= Quaternion.Euler(Vector3.up * Time.deltaTime * 360);
            })
            .Play();
    }

//----------�A�j���[�V�����C�x���g------------------------
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
//-------------------------------------------------------
}