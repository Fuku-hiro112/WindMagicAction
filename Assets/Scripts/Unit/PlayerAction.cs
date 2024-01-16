using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // �VInput�V�X�e���̗��p�ɕK�v
using GameInput;

[RequireComponent(typeof(Animator))]
public class PlayerAction : UnitBase ,IAnimationAttackable
{
    [Header("���x�ݒ�")]
    [SerializeField] private float _moveSpeed = 4.0f; // �ړ����x
    [SerializeField, Tooltip("��]���x")] private float _rotationSpeed = 8.0f; // ��]���x
    [SerializeField, Tooltip("�Ēa���鎞��")] private float _birthInterval = 5.0f; // �Ēa���܂ł̎���

    [Header("�������ݒ�")]
    [SerializeField, Tooltip("��������")] private float _strongDuration = 10.0f; // ��������
    [SerializeField, Tooltip("�����l")] private int _strongValue = 10; // �����l�@�U���{�����l���U���l
    [Space(20)]
    [SerializeField] private int _healValue = 50; // �񕜒l
    [Space(20)]
    [Header("�A�^�b�`�K�{�I�u�W�F�N�g")]
    [SerializeField] private GameObject _patDamage; // �_���[�W�G�t�F�N�g
    [SerializeField] private GameObject _swordWeapon;

    private Animator _myAnim; // ���g�̃A�j���[�^�[
    private CombatAction _myCA; // ���g��CombatAction
    
    private Vector3 _damagePos = new Vector3(0, 1.5f, 0); // �_���[�W�G�t�F�N�g�̈ʒu
    private GameObject _patSmoke; // ���s�G�t�F�N�g
    private GameObject _patStrong; // �����G�t�F�N�g
    private ParticleSystem _patHeal; // �񕜃G�t�F�N�g
    private ParticleSystem.MainModule _smokeMain; // ���s�����̖{��
    private WeaponAction _weaponAction;
    private ConfirmAction _confirmAction;

    private void Start()
    {
        base.OnStart();
        TryGetComponent(out _myAnim);// ���g�̃A�j���[�^�[���擾
        TryGetComponent(out _myCA); // ���g��CombatAction���擾
        _patSmoke  = transform.Find("PatSmoke").gameObject; // ���s�G�t�F�N�g���擾
        _patStrong = transform.Find("PatStrong").gameObject; // �����G�t�F�N�g���擾
        _swordWeapon.TryGetComponent(out _weaponAction);// SwordAction���擾
        transform.Find("PatHeal").TryGetComponent(out _patHeal); // �񕜃G�t�F�N�g���擾
        _smokeMain = _patSmoke.GetComponent<ParticleSystem>().main; // ���s�����̖{�̂��擾
        _confirmAction = ConfirmAction.s_Instance;

        _patHeal.Stop(); // �񕜃G�t�F�N�g���~
        _patStrong.SetActive(false); // �����G�t�F�N�g�𖳌���
    }
    private void FixedUpdate()
    {
        if (_myCA.IsDead) return; // ���g������ł��牽�����Ȃ�

        // �ړ������̃x�N�g�����쐬
        Vector3 direction = _confirmAction.MoveDirection;
        Quaternion horizontalRotation = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        Vector3 moveDirection = horizontalRotation * direction;

        _smokeMain.startSize = 1.5f * moveDirection.sqrMagnitude; // �ړ������ւ̗ʂɉ����č����T�C�Y�𐧌�
        // �ړ��w���̃x�N�g�������A�j���[�^�[�ɓn��
        _myAnim.SetFloat("Speed", moveDirection.magnitude);

        // ���͕����ֈړ�����
        transform.position += moveDirection * _moveSpeed * Time.fixedDeltaTime;
        // ���͕����ւ�������]����
        Vector3 LookDir = Vector3.Slerp(transform.forward, moveDirection, _rotationSpeed * Time.fixedDeltaTime);
        transform.LookAt(transform.position + LookDir);
    }
    private void Update()
    {
        if (_myCA.IsDead || Gamepad.current == null) return; // ���g������ł��� & �Q�[���p�b�g�����������牽�����Ȃ�

        // �m�F�p �ȉ��ύX���K�v
        // �x�{�^�������ŁA�񕜃G�t�F�N�g����������
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            _patHeal.Play();
            _myCA.ChangeHealth(_healValue);
        }
        // �k�o���p�[�����ŁA�_���[�W�G�t�F�N�g����������
        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            //OnDamage(); // �_���[�W�G�t�F�N�g��������
        }
        // �q�o���p�[�����ŁA��莞�Ԃ���������\������
        if (Gamepad.current.rightShoulder.wasPressedThisFrame && !_patStrong.activeSelf)
        {
            //StartCoroutine("StrongAction", _strongDuration);
        }


        // �U���{�^������������
        if (_confirmAction.InputAction.Player.Fire.WasPressedThisFrame())
        {
            //TODO: �U�����[�V�������ɍU���{�^�����󂯕t���Ȃ��悤�ɂ���@�A�ł���ƂQ��o�邩��
            _myAnim.SetTrigger("Attack"); // �U�����[�V�����̔���
            //_stand.Attack();// �U�����Y�[������
        }
        if (_confirmAction.InputAction.Player.Magic.WasPressedThisFrame())
        {

        }
    }

    /// <summary>
    /// ���S����
    /// </summary>
    public override IEnumerator OnDeath()
    {
        _myAnim.SetTrigger("Death"); // �_�E�����[�V��������
        yield return new WaitForSeconds(_birthInterval);
        ReBirth(); // �Đ�������\�� //HACK: Invoke���g���Ƃǂ�����Ăяo����Ă��邩�킩��Ȃ����ߕύX���K�v
    }
    /// <summary>
    /// �_���[�W��������
    /// </summary>
    public override void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // �_���[�W�G�t�F�N�g�𐶐�
        Fx.transform.position = transform.position + _damagePos; // �ʒu��␳
        Destroy(Fx, 1.0f); // 1.0�b��ɃG�t�F�N�g��j��
        StartCoroutine(Vibration(0.0f, 0.7f, 0.2f)); // �o�C�u���[�V����
    }
    /// <summary>
    /// �Đ�����
    /// </summary>
    private void ReBirth()
    {
        _myAnim.Rebind(); // �A�j���[�^�[�̏�����
        transform.position = Vector3.zero; // ���_�Ƀ��X�|�[��
        transform.rotation = Quaternion.identity; // ��]���������
        _myCA.Ready();
    }
    /// <summary>
    /// �p���[�A�b�v���䏈��
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    private IEnumerator StrongAction(float waitTime)
    {
        _patStrong.SetActive(true); // �L����
        _weaponAction.ChangePower(_strongValue);
        yield return new WaitForSeconds(_strongDuration);
        _patStrong.SetActive(false); // ������
        _weaponAction.ChangePower( -_strongValue);// ���ɂ����ł����}�C�i�X���t���Ă܂�
    }
    /// <summary>
    /// �o�C�u���[�V���������i��U���l,���U���l,�����b���j
    /// </summary>
    /// <param name="VibL"></param>
    /// <param name="VibR"></param>
    /// <param name="Duration"></param>
    /// <returns></returns>
    private IEnumerator Vibration(float VibL, float VibR, float Duration)
    {
        // �Q�[���p�b�g�̏ꍇ�U������
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(VibL, VibR);
            yield return new WaitForSeconds(Duration);
            Gamepad.current.SetMotorSpeeds(0, 0); // �o�C�u���[�V������~
        }
    }
    /// <summary>
    /// ���@�U��
    /// </summary>
    public void HomingMagic()
    {
        // �z�[�~���O�e���@�����
    }
//------------�A�j���[�V�����C�x���g-----------------------------
    /// <summary>
    /// �U���L����
    /// </summary>
    public override void AttackStart()
    {
        _weaponActions[0].WeaponActivate(true);// null���o��
    }
    /// <summary>
    /// �U��������
    /// </summary>
    public override void AttackFinish()
    {
        _weaponActions[0].WeaponActivate(false);
    }
//---------------------------------------------------------------
}