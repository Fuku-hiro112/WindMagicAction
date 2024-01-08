using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // �VInput�V�X�e���̗��p�ɕK�v
using GameInput;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4.0f; // �ړ����x
    [SerializeField] private float _rotSpeed = 8.0f; // ��]���x
    [SerializeField] private float _strongDuration = 10.0f; // ��������
    [SerializeField] private int _strongValue = 10; // �����W��
    [SerializeField] private GameObject _patDamage; // �_���[�W�G�t�F�N�g
    [SerializeField] private float _birthInterval = 5.0f; // �Ēa���܂ł̎���
    [SerializeField] private int _healAmount = 50; // �񕜃w���X��
    [SerializeField] private GameObject _standObj; // Stand
    [SerializeField] private GameObject _swordWeapon;
    private Vector3 _damagePos = new Vector3(0, 1.5f, 0); // �_���[�W�G�t�F�N�g�̈ʒu
    private GameObject _patSmoke; // ���s�G�t�F�N�g
    private GameObject _patStrong; // �����G�t�F�N�g
    private ParticleSystem.MainModule _smokeMain; // ���s�����̖{��
    private ParticleSystem _patHeal; // �񕜃G�t�F�N�g
    private Animator _myAnim; // ���g�̃A�j���[�^�[
    private CombatAction _myCA; // ���g��CombatAction
    private StandAction _stand;
    private WeaponAction _swordAction;
    private ConfirmAction _confirmAction = ConfirmAction.s_Instance;

    void Start()
    {
        _patSmoke = transform.Find("PatSmoke").gameObject; // ���s�G�t�F�N�g���擾
        _patStrong = transform.Find("PatStrong").gameObject; // �����G�t�F�N�g���擾
        //Camera.main.TryGetComponent(out _confirmAction);
        _standObj.TryGetComponent(out _stand); // StandAction���擾
        _swordWeapon.TryGetComponent(out _swordAction);// 
        TryGetComponent(out _myAnim);// ���g�̃A�j���[�^�[���擾
        TryGetComponent(out _myCA); // ���g��CombatAction���擾
        transform.Find("PatHeal").TryGetComponent(out _patHeal); // �񕜃G�t�F�N�g���擾
        _smokeMain = _patSmoke.GetComponent<ParticleSystem>().main; // ���s�����̖{�̂��擾

        _patHeal.Stop(); // �񕜃G�t�F�N�g���~
        _patStrong.SetActive(false); // �����G�t�F�N�g�𖳌���
    }
    // �p���[�A�b�v���䏈��
    IEnumerator StrongAction(float waitTime)
    {
        _patStrong.SetActive(true); // �L����
        _swordAction.ChangePower(_strongValue);
        yield return new WaitForSeconds(_strongDuration);
        _patStrong.SetActive(false); // ������
        _swordAction.ChangePower( -_strongValue);// ���ɂ����ł����}�C�i�X���t���Ă܂�
    }
    // ���S����
    void OnDeath()
    {
        _myAnim.SetTrigger("Death"); // �_�E�����[�V��������
        Invoke("ReBirth", _birthInterval); // �Đ�������\��
    }
    // �Đ�����
    void ReBirth()
    {
        _myAnim.Rebind(); // �A�j���[�^�[�̏�����
        transform.position = Vector3.zero; // ���_�Ƀ��X�|�[��
        transform.rotation = Quaternion.identity; // ��]���������
        _myCA.Ready();
    }
    void FixedUpdate()
    {
        if (_myCA.IsDead) return; // ���g������ł��牽�����Ȃ�
        /*
        // ���W���C�X�e�B�b�N�̃x�N�g�����쐬
        Vector3 Dir = Vector3.zero;
        Dir.x = Gamepad.current.leftStick.ReadValue().x;
        Dir.z = Gamepad.current.leftStick.ReadValue().y;
        */
        Vector3 direction = _confirmAction.MoveDirection;

        _smokeMain.startSize = 1.5f * direction.sqrMagnitude; // �ړ������ւ̗ʂɉ����č����T�C�Y�𐧌�
        // �ړ��w���̃x�N�g�������A�j���[�^�[�ɓn��
        _myAnim.SetFloat("Speed", direction.magnitude);

        //if (Dir.sqrMagnitude < 0.01f) return; // �\���ɃW���C�X�e�B�b�N���|��Ă��Ȃ�����

        // ���͕����ֈړ�����
        transform.position += direction * _moveSpeed * Time.fixedDeltaTime;
        // ���͕����ւ�������]����
        Vector3 LookDir = Vector3.Slerp(transform.forward, direction, _rotSpeed * Time.fixedDeltaTime);
        transform.LookAt(transform.position + LookDir);
    }
    /// <summary>
    /// �_���[�W��������
    /// </summary>
    void OnDamage()
    {
        GameObject Fx = Instantiate(_patDamage); // �_���[�W�G�t�F�N�g�𐶐�
        Fx.transform.position = transform.position + _damagePos; // �ʒu��␳
        Destroy(Fx, 1.0f); // 1.0�b��ɃG�t�F�N�g��j��
        StartCoroutine(Vibration(0.0f, 0.7f, 0.2f)); // �o�C�u���[�V����
    }
    /// <summary>
    /// �o�C�u���[�V���������i��U���l,���U���l,�����b���j
    /// </summary>
    /// <param name="VibL"></param>
    /// <param name="VibR"></param>
    /// <param name="Duration"></param>
    /// <returns></returns>
    IEnumerator Vibration(float VibL, float VibR, float Duration)
    {
        Gamepad.current.SetMotorSpeeds(VibL, VibR);
        yield return new WaitForSeconds(Duration);
        Gamepad.current.SetMotorSpeeds(0, 0); // �o�C�u���[�V������~
    }
    // �U���L����
    public void AttackStart()
    {
        _swordAction.WeaponActivate(true);
    }
    // �U��������
    public void AttackFinish()
    {
        _swordAction.WeaponActivate(false);
    }
    void Update()
    {
        if (_myCA.IsDead || Gamepad.current == null) return; // ���g������ł��� & �Q�[���p�b�g�����������牽�����Ȃ�

        // �m�F�p
        // �x�{�^�������ŁA�񕜃G�t�F�N�g����������
        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            _patHeal.Play();
            _myCA.ChangeHealth(_healAmount);
        }
        // �k�o���p�[�����ŁA�_���[�W�G�t�F�N�g����������
        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            OnDamage(); // �_���[�W�G�t�F�N�g��������
        }
        // �q�o���p�[�����ŁA��莞�Ԃ���������\������
        if (Gamepad.current.rightShoulder.wasPressedThisFrame && !_patStrong.activeSelf)
        {
            StartCoroutine("StrongAction", _strongDuration);
        }

        if (_confirmAction.InputAction.Player.Fire.WasPressedThisFrame())
        {
            //TODO: �U�����[�V�������ɍU���{�^�����󂯕t���Ȃ��悤�ɂ���
            _myAnim.SetTrigger("Attack"); // �U�����[�V�����̔���
            _stand.Attack();
        }
    }
}