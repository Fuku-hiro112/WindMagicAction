using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;
using UnityEngine.Assertions;
using Unit;

[RequireComponent(typeof(Camera))]// Camera�R���|�[�l���g���K�v�@�Ȃ��ꍇ�͎����I�ɒǉ�
public class CameraMove : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _bias = 1.0f;
    [SerializeField] private float _distanceToPlayerM = 2f;    // �J�����ƃv���C���[�Ƃ̋���[m]
    [SerializeField] private float _slideDistanceM = 0f;       // �J���������ɃX���C�h������G�v���X�̎��E�ցC�}�C�i�X�̎�����[m]
    [SerializeField] private float _heightM = 1.2f;            // �����_�̍���[m]
    [SerializeField] private float _rotationSensitivity = 50f;// ���x
    private ConfirmAction _confirmAction;
    private UnitStats _unitStats;
    private Vector3 _lookAt;

    private void Reset()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        _unitStats = GameObject.FindGameObjectWithTag("Player")?.GetComponent<UnitStats>();
        _confirmAction = ConfirmAction.s_Instance;
        ResetPosition();// �J�����ʒu���Z�b�g

        Assert.IsNotNull(_target,    $"{this}�̃^�[�Q�b�g���ݒ肳��Ă��Ȃ��B");
        Assert.IsNotNull(_unitStats, $"{this}��_combatAction��Null�ł��B");
    }
    /// <summary>
    /// �J�����ʒu���Z�b�g
    /// </summary>
    public void ResetPosition()
    {
        _lookAt = _target.position + Vector3.up * _heightM;
        transform.position = _lookAt - transform.forward * _distanceToPlayerM;
    }
    void FixedUpdate()
    {

        Vector2 lookDerection = _confirmAction.LookDirection;
        float rotX = lookDerection.x * Time.fixedDeltaTime * _rotationSensitivity;
        float rotY = -lookDerection.y * Time.fixedDeltaTime * _rotationSensitivity;// ����͂������A�������������-(�}�C�i�X��t���Ă���)

        _lookAt = _target.position + Vector3.up * _heightM;// �J�����̒����_�i���S�_�j

        // ��]
        transform.RotateAround(_lookAt, Vector3.up, rotX);// ����]
        // �J�������v���C���[�̐^���^���ɂ���Ƃ��ɂ���ȏ��]�����Ȃ��悤�ɂ���
        if (transform.forward.y > 0.9f && rotY < 0 /*�^��*/|| 
            transform.forward.y < -0.9f && rotY > 0/*�^��*/) 
            rotY = 0;
        transform.RotateAround(_lookAt, transform.right, rotY);// �c��] 

        // �J�����ƃv���C���[�Ƃ̊Ԃ̋����𒲐�
        Vector3 targetPos = _lookAt - transform.forward * _distanceToPlayerM;
        //transform.position = Vector3.Lerp(transform.position, targetPos, _bias * Time.fixedDeltaTime);//HACK: �㉺���������������Ƃ��񂾂񏉊��ʒu�ɖ߂�
        
        // �����_�̐ݒ�
        transform.LookAt(_lookAt);

        // _slideDistanceM���A�J���������ɂ��炷
        transform.position = transform.position + transform.right * _slideDistanceM;
    }

    /* Assert.IsNotNull���Ă����֗��@�\���������̂ł���Ȃ��Ȃ����i; ;�j
    public static void ErrorLog<T>(T check, string message)
    {
        if (check == null)
        {
            Debug.LogError(message);
            Application.Quit();
        }
    }*/
}