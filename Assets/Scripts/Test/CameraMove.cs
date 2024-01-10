using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInput;

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

    private void Reset()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Start()
    {
        if (_target == null)
        {
            Debug.LogError("�^�[�Q�b�g���ݒ肳��Ă��Ȃ�");
            Application.Quit();
        }
        _confirmAction = ConfirmAction.s_Instance;

        // �J�����ʒu���Z�b�g
        Vector3 lookAt = _target.position + Vector3.up * _heightM;
        transform.position = lookAt - transform.forward * _distanceToPlayerM;
    }

    void FixedUpdate()
    {
        Vector2 lookDerection = _confirmAction.LookDirection;
        float rotX = lookDerection.x * Time.fixedDeltaTime * _rotationSensitivity;
        float rotY = -lookDerection.y * Time.fixedDeltaTime * _rotationSensitivity;// ����͂������A�������������-(�}�C�i�X��t���Ă���)

        Vector3 lookAt = _target.position + Vector3.up * _heightM;// �J�����̒����_�i���S�_�j

        // ��]
        transform.RotateAround(lookAt, Vector3.up, rotX);// ����]
        // �J�������v���C���[�̐^���^���ɂ���Ƃ��ɂ���ȏ��]�����Ȃ��悤�ɂ���
        if (transform.forward.y > 0.9f && rotY < 0 /*�^��*/|| 
            transform.forward.y < -0.9f && rotY > 0/*�^��*/) 
            rotY = 0;
        transform.RotateAround(lookAt, transform.right, rotY);// �c��] 

        // �J�����ƃv���C���[�Ƃ̊Ԃ̋����𒲐�
        Vector3 targetPos = lookAt - transform.forward * _distanceToPlayerM;
        //transform.position = Vector3.Lerp(transform.position, targetPos, _bias * Time.fixedDeltaTime);//HACK: �㉺���������������Ƃ��񂾂񏉊��ʒu�ɖ߂�
        
        // �����_�̐ݒ�
        transform.LookAt(lookAt);

        // _slideDistanceM���A�J���������ɂ��炷
        transform.position = transform.position + transform.right * _slideDistanceM;
    }
}