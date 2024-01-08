using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // �i�r���b�V���̗��p�ɕK�v
public class EnemyAction : MonoBehaviour
{
    NavMeshAgent _myNavi; // ���g�̃i�r���b�V��
    Animator _myAnim; // ���g�̃A�j���[�^�[
    GameObject _player; // �v���C���[
    [SerializeField] float _deathTime = 3.0f; // ���S��ɏ�����܂ł̎���
    CombatAction _myCA; // ���g��CombatAction
    CombatAction _playerCA; // �v���C���[��CombatAction
    [SerializeField] GameObject _patDamage; // �_���[�W�G�t�F�N�g
    Vector3 _damagePos = new Vector3(0, 1.5f, 0); // �_���[�W�G�t�F�N�g�̈ʒu
    [SerializeField] GameObject _weapon;
    WeaponAction _weaponAction;
    void Start()
    {
        TryGetComponent(out _myAnim); // ���g�̃A�j���[�^�[���擾
        TryGetComponent(out _myNavi); // ���g�̃i�r���b�V�����擾
        TryGetComponent(out _myCA); // ���g��CombatAction���擾
        _weapon.TryGetComponent(out _weaponAction);
        _player = GameObject.FindGameObjectWithTag("Player"); // �v���C���[���擾
        if (_player)
        {
            // �v���C���[��CombatAction���擾
            _player.TryGetComponent(out _playerCA);
        }
    }
    // �U���L����
    void AttackStart()
    {
        _weaponAction.WeaponActivate(true);
    }
    // �U��������
    void AttackFinish()
    {
        _weaponAction.WeaponActivate(false);
    }
    // �_���[�W���o����
    void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // �_���[�W�G�t�F�N�g�𐶐�
        Fx.transform.position = transform.position + _damagePos; // �ʒu��␳
        Destroy(Fx, 1.0f); // �G�t�F�N�g��1.0�b��ɔj��
    }
    // ���S����
    void OnDeath()
    {
        _myNavi.enabled = false; // �i�r���b�V���؂�
        _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
        _myAnim.SetBool("Attack", false); // �U����~
        _myAnim.SetTrigger("Death"); // ���S���[�V��������
        Destroy(gameObject, _deathTime); // deathTime��Ɏ��g��P��
    }
    void Update()
    {
        if (!_player || _myCA.IsDead)
        {
            return; // �v���C���[�������Ȃ�A�������Ȃ�
        }
        // �v���C���[���S���̑Ή�
        if (_playerCA.IsDead)
        {
            _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
            _myAnim.SetBool("Attack", false); // �U����~
            return; // �ȍ~�̏����͍s��Ȃ�
        }
        // �v���C���[�Ƃ̋��������߂�
        float D = Vector3.Distance(transform.position, _player.transform.position);
        if (D > 5.0f)
        {
            // �v���C���[��5m�ȏ��
            _myNavi.enabled = false; // �i�r���b�V���؂�
            _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
            _myAnim.SetBool("Attack", false); // �U����~
        }
        else if (D <= 1.0f)
        {
            // �v���C���[�Ƃ̋�����1m�ȉ��A�����~�܂��čU���J�n
            _myNavi.enabled = false; // �i�r���b�V���؂�
            _myAnim.SetFloat("Speed", 0); // �ړ��͂��Ȃ�
            _myAnim.SetBool("Attack", true); // �U���J�n
        }
        else
        {
            // �v���C���[�Ƃ̋�����1�`5m�Ȃ�A�ǂ�������
            _myNavi.enabled = true; // �i�r���b�V���Œǂ�
            _myNavi.destination = _player.transform.position; // �^�[�Q�b�g���w��
            _myAnim.SetFloat("Speed", _myNavi.velocity.magnitude); // �ړ����[�V����
            _myAnim.SetBool("Attack", false); // �U����~
        }
    }
}